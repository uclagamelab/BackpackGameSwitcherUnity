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

public class ProcessRunner : MonoBehaviour
{
    private string _currentJoyToKeyConfig = null;
    private readonly string SWITCHER_JOYTOKEY_CONFIG = "menuselect.cfg";

    float lastFocusSwitchAttemptTime = float.NegativeInfinity;

    public string processStateHelper;

    public static bool SwitcherAppHasFocus = true;

    //Add startup forgiveness timer, for games that change window
    public bool IsGameRunning()
    {
        bool processJustStarted = _runningGame != null && (Time.time - currentProcessStartTime) < 5;
        bool windowBasedGameIsRunning = _runningGame != null && !string.IsNullOrEmpty(_runningGame.windowTitle) &&  ExternalWindowTracker.WindowIsPresent(_runningGame.windowTitle);
        bool processBasedGameIsRunning = _runningGame != null && (currentGameRunner.process != null && !currentGameRunner.process.HasExited);
        return processJustStarted || windowBasedGameIsRunning || processBasedGameIsRunning;
    }

    static XUSingleTown<ProcessRunner> _instanceHelper = new XUSingleTown<ProcessRunner>();
    public static ProcessRunner instance => _instanceHelper.instance;
    // DLL Imports


    float currentProcessStartTime = float.PositiveInfinity;

    float okToQuitTime
    {
        get
        {
            return currentProcessStartTime + 5;
        }
    }

    List<Process> safeProcesses;

	
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



	Process _thisProcess = Process.GetCurrentProcess(); //The application switcher process?
    GameData _runningGame = null;
    IGameRunner currentGameRunner => _runningGame?.launchSettings.Runner();
    //Process _currentRunningGameProcess = null; //the currently running game process
	Process _joy2KeyProcess = null;



    IntPtr _joy2KeyPrimaryWindow = IntPtr.Zero;


 
    System.Text.StringBuilder _sb = new System.Text.StringBuilder();

    // Pressing the button opens up the game.
    // Pressing CTRL - C brings this game to the foreground

    void Awake()
    {

    }


	void Start()
	{

        //this.killAllPrevProcesses();

        setJoyToKeyConfig(SWITCHER_JOYTOKEY_CONFIG);


        //Not sure if delay is actually necessary
        this.delayedFunction(() =>
        {
            recordSafeProcesses();
        }, 2);

    }

    private void Update()
    {


        if (_runningGame != null)
        {
            _runningGame.launchSettings.Runner().RunningUpdate();
        }
    }


    void setJoyToKeyConfigIfNotAlreadySet(string configFile)
    {
        if (configFile != _currentJoyToKeyConfig)
        {
            setJoyToKeyConfig(configFile);
        }
    }

    public void setJoyToKeyConfig(string configFile)
    {
        _currentJoyToKeyConfig = configFile;

        string joyToKeyExe = CompanionSoftware.JoyToKey;

        if (!System.IO.File.Exists(joyToKeyExe))
        {
            UnityEngine.Debug.LogError("JoyToKey executable not available at '" + joyToKeyExe + "'");
            return;
        }

        ProcessStartInfo startInfo = new ProcessStartInfo();

        startInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(joyToKeyExe); //"C:\\Users\\Garrett Johnson\\Desktop";
        startInfo.FileName = joyToKeyExe;
        startInfo.Arguments = configFile;//Path.GetFileNameWithoutExtension(exe);


        var newProcess = Process.Start(startInfo);
        if (_joy2KeyProcess == null || _joy2KeyProcess.HasExited)
        {
            _joy2KeyProcess = newProcess;
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
            Debug.LogError("Game not found at path:\n"+ fullPath);
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

        currentProcessStartTime = Time.time;
    }




    /*
    The algorithm... as is... 
    assumes when the process is started, the correct window indeed comes to the front.
    then you can save that window.  
    */


	// Finds the foreground window for the given bucket. If there isn't one, returns IntPtr.zero
	IntPtr GetForegroundWindowFrom(List<IntPtr> bucket)
	{
		IntPtr fgWnd = GetForegroundWindow();

		if( bucket.Contains( fgWnd ) )
		{
			return fgWnd;
		}

		return IntPtr.Zero;
	}

	public void BringThisToForeground()
    {
        //ForceBringToForeground(thisPrimaryWindow);
        #if !UNITY_EDITOR 
        string switcherWindowName = Application.productName;
        ExternalWindowTracker.ForceBringToForeground(ExternalWindowTracker.GetWindowByTitle(switcherWindowName));
        #endif
        setJoyToKeyConfigIfNotAlreadySet(SWITCHER_JOYTOKEY_CONFIG);
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
        TerminateProcess(_joy2KeyProcess);
    }

    public void KillAllNonSafeProcesses(int exitCode = 0)
    {
        Process[] processes = Process.GetProcesses();
        foreach (Process p in processes)
        {
            if(!isSafeProcess(p))
            {
                try
                {
                    WinOsUtil.TerminateProcess((uint)p.Handle, exitCode);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError(e);
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

    void killAllPrevProcesses()
    {
        Process[] processes = Process.GetProcesses();
        foreach (Process p in processes)
        {
            if (p.Id != this._thisProcess.Id)
            {
                WinOsUtil.TerminateProcess((uint)p.Handle, 0);
            }
        }
    }
    
    bool isSafeProcess(Process proc)
    {
        foreach(Process sp in safeProcesses)
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

}
