/*

This class, (aspirationally) is contains functions for...
- changing focus,
- Starting new exes/bat files etc...
- stopping existing processings

 */

//https://msdn.microsoft.com/en-us/library/windows/desktop/ms632668(v=vs.85).aspx


using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using Debug = UnityEngine.Debug;

public class ProcessRunner : MonoBehaviour
{
    private string _currentJoyToKeyConfig = null;
    private readonly string SWITCHER_JOYTOKEY_CONFIG = "menuselect.cfg";

    float lastFocusSwitchAttemptTime = float.NegativeInfinity;

    public string processStateHelper;

    public bool gameProcessIsRunning
    {
        get {
            //return _gameProcessIsRunning;
            return (this._currentRunningGameProcess != null && !this._currentRunningGameProcess.HasExited);
        }
        //set { _gameProcessIsRunning = value; }
    }

    static ProcessRunner _instance;
    public static ProcessRunner instance
    {
        get
        {
            return _instance;
        }
    }
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

	// sets the given window to the foreground window
	[DllImport("user32.dll")]
	public static extern bool SetForegroundWindow(IntPtr hWnd);

	// returns the foreground window
	[DllImport("user32.dll")]
	private static extern IntPtr GetForegroundWindow();

	// focuses the given window
	[DllImport("user32.dll")]
	public static extern bool SetFocus(IntPtr hWnd);

    //code for removing window borders
    //https://www.codeproject.com/Questions/413778/Removing-a-Window-Border-No-Winform
    //Import window changing function
    [DllImport("USER32.DLL")]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    [DllImport("user32.dll")]
    static extern bool DrawMenuBar(IntPtr hWnd);

    [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
    public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

    private const int GWL_STYLE = -16;              //hex constant for style changing
    private const int WS_BORDER = 0x00800000;       //window with border
    private const int WS_CAPTION = 0x00C00000;      //window with a title bar
    private const int WS_SYSMENU = 0x00080000;      //window with no borders etc.
    private const int WS_MINIMIZEBOX = 0x00020000;  //window with minimizebox

    public const int SWP_ASYNCWINDOWPOS = 0x4000;
    public const int SWP_DEFERERASE = 0x2000;
    public const int SWP_DRAWFRAME = 0x0020;
    public const int SWP_FRAMECHANGED = 0x0020;
    public const int SWP_HIDEWINDOW = 0x0080;
    public const int SWP_NOACTIVATE = 0x0010;
    public const int SWP_NOCOPYBITS = 0x0100;
    public const int SWP_NOMOVE = 0x0002;
    public const int SWP_NOOWNERZORDER = 0x0200;
    public const int SWP_NOREDRAW = 0x0008;
    public const int SWP_NOREPOSITION = 0x0200;
    public const int SWP_NOSENDCHANGING = 0x0400;
    public const int SWP_NOSIZE = 0x0001;
    public const int SWP_NOZORDER = 0x0004;
    public const int SWP_SHOWWINDOW = 0x0040;

    public const int HWND_TOP = 0;
    public const int HWND_BOTTOM = 1;
    public const int HWND_TOPMOST = -1;
    public const int HWND_NOTOPMOST = -2;

    [ContextMenu("TESTREMOVE")]
    public void TESTREMOVEBORDER()
    {

        if (_allWindowsCached.ContainsKey("New Unity Project"))
        {
            IntPtr window = _allWindowsCached["New Unity Project"];
            SetWindowPos(window, 0, 0, 0, 1920, 1080, SWP_SHOWWINDOW);


            DrawMenuBar(window);
            this.delayedFunction(() =>
            {
                SetWindowLong(window, GWL_STYLE, WS_SYSMENU);
                SetWindowPos(window, 0, 0, 0, 1920, 600,
                    SWP_NOMOVE | SWP_NOZORDER | SWP_NOSIZE | SWP_FRAMECHANGED | SWP_SHOWWINDOW);

            }
            , 1);
            

        }
        
    }

    // cycles through every window and calls callback
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool EnumWindows(EnumWindowsProc callback, IntPtr extraData);

    [DllImport("USER32.DLL")]
    private static extern IntPtr GetShellWindow();


    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	//static extern IntPtr SetFocus(HandleRef hWnd);
	static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("USER32.DLL")]
    private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

