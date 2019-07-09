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
using System.Runtime.InteropServices;
using System.IO;
using Debug = UnityEngine.Debug;

public class ProcessRunner : MonoBehaviour
{
    private string _currentJoyToKeyConfig = null;
    private readonly string SWITCHER_JOYTOKEY_CONFIG = "menuselect.cfg";

    float lastFocusSwitchAttemptTime = float.NegativeInfinity;

    public string processStateHelper;


    //Add startup forgiveness timer, for games that change window
    public bool IsGameRunning()
    {
        bool processJustStarted = _runningGame != null && (Time.time - currentProcessStartTime) < 2;
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
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
    [DllImport("user32.dll")]
    static extern bool DrawMenuBar(IntPtr hWnd);

    [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
    public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, uint wFlags);

    private const int GWL_STYLE = -16;              //hex constant for style changing
    const uint WS_BORDER = 0x800000;

    /// <summary>The window has a title bar (includes the WS_BORDER style).</summary>
    const uint WS_CAPTION = 0xc00000;

    /// <summary>The window is a child window. A window with this style cannot have a menu bar. This style cannot be used with the WS_POPUP style.</summary>
    const uint WS_CHILD = 0x40000000;

    /// <summary>Excludes the area occupied by child windows when drawing occurs within the parent window. This style is used when creating the parent window.</summary>
    const uint WS_CLIPCHILDREN = 0x2000000;

    /// <summary>
    ///   Clips child windows relative to each other; that is, when a particular child window receives a WM_PAINT message, the WS_CLIPSIBLINGS style clips all other overlapping child windows out of the region of the child window to be updated.
    ///   If WS_CLIPSIBLINGS is not specified and child windows overlap, it is possible, when drawing within the client area of a child window, to draw within the client area of a neighboring child window.
    /// </summary>
    const uint WS_CLIPSIBLINGS = 0x4000000;

    /// <summary>The window is initially disabled. A disabled window cannot receive input from the user. To change this after a window has been created, use the EnableWindow function.</summary>
    const uint WS_DISABLED = 0x8000000;

    /// <summary>The window has a border of a style typically used with dialog boxes. A window with this style cannot have a title bar.</summary>
    const uint WS_DLGFRAME = 0x400000;

      /// <summary>
      ///   The window is the first control of a group of controls. The group consists of this first control and all controls defined after it, up to the next control with the WS_GROUP style.
      ///   The first control in each group usually has the WS_TABSTOP style so that the user can move from group to group. The user can subsequently change the keyboard focus from one control in the group to the next control in the group by using the direction keys.
      ///   You can turn this style on and off to change dialog box navigation. To change this style after a window has been created, use the SetWindowLong function.
      /// </summary>
    const uint WS_GROUP = 0x20000;

    /// <summary>The window has a horizontal scroll bar.</summary>
    const uint WS_HSCROLL = 0x100000;

    /// <summary>The window is initially maximized.</summary>
    const uint WS_MAXIMIZE = 0x1000000;

    /// <summary>The window has a maximize button. Cannot be combined with the WS_EX_CONTEXTHELP style. The WS_SYSMENU style must also be specified.</summary>
    const uint WS_MAXIMIZEBOX = 0x10000;

    /// <summary>The window is initially minimized.</summary>
    const uint WS_MINIMIZE = 0x20000000;

    /// <summary>The window has a minimize button. Cannot be combined with the WS_EX_CONTEXTHELP style. The WS_SYSMENU style must also be specified.</summary>
    const uint WS_MINIMIZEBOX = 0x20000;

    /// <summary>The window is an overlapped window. An overlapped window has a title bar and a border.</summary>
    const uint WS_OVERLAPPED = 0x0;

    /// <summary>The window is an overlapped window.</summary>
    const uint WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_SIZEFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX;

      /// <summary>The window is a pop-up window. This style cannot be used with the WS_CHILD style.</summary>
    const uint WS_POPUP = 0x80000000u;

      /// <summary>The window is a pop-up window. The WS_CAPTION and WS_POPUPWINDOW styles must be combined to make the window menu visible.</summary>
    const uint WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU;

      /// <summary>The window has a sizing border.</summary>
    const uint WS_SIZEFRAME = 0x40000;

    /// <summary>The window has a window menu on its title bar. The WS_CAPTION style must also be specified.</summary>
    const uint WS_SYSMENU = 0x80000;

    /// <summary>
    ///   The window is a control that can receive the keyboard focus when the user presses the TAB key.
    ///   Pressing the TAB key changes the keyboard focus to the next control with the WS_TABSTOP style.
    ///   You can turn this style on and off to change dialog box navigation. To change this style after a window has been created, use the SetWindowLong function.
    ///   For user-created windows and modeless dialogs to work with tab stops, alter the message loop to call the IsDialogMessage function.
    /// </summary>
    const uint WS_TABSTOP = 0x10000;

      /// <summary>The window is initially visible. This style can be turned on and off by using the ShowWindow or SetWindowPos function.</summary>
      const uint WS_VISIBLE = 0x10000000;

    /// <summary>The window has a vertical scroll bar.</summary>
    const uint WS_VSCROLL = 0x200000;

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


    IntPtr _runningPrimaryWindow
    {
        get
        {
            List<IntPtr> _runningWindowHandles = new List<IntPtr>();
            CollectProcessWindows(currentGameRunner.process.Id, _runningWindowHandles);


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
    
        if (currentGameRunner != null)
        {
            //UnityEngine.Debug.Log("CloseProcess called : " + _currentRunningGameProcess.Id);

            KillAllNonSafeProcesses();
        }
        currentGameRunner?.Reset();
        _runningGame = null;
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
                    TerminateProcess((uint)p.Handle, exitCode);
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
            TerminateProcess((uint)p.Handle, exitCode);
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
