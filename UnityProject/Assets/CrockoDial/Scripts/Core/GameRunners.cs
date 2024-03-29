﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System;


#region STARTUP OPTIONS TYPES -------------------------------------------------------------
[System.Serializable]
public class GameLaunchSettings
{
    #region SERIALIZED------------------------------------------------
        public GameLaunchType type = GameLaunchType.Unity;
        public UnityExeRunner unityStartupOptions = new UnityExeRunner();
        public GenericExeRunner genericStartupOptions = new GenericExeRunner();
        public WebsiteRunner websiteStartupOptions = new WebsiteRunner();
        public float joyToKeyConfigDelay = -1;
        public string launchHelperScriptPath = "";
 
    [SerializeField]
    public MouseStartUpOptions mouseStartupOptions;
    #endregion -------------------------------------------------------
    public string launchHelperScriptPathAbsolute => string.IsNullOrEmpty(launchHelperScriptPath) ? "" : Path.Combine(_srcGame.rootFolder.FullName, this.launchHelperScriptPath);


    #region NON-SERIALIZED-----------------------------------------
    [System.NonSerialized]
    private GameData _srcGame;
    #endregion -------------------------------------------------------

    public bool joyToKeyDelayed => joyToKeyConfigDelay > 0;
    public IGameRunner Runner()
    {
        if (type == GameLaunchType.Generic)
        {
            return this.genericStartupOptions;
        }
        else if (type == GameLaunchType.Unity)
        {
            return unityStartupOptions;
        }
        else if (type == GameLaunchType.Website)
        {
            return websiteStartupOptions;
        }
        return null;
    }


    public void Audit(System.Text.StringBuilder auditMsgStringBuilder)
    {
        if (type == GameLaunchType.Unity)
        {
            if (unityStartupOptions.hasResolutionSetupScreen && string.IsNullOrEmpty(this._srcGame.windowTitle))
            {
                auditMsgStringBuilder.Append(_srcGame.title);
                auditMsgStringBuilder.Append(" needs a valid window title to skip its resultion dialog");
            }
        }
    }

    internal void SetUpWithGame(GameData gameData)
    {
        _srcGame = gameData;
        this.unityStartupOptions.SetUpWithGame(gameData);
        this.genericStartupOptions.SetUpWithGame(gameData);
        this.websiteStartupOptions.SetUpWithGame(gameData);
    }
}

public enum GameLaunchType
{
    Generic = 0,
    Unity = 1,
    Website = 2,
}

public interface IGameRunner
{
    //string GetCommandLineArgs();
    void SetUpWithGame(GameData game);
    void Launch();
    void RunningUpdate();
    void Reset();
    void FocusWindow();
    Process process
    {
        get;
    }
}

public abstract class AbstractGameRunner : IGameRunner
{

    [NonSerialized]
    protected GameData _srcGame;


    protected bool _waitingForMainGameWindow = true;
    float _timeSinceWindowAppeared = -1;

    public void SetUpWithGame(GameData game)
    {
        _srcGame = game;
    }

    public Process process
    {
        get;
        protected set;
    }



    public abstract void Launch();

    public void LaunchHelperProcess()
    {
        if (!string.IsNullOrEmpty(_srcGame.launchSettings.launchHelperScriptPath))
        {
            bool isAutoItScript = System.IO.Path.GetExtension(_srcGame.launchSettings.launchHelperScriptPath).ToLower() == ".au3";
            if (isAutoItScript)
            {
                ProcessRunner.StartProcess(
                    Path.GetDirectoryName(_srcGame.launchSettings.launchHelperScriptPathAbsolute), 
                    CompanionSoftware.AutoIt,
                    Path.GetFileName(_srcGame.launchSettings.launchHelperScriptPathAbsolute)
                    );
            }
            else
            {
                string startDir = _srcGame.launchSettings.launchHelperScriptPathAbsolute;
                ProcessRunner.StartProcess(Path.GetDirectoryName(startDir), Path.GetFileName(startDir), "");
            }
        }
    }

