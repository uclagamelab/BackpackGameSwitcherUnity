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
    bool _foundResolutionWindow = false;
    XUTimer _dialogWaitTimer = new XUTimer();

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
        string startDir = Path.Combine(this._srcGame.rootFolder.FullName, this._srcGame.exePath);
        if (hasResolutionSetupScreen)
        {
            //start with other args
            _dialogWaitTimer.Restart(DialogWaitDuration);
            return ProcessRunner.StartProcess(Path.GetDirectoryName(startDir), Path.GetFileName(startDir), CommonArgs._hasResolutionDialogArgs);
        }
        else
        {
            //start with args normally
            return ProcessRunner.StartProcess(Path.GetDirectoryName(startDir), Path.GetFileName(startDir), CommonArgs._1080pFullscreenArgs);
        }
    }

    public override void RunningUpdate()
    {
        if (hasResolutionSetupScreen && !_foundResolutionWindow)
        {
            if (ProcessRunner.WindowIsPresent(this.ResulotionDialogWindowTitle))//_dialogWaitTimer.expired)
            {
                _foundResolutionWindow = true;
                _dialogWaitTimer.Stop();
                //Send keys
                ProcessRunner.SendKeyStrokesToWindow(this.ResulotionDialogWindowTitle, "{ENTER}");
            }
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

