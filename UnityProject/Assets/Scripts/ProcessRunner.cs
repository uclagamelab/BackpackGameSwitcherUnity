/*

This class, (aspirationally) is contains functions for...
- changing focus,
- Starting new exes/bat files etc...
- stopping existing processings

 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;


public class ProcessRunner : MonoBehaviour
{

    bool switcherHasFocus = false;
    bool _gameProcessIsRunning = false;
    public bool gameProcessIsRunning
    {
        get {
            //return _gameProcessIsRunning;
            return (this._runningProcess != null && !this._runningProcess.HasExited);
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

    float okToQuitTime = float.PositiveInfinity;

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

	// cycles through every window and calls callback
	private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool EnumWindows(EnumWindowsProc callback, IntPtr extraData);



	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	//static extern IntPtr SetFocus(HandleRef hWnd);
	static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);


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
	Process _runningProcess = null; //the currently running game process
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
            CollectProcessWindows(_runningProcess.Id, _runningWindowHandles);

            foreach (IntPtr ihwnd in _runningWindowHandles)
            {
                //???
            }

            if (_runningWindowHandles.Count > 0)
            {
                return _runningWindowHandles[0];
            }
            return IntPtr.Zero;
        }
    }

    IntPtr _joy2KeyPrimaryWindow = IntPtr.Zero;


	

	// Pressing the button opens up the game.
	// Pressing CTRL - C brings this game to the foreground

    void Awake()
    {
        _instance = this;
    }


	void Start()
	{

        //this.killAllPrevProcesses();

        setJoyToKeyConfig("menuselect.cfg");

        //Not sure if delay is actually necessary
        this.delayedFunction(() =>
        {
            recordSafeProcesses();
        }, 2);

    }



	void Update()
	{



		// Check if the process has Exitted
		if( _runningProcess != null)
		{


            if (_runningProcess.HasExited && Time.time > okToQuitTime)
            {

            }
            else
            {
                
                //OK to do so frequently???
                if (Time.time > lastFocusSwitchAttemptTime + .5f)
                {
                    lastFocusSwitchAttemptTime = Time.time;
                    BringRunningToForeground(); //this function should be robust to repeated calls
                }
            }
		}
        else
        {
            if (Time.time > lastFocusSwitchAttemptTime + 2)
            {
                lastFocusSwitchAttemptTime = Time.time;
                //BringThisToForeground(); //why did I comment this out?
            }
        }
	}

    float lastFocusSwitchAttemptTime = float.NegativeInfinity;

    //Not really used anymore, since joytokey does a good job on its own.
    void setJoyToKeyConfig(string configFile)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo();

        startInfo.WorkingDirectory = @GameCatalog.Instance.joyToKeyData.directory; //"C:\\Users\\Garrett Johnson\\Desktop";
        startInfo.FileName = @GameCatalog.Instance.joyToKeyData.executable;
        startInfo.Arguments = configFile;//Path.GetFileNameWithoutExtension(exe);


        _joy2KeyProcess = Process.Start(startInfo);
    }

    // Opens the given process
    // Returns true if it worked, otherwise returns false if there was already a process running
    public bool OpenProcess(string directory, string exe, string cmdArgs, string joyToKeyArgs)
    {

        /*if( _runningProcess != null )
		{
			return false;
		}*/

        ////////////////////
        //Open Joy2Key with appropriate config file
        ////////////////////
        //setJoyToKeyConfig(joyToKeyArgs);

        ///////////////// End joy2key startup

        ProcessStartInfo startInfo = new ProcessStartInfo();

        if (cmdArgs.Length == 0 && false) {
            UnityEngine.Debug.Log("no unity");
            startInfo.UseShellExecute = true;
        }
        else {
            startInfo.CreateNoWindow = false;
            //startInfo.UseShellExecute = false;
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
        }

        startInfo.WorkingDirectory = directory; //"C:\\Users\\Garrett Johnson\\Desktop";
        startInfo.FileName = exe;               //"angryBots.exe";
        startInfo.Arguments = cmdArgs; 			//"-popupwindow -screen-width 1920 -screen-height 1080";
        /*startInfo.CreateNoWindow = false;
        startInfo.WindowStyle = ProcessWindowStyle.Maximized;
        startInfo.UseShellExecute = false;*/

        _runningProcess = Process.Start(startInfo);
        okToQuitTime = Time.time + 5;

        //new -----------
        //this.delayedFunction(() => {
            BringRunningToForeground();
        //}, 2);
        return true;
	}

	// Closes the process if one is running
	void CloseProcess()
	{
        UnityEngine.Debug.Log("CloseProcess called");

        if ( _runningProcess != null )
		{
            UnityEngine.Debug.Log("CloseProcess called : " + _runningProcess.Id);
            // do we need to use kill here?
            //_runningProcess.Kill();
            //ProcessUtility.KillTree(_runningProcess.Id);

            KillAllNonSafeProcesses(_runningProcess.Handle, (uint) _runningProcess.Id, 0);

			// Clear window associations
			//_runningWindowHandles.Clear();
			//_runningPrimaryWindow = IntPtr.Zero;

			//TerminateProcessTreeOld (_joy2KeyProcess.Handle, (uint)_joy2KeyProcess.Id,0);
            

        }


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
		if( processId == 0 ) return;

		// look through all the windows
		EnumWindows( delegate(IntPtr hWnd, IntPtr lParam){
			
			// add the handle to the bucket if it's associatd with the given process
			if( DoesWindowMatchProcessId( hWnd, processId ) )
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
		int winProcId;
        //int threadId = GetWindowThreadProcessId( new HandleRef(new object(), hWnd), out winProcId);
        uint uwinProcId;
        GetWindowThreadProcessId(hWnd, out uwinProcId);
        //winProcId = uwinProcId.ToString();

     
        //return winProcId == processId;
        return uwinProcId.ToString().Equals(processId.ToString());
    }

	// Forces the given window to show in the foreground
	void ForceBringToForeground(IntPtr hWnd)
	{
		IntPtr fgWnd = GetForegroundWindow();

        if (hWnd == fgWnd)
        {
            print("OK ALREADY!!!!!!!!!!!!");
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

	public bool IsGameRunning(){ return _runningProcess != null && !_runningProcess.HasExited; }
	public void BringThisToForeground()
    {
        //ForceBringToForeground(thisPrimaryWindow);
        sendKeysBatchFile(Application.productName);
    }

    public void quitCurrentGame()
    {
        CloseGame();
        BringThisToForeground();
    }
	
    public void BringRunningToForeground()
    {
        //string windowTitle =_runningProcess.MainWindowTitle; //doesn't work...



        //UnityEngine.Debug.Log("!!!!!************trying to bring game to fg : '" + _runningPrimaryWindow + "'");
        bool useOldWay = true;
        if (useOldWay)
        {
            //orig way <<<<<<<<
            ForceBringToForeground(_runningPrimaryWindow);
            //>>>>>>>>>>>>>>>>
        }
        else
        {
            string windowTitle = "???";
            UnityEngine.Debug.LogError("this way doesn't work! (can't get window title?)");
            //string windowTitle = "";
            /// Alt way <<<<<<<<<<<<<<<<
            sendKeysBatchFile(windowTitle);
            //>>>>>>>>>>>>>>>>
        }


    }

    public void sendKeysBatchFile(string windowTitle)
    {
        string cmdText = "call " + Application.streamingAssetsPath + Application.streamingAssetsPath + "\\~Special" + "\\sendKeys.bat \"" + windowTitle + "\" \"\"";

        System.Diagnostics.Process process = new System.Diagnostics.Process();
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        startInfo.FileName = "cmd.exe";
        startInfo.Arguments = "/C " + cmdText;
        process.StartInfo = startInfo;
        process.Start();
    }

	public void CloseGame(){ CloseProcess(); }

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
    /////////////////

        //Was this a good way???
    void OnApplicationFocus(bool hasFocus)
    {
        //this.gameProcessIsRunning = !hasFocus;
        switcherHasFocus = hasFocus;
    }
}