    public virtual void Reset()
    {
        _timeSinceWindowAppeared = 0;
        _waitingForMainGameWindow = !string.IsNullOrEmpty(_srcGame.windowTitle);
        process = null;
    }
    public virtual void RunningUpdate()
    {
        if (_waitingForMainGameWindow)
        {
            if (ExternalWindowTracker.WindowIsPresent(_srcGame.windowTitle))
            {
                Debug.Log("Main Game Window Appeared");
                _waitingForMainGameWindow = false;
                _srcGame.launchSettings.mouseStartupOptions?.Perform();
            }
        }

        if (!_waitingForMainGameWindow)
        {
            float prevTime = _timeSinceWindowAppeared;
            _timeSinceWindowAppeared += Time.deltaTime;

            if (_srcGame.launchSettings.joyToKeyDelayed)
            {
                bool joyToKeyDelayReached = prevTime < _srcGame.launchSettings.joyToKeyConfigDelay && _timeSinceWindowAppeared >= _srcGame.launchSettings.joyToKeyConfigDelay;
                if (joyToKeyDelayReached)
                {
                    Debug.Log("Joy to key delay reached!");
                    ProcessRunner.instance.setJoyToKeyConfig(_srcGame.joyToKeyConfig);
                }
            }

        }

    }

    public virtual void FocusWindow()
    {
        GameData currentlySelectedGame = this._srcGame;
        string windowTitle = currentlySelectedGame.windowTitle;
        

        if (string.IsNullOrEmpty(windowTitle))
        {
            ExternalWindowTracker.ForceBringToForeground(ExternalWindowTracker.runningPrimaryWindowGuess(process.Id));//currentGameRunner.process.Id));
        }
        else
        {
            ExternalWindowTracker.ForceBringToForeground(ExternalWindowTracker.GetWindowByTitle(windowTitle));
        }


        //    /// Alt way <<<<<<<<<<<<<<<<
        //    SendKeyStrokesToWindow(windowTitle);
        //    //>>>>>>>>>>>>>>>>
    }
}


[System.Serializable]
public class UnityExeRunner : AbstractGameRunner
{
    #region SERIALIZED --------------------------------------------
    public bool hasResolutionSetupScreen = false;
    #endregion --------------------------------------------------------

    #region NON-SERIALIZED --------------------------------------------
    #endregion --------------------------------------------------------

    public string ResulotionDialogWindowTitle => _srcGame.windowTitle + " Configuration";

    public static class CommonArgs
    {
        public const string _1080pFullscreenArgs = "-screen-fullscreen 1 -screen-width 1920 -screen-height 1080 "; //-single-instance
        public const string _hasResolutionDialogArgs = "";
    }

    public string GetCommandLineArgs()
    {
        if (!this.hasResolutionSetupScreen)
        {
            return CommonArgs._1080pFullscreenArgs;
        }
        return "";
    }

    public override void Launch()
    {
        //------RESET STATE --------
        Reset();

  
        string startDir = Path.Combine(this._srcGame.rootFolder.FullName, this._srcGame.exePath);
        Process ret;
        if (hasResolutionSetupScreen)
        {
            //start with other args
            //_dialogWaitTimer.Restart(.25f);
            ret = ProcessRunner.StartProcess(Path.GetDirectoryName(startDir), Path.GetFileName(startDir), CommonArgs._hasResolutionDialogArgs);
        }
        else
        {
            if (!_waitingForMainGameWindow && _srcGame.launchSettings.mouseStartupOptions != null)
            {
                Debug.Log("Performing mouse routine");
                _srcGame.launchSettings.mouseStartupOptions.Perform();
            }
            //start with args normally
             ret = ProcessRunner.StartProcess(Path.GetDirectoryName(startDir), Path.GetFileName(startDir), CommonArgs._1080pFullscreenArgs);
        }

        LaunchHelperProcess();
        process = ret;
    }

    enum DialogSkipState
    {
        WaitingForDialogToAppear,
        DialogHasAppeared,
        DialogHasClosed
    }
    DialogSkipState _resDiaSkipState = DialogSkipState.WaitingForDialogToAppear;



    float _lastResDialogKeySendTimer = float.MaxValue;

    void SendKeyToResDialog()
    {
        _lastResDialogKeySendTimer = 0;
        ProcessRunner.SendKeyStrokesToWindow(this.ResulotionDialogWindowTitle, "{ENTER}");
    }