    [DllImport("USER32.DLL")]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("USER32.DLL")]
    private static extern bool IsWindowVisible(IntPtr hWnd);


    // Returnsthe process ID associated with the window
    /*[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern int GetWindowThreadProcessId(HandleRef handle, out int processId);
	*/
    [DllImport("user32.dll")]
	static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint lpdwProcessId);

    // ???
    [DllImport("user32.dll")]
	static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);




	Process _thisProcess = Process.GetCurrentProcess(); //The application switcher process?
    GameData _runningGame = null;
    Process _currentRunningGameProcess = null; //the currently running game process
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


    IntPtr _runningPrimaryWindow
    {
        get
        {
            List<IntPtr> _runningWindowHandles = new List<IntPtr>();
            CollectProcessWindows(_currentRunningGameProcess.Id, _runningWindowHandles);


            if (_runningWindowHandles.Count > 0)
            {
                return _runningWindowHandles[0];
            }
            return IntPtr.Zero;
        }
    }

    IntPtr _joy2KeyPrimaryWindow = IntPtr.Zero;


    XUTimer _windowCheckTimers = new XUTimer(.3f);
    System.Text.StringBuilder _sb = new System.Text.StringBuilder();

    // Pressing the button opens up the game.
    // Pressing CTRL - C brings this game to the foreground

    void Awake()
    {
        _instance = this;
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

    void setJoyToKeyConfig(string configFile)
    {
        _currentJoyToKeyConfig = configFile;

        if (!System.IO.File.Exists(@GameCatalog.Instance.joyToKeyData.executable))
        {
            UnityEngine.Debug.LogError("JoyToKey executable not available at '" + GameCatalog.Instance.joyToKeyData.executable + "'");
            return;
        }

        ProcessStartInfo startInfo = new ProcessStartInfo();

        startInfo.WorkingDirectory = @GameCatalog.Instance.joyToKeyData.directory; //"C:\\Users\\Garrett Johnson\\Desktop";
        startInfo.FileName = @GameCatalog.Instance.joyToKeyData.executable;
        startInfo.Arguments = configFile;//Path.GetFileNameWithoutExtension(exe);

        _joy2KeyProcess = Process.Start(startInfo);
    }

    public static Process StartProcess(string directory, string exe, string cmdArgs)
    {
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

        _currentRunningGameProcess = gameToStart.launchSettings.Runner().Launch();//StartProcess(gameToStart.directory, gameToStart.appFile, ""/*currentGameData.commandLineArguments*/);
        _runningGame = gameToStart;
        setJoyToKeyConfig(gameToStart.joyToKeyConfig);
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

	// Forces the given window to show in the foreground
	void ForceBringToForeground(IntPtr hWnd)
	{
		IntPtr fgWnd = GetForegroundWindow();

        if (hWnd == fgWnd)
        {
            //print("window already in foreground");
            return;
        }

        /*if( _thisWindowHandles.Contains( fgWnd ) )
		{
			SetForegroundWindow(hWnd);
			SetFocus(hWnd);
			ShowWindow(hWnd, 3);
			return;
		}*/

        var _thisThreadID = GetWindowThreadProcessId(_thisPrimaryWindow,  IntPtr.Zero);
		var _targetThreadID = GetWindowThreadProcessId( fgWnd, IntPtr.Zero );
		
		AttachThreadInput( _thisThreadID, _targetThreadID, true);
        SetForegroundWindow(hWnd);
		SetFocus(hWnd);
        ShowWindow(hWnd, 3); //orig by itself

        //ShowWindow(hWnd, 6);
        //ShowWindow(hWnd, 9);

        //AttachThreadInput( _thisThreadID, _targetThreadID, false);
    }

    //Add startup forgiveness timer, for games that change window
	public bool IsGameRunning()
    {
        return _currentRunningGameProcess != null && !_currentRunningGameProcess.HasExited;
    }
	public void BringThisToForeground()
    {

        //ForceBringToForeground(thisPrimaryWindow);
        string switcherWindowName = Application.productName;
        SendKeyStrokesToWindow(switcherWindowName);
        setJoyToKeyConfigIfNotAlreadySet(SWITCHER_JOYTOKEY_CONFIG);
    }

    public void quitCurrentGame()
    {
        StopCurrentRunningGame();
        BringThisToForeground();
    }
	
    public void BringRunningToForeground(GameData currentlySelectedGame)
    {
        string overrideWindowTitle = currentlySelectedGame.windowTitle;
        setJoyToKeyConfigIfNotAlreadySet(currentlySelectedGame.joyToKeyConfig);
        

        bool useOldWay = string.IsNullOrEmpty(overrideWindowTitle);
        if (useOldWay)
        {
            //orig way <<<<<<<<
            ForceBringToForeground(_runningPrimaryWindow);
            //>>>>>>>>>>>>>>>>
        }
        else
        {
            string windowTitle = overrideWindowTitle;
            //string windowTitle = "";
            /// Alt way <<<<<<<<<<<<<<<<
            SendKeyStrokesToWindow(windowTitle);
            //>>>>>>>>>>>>>>>>
        }


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
        _runningGame?.launchSettings.Runner()?.LaunchCleanUp();
        _runningGame = null;
        if (_currentRunningGameProcess != null)
        {
            UnityEngine.Debug.Log("CloseProcess called : " + _currentRunningGameProcess.Id);

            KillAllNonSafeProcesses(_currentRunningGameProcess.Handle, (uint)_currentRunningGameProcess.Id, 0);
        }
    }

	///////////
	// [StructLayout(LayoutKind.Sequential)]
	private struct PROCESS_BASIC_INFORMATION
	{
		public int ExitStatus;
		public int PebBaseAddress;
		public int AffinityMask;
		public int BasePriority;
		public uint UniqueProcessId;
		public uint InheritedFromUniqueProcessId;
	}
	[DllImport("kernel32.dll")]
	static extern bool TerminateProcess(uint hProcess, int exitCode);
	[DllImport("ntdll.dll")]
	static extern int NtQueryInformationProcess(
		IntPtr hProcess,
		int processInformationClass /* 0 */,
		ref PROCESS_BASIC_INFORMATION processBasicInformation,
		uint processInformationLength,
		out uint returnLength
		);

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


    public void KillAllNonSafeProcesses(IntPtr hProcess, uint processID, int exitCode)
    {
        Process[] processes = Process.GetProcesses();
        foreach (Process p in processes)
        {
            if(!isSafeProcess(p))
            {
                try
                {
                    TerminateProcess((uint)p.Handle, exitCode);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError(e);
                }
            }
        }
    }

    void killAllPrevProcesses()
    {
        Process[] processes = Process.GetProcesses();
        foreach (Process p in processes)
        {
            if (p.Id != this._thisProcess.Id)
            {
                TerminateProcess((uint)p.Handle, 0);
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

        public static void TerminateProcessTreeOld(IntPtr hProcess, uint processID, int exitCode)
	{
		// Retrieve all processes on the system
		Process[] processes = Process.GetProcesses();
		foreach (Process p in processes)
		{
			// Get some basic information about the process
			PROCESS_BASIC_INFORMATION pbi = new PROCESS_BASIC_INFORMATION();
			try
			{
				uint bytesWritten;
				NtQueryInformationProcess(p.Handle,
				                          0, ref pbi, (uint)Marshal.SizeOf(pbi),
				                          out bytesWritten); // == 0 is OK

                // Is it a child process of the process we're trying to terminate?
                if (pbi.InheritedFromUniqueProcessId == processID)
                {
                    // The terminate the child process and its child processes
                    TerminateProcessTreeOld(p.Handle, pbi.UniqueProcessId, exitCode);
                }
			}
			catch (Exception  ex )
			{
                
                UnityEngine.Debug.LogWarning(ex);
				// Ignore, most likely 'Access Denied'
			}
		}
		
		// Finally, termine the process itself:
		TerminateProcess((uint)hProcess, exitCode);
	}


    struct MyProcInfo
    {
        public int ProcessId;
        public string ProcessName;
    }

    static MyProcInfo GetProcessIdIfStillRunning(int pid)
    {
        try
        {
            var p = Process.GetProcessById(pid);
            return new MyProcInfo() { ProcessId = p.Id, ProcessName = p.ProcessName };
        }
        catch (ArgumentException)
        {
            return new MyProcInfo() { ProcessId = -1, ProcessName = "No-longer existent process" };
        }
    }
}
