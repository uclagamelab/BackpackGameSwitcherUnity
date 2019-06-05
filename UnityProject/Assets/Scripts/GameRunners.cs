using System.Collections;
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
    #endregion -------------------------------------------------------

    #region NON-SERIALIZED-----------------------------------------
    GameData _srcGame;
    #endregion -------------------------------------------------------

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
    }
}

public enum GameLaunchType
{
    Generic = 0,
    Unity = 1
}

public interface IGameRunner
{
    //string GetCommandLineArgs();
    void SetUpWithGame(GameData game);
    Process Launch();
    void RunningUpdate();
    void LaunchCleanUp();
}

public abstract class AbstractGameRunner : IGameRunner
{
    [System.NonSerialized]
    protected GameData _srcGame;

    [SerializeField]
    public MouseStartUpOptions mouseStartupOptions;


    public void SetUpWithGame(GameData game)
    {
        _srcGame = game;
    }

    public abstract Process Launch();
    public abstract void LaunchCleanUp();
    public abstract void RunningUpdate();
}


[System.Serializable]
public class UnityExeRunner : AbstractGameRunner
{
    #region SERIALIZED --------------------------------------------
    public bool hasResolutionSetupScreen = false;
    #endregion --------------------------------------------------------

    #region NON-SERIALIZED --------------------------------------------
    //XUTimer _dialogWaitTimer = new XUTimer();

    static float DialogWaitDuration = 5;
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

    public override Process Launch()
    {
        //------RESET STATE --------
        _resDiaSkipState = DialogSkipState.WaitingForDialogToAppear;
        _waitingForMainGameWindow = !string.IsNullOrEmpty(_srcGame.windowTitle);
        //--------------------------

        string startDir = Path.Combine(this._srcGame.rootFolder.FullName, this._srcGame.exePath);
        if (hasResolutionSetupScreen)
        {
            //start with other args
            //_dialogWaitTimer.Restart(.25f);
            return ProcessRunner.StartProcess(Path.GetDirectoryName(startDir), Path.GetFileName(startDir), CommonArgs._hasResolutionDialogArgs);
        }
        else
        {
            if (!_waitingForMainGameWindow)
            {
                Debug.Log("DING GDDO");
                mouseStartupOptions?.Perform();
            }
            //start with args normally
            return ProcessRunner.StartProcess(Path.GetDirectoryName(startDir), Path.GetFileName(startDir), CommonArgs._1080pFullscreenArgs);
        }
    }

    enum DialogSkipState
    {
        WaitingForDialogToAppear,
        DialogHasAppeared,
        DialogHasClosed
    }
    DialogSkipState _resDiaSkipState = DialogSkipState.WaitingForDialogToAppear;
    bool _waitingForMainGameWindow = true;

    float _lastResDialogKeySend = float.NegativeInfinity;

    void SendKeyToResDialog()
    {
        _lastResDialogKeySend = Time.time;
        ProcessRunner.SendKeyStrokesToWindow(this.ResulotionDialogWindowTitle, "{ENTER}");
    }

    public override void RunningUpdate()
    {
        if (_waitingForMainGameWindow)
        {
            if (ProcessRunner.WindowIsPresent(_srcGame.windowTitle))
            {
                Debug.Log("Main Game Window Appeared");
                _waitingForMainGameWindow = false;
                mouseStartupOptions?.Perform();
            }
        }

        if (hasResolutionSetupScreen)
        {
            if (_resDiaSkipState == DialogSkipState.WaitingForDialogToAppear)
            {
                if (ProcessRunner.WindowIsPresent(this.ResulotionDialogWindowTitle))
                {
                    _resDiaSkipState = DialogSkipState.DialogHasAppeared;
                }
            }
            else if (_resDiaSkipState == DialogSkipState.DialogHasAppeared)
            {
                if (!ProcessRunner.WindowIsPresent(this.ResulotionDialogWindowTitle))
                {
                    _resDiaSkipState = DialogSkipState.DialogHasClosed;
                }
                else if (Time.time - _lastResDialogKeySend > .5f)
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

    public override void LaunchCleanUp()
    {
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

    public override Process Launch()
    {
        string startDir = Path.Combine(this._srcGame.rootFolder.FullName, this._srcGame.exePath);
        return ProcessRunner.StartProcess(Path.GetDirectoryName(startDir), Path.GetFileName(startDir), commandLineArguments);
    }

    public override void RunningUpdate()
    {
    }

    public override void LaunchCleanUp()
    {
    }
}
#endregion

[System.Serializable]
public class MouseStartUpOptions
{
    public float extraStartDelay = 0;
    public AutoMouseEvent[] startUpRoutine;

    [System.Serializable]
    public class AutoMouseEvent
    {
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

    public enum ClickEventType
    {
        None = 0, leftClick = 1
    }

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
                if (ame.clickType == ClickEventType.leftClick)
                {
                    yield return new WaitForSeconds(.05f);
                    ProcessRunner.MouseClick();
                }
            }
        }
    }
}