    public override void RunningUpdate()
    {
        base.RunningUpdate();
        if (hasResolutionSetupScreen)
        {
            if (_resDiaSkipState == DialogSkipState.WaitingForDialogToAppear)
            {
                if (ExternalWindowTracker.WindowIsPresent(this.ResulotionDialogWindowTitle))
                {
                    _resDiaSkipState = DialogSkipState.DialogHasAppeared;
                }
            }
            else if (_resDiaSkipState == DialogSkipState.DialogHasAppeared)
            {
                _lastResDialogKeySendTimer += Time.deltaTime;
                if (!ExternalWindowTracker.WindowIsPresent(this.ResulotionDialogWindowTitle))
                {
                    _resDiaSkipState = DialogSkipState.DialogHasClosed;
                }
                else if (_lastResDialogKeySendTimer > .5f)
                {
                    Debug.Log("Sending");
                    SendKeyToResDialog();
                }
            }
            //else 
            //{
            //    if (ProcessRunner.WindowIsPresent(this.ResulotionDialogWindowTitle) && _dialogWaitTimer.expired)
            //    {
            //        _dialogWaitTimer.Restart();
            //        needToSendKeys = true;
            //    }

            //}

        }
    }

    public override void Reset()
    {
        base.Reset();
        //------RESET STATE --------
        _resDiaSkipState = DialogSkipState.WaitingForDialogToAppear;
        
        //--------------------------
    }
}

[System.Serializable]
public class GenericExeRunner : AbstractGameRunner
{
    #region SERIALIZED ------------------------------------------------
    public string commandLineArguments = string.Empty;
    #endregion --------------------------------------------------------

    #region NON-SERIALIZED --------------------------------------------
    #endregion --------------------------------------------------------

    public override void Launch()
    {
        Reset();
        string startDir = Path.Combine(this._srcGame.rootFolder.FullName, this._srcGame.exePath);
        LaunchHelperProcess();
        process = ProcessRunner.StartProcess(Path.GetDirectoryName(startDir), Path.GetFileName(startDir), commandLineArguments);
    }

    public override void RunningUpdate()
    {
        base.RunningUpdate();
    }

}
#endregion

[System.Serializable]
public class WebsiteRunner : AbstractGameRunner
{
    #region SERIALIZED ------------------------------------------------
    public string url = string.Empty;

    public override void Launch()
    {
        /*Reset();
        string startDir = Path.Combine(this._srcGame.rootFolder.FullName, this._srcGame.exePath);
        
        process = ProcessRunner.StartProcess(Path.GetDirectoryName(startDir), Path.GetFileName(startDir), commandLineArguments);*/
        LaunchHelperProcess();
        process = ProcessRunner.StartProcess(CompanionSoftware.BrowserLauncher, url);


    }

    public override void RunningUpdate()
    {
        //don't do anything!
        //base.RunningUpdate();
    }

    public override void FocusWindow()
    {
        //also don't do anything!
        //base.FocusWindow();
    }
    #endregion --------------------------------------------------------
}
#region MOUSE
[System.Serializable]
public class MouseStartUpOptions
{
    public float extraStartDelay = 0;
    public AutoMouseEvent[] startUpRoutine;




    public void Perform()
    {
        ProcessRunner.instance?.StartCoroutine(routine());
    }

    IEnumerator routine()
    {
        if (startUpRoutine != null)
        {
            yield return new WaitForSeconds(extraStartDelay);
            foreach (AutoMouseEvent ame in startUpRoutine)
            {
                yield return new WaitForSeconds(ame.delay);
                ProcessRunner.SetMousePosition((int)ame.position.x, (int)ame.position.y);
                if (ame.clickType == AutoMouseEvent.ClickEventType.leftClick)
                {
                    yield return new WaitForSeconds(.05f);
                    ProcessRunner.MouseClick();
                }
            }
        }
    }
}

[System.Serializable]
public class AutoMouseEvent
{
    public enum ClickEventType
    {
        None = 0, leftClick = 1
    }
    public ClickEventType clickType;
    public float delay;
    public Vector2 position;

    public AutoMouseEvent(Vector2 position, ClickEventType clickType, float delay)
    {
        this.clickType = clickType;
        this.delay = delay;
        this.position = position;
    }
}

#endregion