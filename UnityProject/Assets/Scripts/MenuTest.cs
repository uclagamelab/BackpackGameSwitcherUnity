using UnityEngine;
using System.Collections;

using System.Diagnostics;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;

public class MenuTest : MonoBehaviour {

	//[DllImport("user32.dll")]
	//private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

	//[DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
	//public static extern IntPtr FindWindow(String lpClassName, String lpWindowName);
	
	[DllImport("USER32.DLL")]
	public static extern bool SetForegroundWindow(IntPtr hWnd);
	//[DllImport("USER32.DLL")]
	//public static extern bool SetActiveWindow(IntPtr hWnd);
	[DllImport("USER32.DLL")]
	public static extern bool SetFocus(IntPtr hWnd);

	//[DllImport("user32.dll", SetLastError = true)]
	//[return: MarshalAs(UnmanagedType.Bool)]
	//static extern bool UnregisterHotKey(IntPtr hWnd, int id); 



	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern int GetWindowThreadProcessId(HandleRef handle, out int processId);
	
	private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool EnumWindows(EnumWindowsProc callback, IntPtr extraData);


	[DllImport("user32.dll")]
   	private static extern short GetAsyncKeyState(int vlc);



	//[DllImport("user32.dll", EntryPoint="SystemParametersInfo")]
	//public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, uint pvParam, uint fWinIni);
	
	//- See more at: http://markribau.org/blog/2005/12/29/why-dont-focus-and-setforegroundwindow-work/#sthash.U5FhGSpK.dpuf


	[DllImport("user32.dll")]
	static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

