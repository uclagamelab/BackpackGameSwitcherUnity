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


    //Add startup forgiveness timer, for games that change window
    public bool IsGameRunning()
    {
        bool processJustStarted = _runningGame != null && (Time.time - currentProcessStartTime) < 5;
        bool windowBasedGameIsRunning = _runningGame != null && !string.IsNullOrEmpty(_runningGame.windowTitle) && WindowIsPresent(_runningGame.windowTitle);
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

	
    [ContextMenu("TESTREMOVE")]
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

    



	Process _thisProcess = Process.GetCurrentProcess(); //The application switcher process?
    GameData _runningGame = null;
    IGameRunner currentGameRunner => _runningGame?.launchSettings.Runner();
    //Process _currentRunningGameProcess = null; //the currently running game process
	Process _joy2KeyProcess = null;




    IntPtr _thisPrimaryWindow = IntPtr.Zero;
    IntPtr thisPrimaryWindow
    {
        get
        {
            List<IntPtr> menuWindowHandles = new List<IntPtr>();
            CollectProcessWindows(_thisProcess.Id, menuWindowHandles);

            if (menuWindowHandles.Count > 0)
            {
                

                //print("got it!");
                _thisPrimaryWindow = menuWindowHandles[0];
            }
            //return IntPtr.Zero;
            return _thisPrimaryWindow;
            
        }
    }


    //https://stackoverflow.com/questions/1888863/how-to-get-main-window-handle-from-process-id
    /// <summary>
    /// Best guess at the running game process's main window
    /// </summary>
    List<IntPtr> _runningWindowHandlesScratch = new List<IntPtr>();
    IntPtr runningPrimaryWindowGuess 
    {
        get
        {
            _runningWindowHandlesScratch.Clear();
            CollectProcessWindows(currentGameRunner.process.Id, _runningWindowHandlesScratch);

            IntPtr ret = IntPtr.Zero;
            foreach (IntPtr handle in _runningWindowHandlesScratch)
            {
                if (WinOsUtil.GetWindow(handle, GetWindowType.GW_OWNER) == IntPtr.Zero && WinOsUtil.IsWindowVisible(handle))
                {
                    #if UNITY_EDITOR
                    if (ret != IntPtr.Zero)
                    {
                        //TODO: this would be smart to put in the audit/error messages section
                        Debug.Log("Multiple windows seem like they could be the main window for this process!\nPlease manually specify the main windows title");
                    }
                    #endif
                    ret = handle;
                    #if !UNITY_EDITOR
                    break;
                    #endif

                    //return handle;
                }
            }
            return ret;
        }
    }

    IntPtr _joy2KeyPrimaryWindow = IntPtr.Zero;


    XUTimer _windowCheckTimers = new XUTimer(.3f);
    System.Text.StringBuilder _sb = new System.Text.StringBuilder();

    // Pressing the button opens up the game.
    // Pressing CTRL - C brings this game to the foreground

    void Awake()
    {
        _windowCheckTimers.Start();
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
    bool allWindowIter(IntPtr hWnd, IntPtr lParam)
    {
        //if (hWnd == shellWindow) return true;
        if (!IsWindowVisible(hWnd)) return true;

        if (!IsWindowVisible(hWnd)) return true;

        int length = GetWindowTextLength(hWnd);
        if (hWnd == IntPtr.Zero || length == 0) return true;

        System.Text.StringBuilder sb = new System.Text.StringBuilder(256);
        GetWindowText(hWnd, sb, 256);

        string windowTitle = sb.ToString();
        if (!_allWindowsCached.ContainsKey(windowTitle))
        {
            _allWindowsCached.Add(sb.ToString(), hWnd);
        }
        else
        {
            //TODO : support duplicate window names
            //Dictionary<string, HashSet<IntPtr>>
#if UNITY_EDITOR
            //Debug.LogError("Multiple windows with title: " + windowTitle);
#endif
        }
        
        return true;
    }
    public static Dictionary<string, IntPtr> _allWindowsCached = new Dictionary<string, IntPtr>();
    private void Update()
    {
        //----------------------------------------------
        if (_windowCheckTimers.expired)
        {
            _allWindowsCached.Clear();
            _windowCheckTimers.Restart();
            //foreach (Process p in Process.GetProcesses())
            //{
            //    if (!string.IsNullOrEmpty(p.MainWindowTitle))
            //    {
            //        _allWindowsCached.Add(p.MainWindowTitle);
            //    }
            //}
            //IntPtr shellWindow = GetShellWindow();


            //_allWindowsCached.Clear();

            EnumWindows(allWindowIter, IntPtr.Zero);

        }
        
        //----------------------------------------------

        if (_runningGame != null)
        {
            _runningGame.launchSettings.Runner().RunningUpdate();
        }
    }

    public static bool WindowIsPresent(string windowTitle)
    {
        return _allWindowsCached.ContainsKey(windowTitle);
    }

    public static IntPtr GetWindowByTitle(string windowTitle)
    {
        IntPtr ret = IntPtr.Zero;
        if (_allWindowsCached.ContainsKey(windowTitle))
        {
            ret = _allWindowsCached[windowTitle];
        }

        return ret;
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
        return;
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

    public static Process StartProcess(string directory, string exe, string cmdArgs)
    {

        //this is a little weird...
        string fullPath = Path.Combine(directory, exe);
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

 
        startInfo.WorkingDirectory = directory; //"C:\\Users\\Garrett Johnson\\Desktop";
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

    
	// Puts all windows existing associated with the processId in the bucket
	void CollectProcessWindows(int processId, List<IntPtr> bucket)
	{
        _allWindowsCached.Clear();

        if ( processId == 0 ) return;

		// look through all the windows
		EnumWindows( delegate(IntPtr hWnd, IntPtr lParam)
        {
            // add the handle to the bucket if it's associatd with the given process
            if ( DoesWindowMatchProcessId( hWnd, processId ) )
			{
				if( !bucket.Contains( hWnd ) )
				{
					bucket.Add(hWnd);
				}
			}
			return true;
		}, IntPtr.Zero);

	}
    
	// returns true if the 'hWnd' is managed by the 'processId'
	bool DoesWindowMatchProcessId( IntPtr hWnd, int processId )
	{
        //int threadId = GetWindowThreadProcessId( new HandleRef(new object(), hWnd), out winProcId);
        uint uwinProcId;
        GetWindowThreadProcessId(hWnd, out uwinProcId);
        //winProcId = uwinProcId.ToString();

        bool matched = uwinProcId.ToString().Equals(processId.ToString());
        return matched;
    }

 
    //https://stackoverflow.com/questions/17879890/understanding-attachthreadinput-detaching-lose-focus


           // Forces the given window to show in the foreground
    void ForceBringToForeground(IntPtr hWnd)
	{
        if (hWnd == IntPtr.Zero)
        {
#if UNITY_EDITOR
            Debug.Log("Got bad window handle for foreground control");
#endif
            return;
        }

		IntPtr fgWnd = GetForegroundWindow();

        if (hWnd == fgWnd)
        {
            //print("window already in foreground");
            return;
        }

        //More detailed code and justification can be found here:
        //https://stackoverflow.com/questions/17879890/understanding-attachthreadinput-detaching-lose-focus

        //var currentThread = GetWindowThreadProcessId(_thisPrimaryWindow, IntPtr.Zero);
        //    var activeThread = GetWindowThreadProcessId(fgWnd, IntPtr.Zero);
        //    var windowThread = GetWindowThreadProcessId(hWnd, IntPtr.Zero);


        SetForegroundWindow(hWnd);
        //SetFocus(hWnd);
        //https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-showwindow
        //ShowWindow(hWnd, 3);// 3); //orig by itself
        ShowWindow(hWnd, 9);


        //if (currentThread != activeThread)
        //    AttachThreadInput(currentThread, activeThread, false);
        //if (windowThread != currentThread)
        //    AttachThreadInput(windowThread, currentThread, false);
    }


	public void BringThisToForeground()
    {
        //ForceBringToForeground(thisPrimaryWindow);
#if !UNITY_EDITOR
        string switcherWindowName = Application.productName;
        ForceBringToForeground(GetWindowByTitle(switcherWindowName));
#endif
        setJoyToKeyConfigIfNotAlreadySet(SWITCHER_JOYTOKEY_CONFIG);
    }

    public void quitCurrentGame()
    {
        StopCurrentRunningGame();
        BringThisToForeground();
    }
	
    public void BringRunningToForeground(GameData currentlySelectedGame)
    {
        string windowTitle = currentlySelectedGame.windowTitle;
        setJoyToKeyConfigIfNotAlreadySet(currentlySelectedGame.joyToKeyConfig);




        if (string.IsNullOrEmpty(windowTitle))
        {
            ForceBringToForeground(runningPrimaryWindowGuess);
        }
        else
        {
            ForceBringToForeground(GetWindowByTitle(windowTitle));
        }

           
        //    /// Alt way <<<<<<<<<<<<<<<<
        //    SendKeyStrokesToWindow(windowTitle);
        //    //>>>>>>>>>>>>>>>>
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
}
