/*

This class, (aspirationally) is contains functions for...
- changing focus,
- Starting new exes/bat files etc...
- stopping existing processings

TODO: factor out dll stuff
 */

//https://msdn.microsoft.com/en-us/library/windows/desktop/ms632668(v=vs.85).aspx


using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
//using System.Runtime.InteropServices;
using System.IO;
using Debug = UnityEngine.Debug;
using static WinOsUtil;
using Microsoft.Win32;

public class ProcessRunner : MonoBehaviour
{
    public static bool SwitcherAppHasFocus { get; private set; } = true;

    public class EventCallbacks
    {
        public System.Action OnProcessLaunched = () => { };
        public System.Action OnProcessExited = () => { };
    }

    public static EventCallbacks Events = new EventCallbacks();

    //Add startup forgiveness timer, for games that change window

    bool _gameRunningLastFrame = false;
    public bool IsGameRunning()
    {
        bool processJustStarted = _runningGame != null && currentProcessStartTimer < 5;
        bool windowBasedGameIsRunning = _runningGame != null && !string.IsNullOrEmpty(_runningGame.windowTitle) && ExternalWindowTracker.WindowIsPresent(_runningGame.windowTitle);
        bool processBasedGameIsRunning = _runningGame != null && (currentGameRunner.process != null && !currentGameRunner.process.HasExited);
        return processJustStarted || windowBasedGameIsRunning || processBasedGameIsRunning;
    }

    static XUSingleTown<ProcessRunner> _instanceHelper = new XUSingleTown<ProcessRunner>();
    public static ProcessRunner instance => _instanceHelper.instance;

    float currentProcessStartTimer = 0;

    List<Process> safeProcesses;

    Process _thisProcess = Process.GetCurrentProcess(); //The application switcher process?
    GameData _runningGame = null;
    IGameRunner currentGameRunner => _runningGame?.launchSettings.Runner();
    //Process _currentRunningGameProcess = null; //the currently running game process
    Process _joy2KeyProcess = null;

    IntPtr _joy2KeyPrimaryWindow = IntPtr.Zero;

    System.Text.StringBuilder _sb = new System.Text.StringBuilder();

    // Pressing the button opens up the game.
    // Pressing CTRL - C brings this game to the foreground

    bool _usingJoyToKey = true;

    IEnumerator Start()
    {
        if (SwitcherSettings.Data._controlMode == CrockoInputMode.mouseAndKeyboard)
        {
            _usingJoyToKey = false;
        }

        determineSwitcherJoyToKeyConfigFile();
        setJoyToKeyConfig(switcherJoyToKeyConfig);

        //Not sure if delay is actually necessary
        yield return new WaitForSeconds(2);
        recordSafeProcesses();
        if (SwitcherSettings.Data._ShutDownExplorerWhileRunning)
        {
#if UNITY_EDITOR
            Debug.Log("<Shutdown of explorer would happen here>");
#else
            SetWindowsExplorerRunning(false);
#endif
        }
    }

    private void Update()
    {
        var gameRunningNow = IsGameRunning();
        if (_gameRunningLastFrame && !gameRunningNow)
        {
            Events.OnProcessExited.Invoke();
        }
        _gameRunningLastFrame = gameRunningNow;

        if (_runningGame != null)
        {
            _runningGame.launchSettings.Runner().RunningUpdate();
        }

        if (gameRunningNow)
        {
            currentProcessStartTimer += Time.deltaTime;
        }
    }

    public static Process StartProcess(string exe, string cmdArgs)
    {
        return StartProcess(null, exe, cmdArgs);
    }
    public static Process StartProcess(string directory, string exe, string cmdArgs)
    {

        //this is a little weird...
        string fullPath = exe;
        if (directory != null)
        {
            fullPath = Path.Combine(directory, exe);
        }

        if (!File.Exists(exe) && !File.Exists(fullPath))
        {
            Debug.LogError("Game not found at path:\n" + fullPath);
            return null;
        }

        ProcessStartInfo startInfo = new ProcessStartInfo();
        // --- the working settings -----------------
        startInfo.CreateNoWindow = false;
        startInfo.WindowStyle = ProcessWindowStyle.Normal;

        //-------------------------------------------------
        /*if (cmdArgs != null)
        { 
            startInfo.UseShellExecute = true;
        }*/
        /*
        //In the old code.... before else branch containing above "working settings"
        if (cmdArgs != null && cmdArgs.Length == 0) {
            UnityEngine.Debug.Log("no unity");
            startInfo.UseShellExecute = true;
        }*/


        if (directory != null)
        {
            startInfo.WorkingDirectory = directory; //"C:\\Users\\Garrett Johnson\\Desktop";
        }

        startInfo.FileName = exe;               //"angryBots.exe";
        if (cmdArgs != null)
        {
            startInfo.Arguments = cmdArgs; 			//"-popupwindow -screen-width 1920 -screen-height 1080";
        }

        return Process.Start(startInfo);
    }