	[DllImport("user32.dll")]
	static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);


	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

	[DllImport("user32.dll")]
	private static extern IntPtr GetForegroundWindow();

	bool findRunningProcessWindow( IntPtr hWnd, IntPtr lParam )
	{
		if( _runningProcess != null )
		{
			int pId;
			GetWindowThreadProcessId( new HandleRef(new object(), hWnd), out pId);

			if( pId == _runningProcess.Id )
			{
				if( !_runningWindows.Contains(hWnd))
				{
					_runningWindows.Add(hWnd);
				}
			}
			    
		}
		return true;
	}

	bool findMyWindow( IntPtr hWnd, IntPtr lParam )
	{
		int pId;
		GetWindowThreadProcessId( new HandleRef(new object(), hWnd), out pId);
		
		if( pId == Process.GetCurrentProcess().Id )
		{
			if( !_myWindows.Contains(hWnd))
			{
				_myWindows.Add(hWnd);
			}
			_myWindow = hWnd;
		}

		return true;
	}



	Process _runningProcess = null;
	List<IntPtr> _runningWindows = new List<IntPtr>();//(IntPtr)0;
	IntPtr _myWindow = (IntPtr)0;
	List<IntPtr> _myWindows = new List<IntPtr>();
	IntPtr _foregroundWindow = (IntPtr)0;
	IntPtr _myFgWindow = (IntPtr)0;
	int _hitCommand = 0;

	void Start()
	{

		//RegisterHotKey( _myWindow, 0, 0x0002, 0x42);
		//UnregisterHotKey( _myWindow, 0);
	}

	bool _comboPressed = false;

	void Update()
	{
		EnumWindows( findMyWindow, (IntPtr)0);


		IntPtr fgWindow = GetForegroundWindow();
		if(_runningWindows.Contains(fgWindow))
		{
			_foregroundWindow = fgWindow;
		}
		else if(_myWindows.Contains(fgWindow))
		{
			_myFgWindow = fgWindow;
		}

		if( GetAsyncKeyState(0x11) != 0 && GetAsyncKeyState(0x43) != 0) // ctrl-c
		{
            UnityEngine.Debug.Log("ctrl-c");
			if(!_comboPressed)
			{
				_comboPressed = true;
				//foreach( IntPtr ip in _myWindows )
				//{
				/// ONE METHOD THAT WORKS SOMETIMES
				/*
				SystemParametersInfo( (uint) 0x2001, 0, 0, 0x0002 | 0x0001);
				Thread.Sleep(100);
				SetForegroundWindow(_myFgWindow);
				ShowWindow(_myFgWindow, 1);
				SetForegroundWindow(_myFgWindow);
				ShowWindow(_myFgWindow, 1);
				SetForegroundWindow(_myFgWindow);
				ShowWindow(_myFgWindow, 1);

				SetFocus(_myFgWindow);
				SystemParametersInfo( (uint) 0x2001, 200000, 200000, 0x0002 | 0x0001);
				*/

				var _myThreadID = GetWindowThreadProcessId( _myFgWindow , IntPtr.Zero);
				var _otherThreadID = GetWindowThreadProcessId( _foregroundWindow, IntPtr.Zero );

				AttachThreadInput( _myThreadID, _otherThreadID, true);
				SetForegroundWindow(_myFgWindow);
				SetFocus(_myFgWindow);
				AttachThreadInput( _myThreadID, _otherThreadID, false);


				//ShowWindow(_myFgWindow, 1);
				//SetForegroundWindow(_myFgWindow);
					//SetForegroundWindow(ip);
					//SetActiveWindow(ip);
					//SetFocus(ip);;

				_hitCommand ++;
			}
			//}
		}
		else
		{
			_comboPressed = false;
		}
		if( Input.GetKeyDown(KeyCode.B))
		{
			_hitCommand ++;
		}

		if( _runningProcess != null)
		{
			EnumWindows( findRunningProcessWindow, (IntPtr)0);
		}
		else
		{
			_runningWindows = new List<IntPtr>();
		}
	}
	string str = "";
	void OnGUI()
	{
	
		str += Event.current.character;

		GUI.Label(new Rect(100,400,500,800), " str : " + str);

		
		if(_runningProcess != null ) GUI.Label(new Rect(100,100,100,100), " game fg handle : " + _foregroundWindow + " : my fg handle : " + _myFgWindow);
		else GUI.Label(new Rect(100,100,100,100), "thing : none" );

		if( _runningProcess != null )
		{


			if(GUI.Button(new Rect(20,20,100,50), "Close Thing"))
			{

					_runningProcess.Kill();

			}
			if(GUI.Button(new Rect(20,220,100,50), "Bring Thing to front"))
			{
				//foreach( IntPtr ip in _runningWindows )
				//{

				//ShowWindow(_foregroundWindow, 1);
				SetForegroundWindow(_foregroundWindow);
			

				//ShowWindow(_foregroundWindow, 6);
					//SetActiveWindow(ip);

					
					//SetFocus(ip);

				//}
			}


			GUI.Label(new Rect(20,200,1000,1000),"RUNNING GAME! "+Time.frameCount+" hasExit:" + _runningProcess.HasExited + " resp: " + _runningProcess.Responding + " exitCode: " + _runningProcess.ExitCode );





			if( _runningProcess.HasExited )
			{
				_runningProcess = null;
				//_runningWindow = (IntPtr)0;
			}
			else
			{
				return;
			}
		}

		if(GUI.Button(new Rect(20,20,100,50), "Run Thing"))
		{

			// Use ProcessStartInfo class
			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.CreateNoWindow = false;
			startInfo.UseShellExecute = false;
			startInfo.WorkingDirectory = "C:\\Users\\Garrett Johnson\\Desktop";
			startInfo.FileName = "angryBots.exe";
			startInfo.WindowStyle = ProcessWindowStyle.Normal;
			startInfo.Arguments = "-popupwindow -screen-width 1920 -screen-height 1080";

			try
			{
				// Start the process with the info we specified.
				// Call WaitForExit and then the using statement will close.
				_runningProcess= Process.Start(startInfo);


				/*using (_runningProcess = Process.Start(startInfo))
				{
					_runningProcess.WaitForExit();
					_runningProcess = null;
				}*/
				//_runningProcess.Disposed += OnCloseGame;
			}
			catch
			{
				_runningProcess = null;
				// Log error.
			}








		}
	}

	void OnCloseGame(object sender, System.EventArgs e) {
		_runningProcess = null;
	}
}