    [ContextMenu("restartExplorer")]
    public void restartExplorer()
    {
        SetWindowsExplorerRunning(true);
    }

    [ContextMenu("kill explorer")]
    public void killExplorer()
    {
        SetWindowsExplorerRunning(false);
    }

    bool _haveKilledExplorer = false;
    public void SetWindowsExplorerRunning(bool on)
    {
        if (on)
        {
            string targetExe = "explorer";
            Process[] processes = Process.GetProcessesByName(targetExe);

            bool shouldRestart = true;

            foreach (var p in processes)
            {
                shouldRestart &= p.HasExited;
            }

            if (shouldRestart)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "explorer.exe";
                Process.Start(startInfo);
            }
        }
        else
        {
            _haveKilledExplorer = true;
            if (IsWindows11)
            {
                //I suspect this method is probably fine also for previous versions of windows, may ask tyler to test.
                var psinfo = new ProcessStartInfo();
                psinfo.WindowStyle = ProcessWindowStyle.Hidden;
                psinfo.FileName = "taskkill";
                psinfo.Arguments = "/f /im explorer.exe";
                Process.Start(psinfo);
            }
            else
            {
                string targetExe = "explorer";
                Process[] processes = Process.GetProcessesByName(targetExe);
                foreach (Process p in processes)
                {
                    bool canCheck = !p.HasExited;
                    if (canCheck)
                    {
                        p.CloseMainWindow();
                    }
                }
            }
        }
    }

    public bool IsWindows11 => SystemInfo.operatingSystem.StartsWith("Windows 11");

    public void StartGame(GameData gameToStart)
    {
        //ProcessRunner.instance.OpenProcess(gameToStart.directory, gameToStart.appFile, ""/*currentGameData.commandLineArguments*/, currentGameData.joyToKeyConfig);

        //_currentRunningGame = gameToStart.launchSettings.Runner();//StartProcess(gameToStart.directory, gameToStart.appFile, ""/*currentGameData.commandLineArguments*/);
        _runningGame = gameToStart;
        currentGameRunner.Launch();


        //IDEALLY: this would be better folded into the process runner, but whatever for now
        if (!_runningGame.launchSettings.joyToKeyDelayed)
        {
            ProcessRunner.instance.setJoyToKeyConfig(_runningGame.joyToKeyConfig);
        }

        currentProcessStartTimer = 0;

        Events.OnProcessLaunched.Invoke();
    }

    /*
    The algorithm... as is... 
    assumes when the process is started, the correct window indeed comes to the front.
    then you can save that window.  
    */

    public void BringThisToForeground()
    {
        setJoyToKeyConfigIfNotAlreadySet(switcherJoyToKeyConfig);
        string switcherWindowName = Application.productName;

#if UNITY_EDITOR
        if (ExternalWindowTracker.EditorFocusStealing)
        {
            switcherWindowName = ExternalWindowTracker.editorWindowTitle;
            //Debug.LogError(switcherWindowName);
        }
#endif

        ExternalWindowTracker.ForceBringToForeground(ExternalWindowTracker.GetWindowByTitle(switcherWindowName));
    }

    public void quitCurrentGame()
    {
        StopCurrentRunningGame();
        BringThisToForeground();
    }

    public void BringRunningToForeground()
    {
        setJoyToKeyConfigIfNotAlreadySet(_runningGame.joyToKeyConfig);
        currentGameRunner?.FocusWindow();
    }

    static string lastSentWindow = null;
    static string lastSentKeyStroke = null;
    static string cachedSendKeyCommand;
    static System.Text.StringBuilder sendKeyStrokesCommandBuilder = new System.Text.StringBuilder();
    
    //TODO: this, and the bat file can probably be replaced with the internal implementation
    public static void SendKeyStrokesToWindow(string windowTitle, string key = "")
    {
        //string cmdText = "call \"" +  Application.streamingAssetsPath + "\\~Special\\sendKeys.bat\" \"" + windowTitle + "\" \"" + key + "\"";
        if (windowTitle != lastSentWindow || key != lastSentKeyStroke)
        {
            sendKeyStrokesCommandBuilder.Clear();
            sendKeyStrokesCommandBuilder.Append("/C call \"");
            sendKeyStrokesCommandBuilder.Append(Application.streamingAssetsPath);
            sendKeyStrokesCommandBuilder.Append("\\~Special\\sendKeys.bat\" \"");
            sendKeyStrokesCommandBuilder.Append(windowTitle);
            sendKeyStrokesCommandBuilder.Append("\" \"");
            sendKeyStrokesCommandBuilder.Append(key);
            sendKeyStrokesCommandBuilder.Append("\"");
            cachedSendKeyCommand = sendKeyStrokesCommandBuilder.ToString();
        }

        //Debug.Log(cmdBuilder.ToString());
        Process process = new System.Diagnostics.Process();
        ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        startInfo.FileName = "cmd.exe";
        startInfo.Arguments = cachedSendKeyCommand;
        process.StartInfo = startInfo;
        process.Start();
    }

    static System.Text.StringBuilder _mousePosCommandBuilderCmdBuilder = new System.Text.StringBuilder();
    public static void SetMousePosition(int x, int y, bool absolute = true)
    {
        _mousePosCommandBuilderCmdBuilder.Clear();
        _mousePosCommandBuilderCmdBuilder.Append("/C call \"");
        _mousePosCommandBuilderCmdBuilder.Append(Application.streamingAssetsPath);
        _mousePosCommandBuilderCmdBuilder.Append("\\~Special\\mouse.bat\" moveTo ");
        _mousePosCommandBuilderCmdBuilder.Append(x);
        _mousePosCommandBuilderCmdBuilder.Append('x');
        _mousePosCommandBuilderCmdBuilder.Append(y);

#if UNITY_EDITOR
        Debug.Log(_mousePosCommandBuilderCmdBuilder.ToString());
#endif
        Process process = new System.Diagnostics.Process();
        ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        startInfo.FileName = "cmd.exe";
        startInfo.Arguments = _mousePosCommandBuilderCmdBuilder.ToString();
        process.StartInfo = startInfo;
        process.Start();
    }

    public static void MouseClick()
    {
        _mousePosCommandBuilderCmdBuilder.Clear();
        _mousePosCommandBuilderCmdBuilder.Append("/C call \"");
        _mousePosCommandBuilderCmdBuilder.Append(Application.streamingAssetsPath);
        _mousePosCommandBuilderCmdBuilder.Append("\\~Special\\mouse.bat\" click");

        Debug.Log(_mousePosCommandBuilderCmdBuilder.ToString());
        Process process = new System.Diagnostics.Process();
        ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        startInfo.FileName = "cmd.exe";
        startInfo.Arguments = _mousePosCommandBuilderCmdBuilder.ToString();
        process.StartInfo = startInfo;
        process.Start();
    }

    public void StopCurrentRunningGame()
    {
        if (currentGameRunner != null)
        {
            //UnityEngine.Debug.Log("CloseProcess called : " + _currentRunningGameProcess.Id);

            KillAllNonSafeProcesses();
        }
        currentGameRunner?.Reset();
        _runningGame = null;
    }

    void recordSafeProcesses()
    {
        if (this.safeProcesses == null)
        {
            this.safeProcesses = new List<Process>();
        }
        this.safeProcesses.Clear();

        Process[] processes = Process.GetProcesses();
        foreach (Process p in processes)
        {
            this.safeProcesses.Add(p);
        }
    }

    [ContextMenu("TryKillNamedExe")]
    public void TryKillNamedExe()
    {
        TryKillNamedExe(CompanionSoftware.Rainmeter);
    }
    public static void TryKillNamedExe(string targetExe)
    {
        Process[] processes = Process.GetProcesses();
        foreach (Process p in processes)
        {
            bool canCheck = !p.HasExited;
            if (canCheck && p.MainModule.FileName == targetExe)
            {
                Debug.Log(p.MainModule.FileName);
                p.Kill();
            }
        }
    }

    private void OnApplicationQuit()
    {
#if !UNITY_EDITOR
        if (_haveKilledExplorer || SwitcherSettings.Data._ShutDownExplorerWhileRunning)
        {
            SetWindowsExplorerRunning(true);
        }
#endif
        TerminateProcess(_joy2KeyProcess);
    }

    public void KillAllNonSafeProcesses(int exitCode = 0)
    {
        Process[] processes = Process.GetProcesses();
        foreach (Process p in processes)
        {
            if (!isSafeProcess(p))
            {
                try
                {
                    WinOsUtil.TerminateProcess((uint)p.Handle, exitCode);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }
    }

    public static void TerminateProcess(Process p, int exitCode = 0)
    {
        if (p != null && !p.HasExited)
        {
            WinOsUtil.TerminateProcess((uint)p.Handle, exitCode);
        }
    }

    bool isSafeProcess(Process proc)
    {
        foreach (Process sp in safeProcesses)
        {
            if (sp.Id == proc.Id)
            {
                return true;
            }
        }
        return false;
    }

    void OnApplicationFocus(bool hasFocus)
    {
        SwitcherAppHasFocus = hasFocus;
    }

    #region Joy To Key
    private string _currentJoyToKeyConfig = null;
    public const string DEFAULT_SWITCHER_JOYTOKEY_CONFIG = "menu_select.cfg";
    private string switcherJoyToKeyConfig = DEFAULT_SWITCHER_JOYTOKEY_CONFIG;

    void determineSwitcherJoyToKeyConfigFile()
    {
        switcherJoyToKeyConfig = DEFAULT_SWITCHER_JOYTOKEY_CONFIG;
        var settingsVal = SwitcherSettings.Data._JoyToKeyMenuConfig;
        if (!string.IsNullOrEmpty(settingsVal))
        {
            switcherJoyToKeyConfig = settingsVal;
        }
    }

    void setJoyToKeyConfigIfNotAlreadySet(string configFile)
    {
        if (configFile != _currentJoyToKeyConfig)
        {
            setJoyToKeyConfig(configFile);
        }
    }

    bool joyToKeyProfileExists(string profile)
    {
        return
            !string.IsNullOrEmpty(profile)
            &&
            !string.IsNullOrEmpty(SwitcherSettings.Data.JoyToKeyFolder)
            &&
            System.IO.File.Exists(Path.Combine(SwitcherSettings.Data.JoyToKeyFolder, profile));
    }

    public void setJoyToKeyConfig(string configFile)
    {
        if (!_usingJoyToKey)
        {
            return;
        }

        _currentJoyToKeyConfig = configFile;

        string joyToKeyExe = CompanionSoftware.JoyToKey;

        if (!System.IO.File.Exists(joyToKeyExe))
        {
            UnityEngine.Debug.LogError("JoyToKey executable not available at '" + joyToKeyExe + "'");
            return;
        }

        ProcessStartInfo startInfo = new ProcessStartInfo();

        startInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(joyToKeyExe); //"C:\\Users\\Garrett Johnson\\Desktop";
        startInfo.FileName = System.IO.Path.GetFileName(joyToKeyExe); ;
        startInfo.Arguments = configFile;//Path.GetFileNameWithoutExtension(exe);


        var newProcess = Process.Start(startInfo);
        if (_joy2KeyProcess == null || _joy2KeyProcess.HasExited)
        {
            _joy2KeyProcess = newProcess;
        }
    }
    #endregion


    /*[ContextMenu("TESTREMOVE")]
    public void TESTREMOVEBORDER()
    {

        if (_allWindowsCached.ContainsKey("New Unity Project"))
        {
            IntPtr window = _allWindowsCached["New Unity Project"];
            //SetWindowPos(window, 0, 0, 0, 1920, 1080, SWP_SHOWWINDOW);


            //// --- WORKING FOR OLDER UNITY BUILD
            SetWindowPos(window, 0, 0, 0, 1920, 1080, SWP_SHOWWINDOW);
            SetWindowLong(window, GWL_STYLE, WS_POPUP);
            SetWindowPos(window, 0, 0, 0, 1920, 1080,
             SWP_NOMOVE | SWP_NOZORDER | SWP_NOSIZE | SWP_FRAMECHANGED | SWP_SHOWWINDOW);
            ////--------------------------
            // --- MORE LIK UE4

            //SetWindowLong(window, GWL_STYLE, WS_POPUP);
            //SetWindowPos(window, 0, 0, 0, 1920, 1080,
            // SWP_NOMOVE | SWP_NOZORDER | SWP_NOSIZE | SWP_FRAMECHANGED | SWP_SHOWWINDOW);
            //this.delayedFunction(() =>
            //    {
            //        SetWindowPos(window, 0, 0, 0, 1920, 1080, SWP_FRAMECHANGED);
            //    }, 1);

            //--------------------------
        }
    }
    */
}
