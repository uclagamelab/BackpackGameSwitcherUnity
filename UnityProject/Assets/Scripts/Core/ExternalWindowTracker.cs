using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExternalWindowTracker : MonoBehaviour
{

    XUTimer _windowCheckTimers = new XUTimer(.3f);
    public static Dictionary<string, IntPtr> _allWindowsCached = new Dictionary<string, IntPtr>();
    public static string editorWindowTitle = null;
    [SerializeField] bool _editorFocusStealing = false;

    public static bool EditorFocusStealing
    {
        get;
        private set;
    }

    // Start is called before the first frame update
    void Awake()
    {
        
        _windowCheckTimers.Start();

        #if UNITY_EDITOR
        EditorFocusStealing = _editorFocusStealing;
        System.Text.StringBuilder sb = new System.Text.StringBuilder(512);
        WinOsUtil.GetWindowText(WinOsUtil.GetActiveWindow(), sb, sb.Capacity);
        editorWindowTitle = sb.ToString();
        Debug.Log(editorWindowTitle);
        #endif
    }

    private void Update()
    {
        //----------------------------------------------
        if (_windowCheckTimers.expired)
        {
            _allWindowsCached.Clear();
            _windowCheckTimers.Restart();
            //foreach (Process p in Process.GetProcesses())
            //{
            //if (!p.HasExited)
            //{
            //    if (p.MainModule.FileName.Contains("chrome"))
            //    {
            //        print(p.MainModule.FileName + " : " + p.MainWindowTitle);
            //    }
            //}
            //if (!string.IsNullOrEmpty(p.MainWindowTitle))
            //{
            //_allWindowsCached.Add(p.MainWindowTitle);
            //}
            //}
            //IntPtr shellWindow = GetShellWindow();


            //_allWindowsCached.Clear();

            WinOsUtil.EnumWindows(allWindowIter, IntPtr.Zero);

        }

    }

    public static bool WindowIsPresent(string windowTitle)
    {
        return _allWindowsCached.ContainsKey(windowTitle);
    }

    // Puts all windows existing associated with the processId in the bucket
    static void CollectProcessWindows(int processId, List<IntPtr> bucket)
    {
        _allWindowsCached.Clear();

        if (processId == 0) return;

        // look through all the windows
        WinOsUtil.EnumWindows(delegate (IntPtr hWnd, IntPtr lParam)
        {
            // add the handle to the bucket if it's associatd with the given process
            if (DoesWindowMatchProcessId(hWnd, processId))
            {
                if (!bucket.Contains(hWnd))
                {
                    bucket.Add(hWnd);
                }
            }
            return true;
        }, IntPtr.Zero);

    }

    bool allWindowIter(IntPtr hWnd, IntPtr lParam)
    {
        //if (hWnd == shellWindow) return true;
        if (!WinOsUtil.IsWindowVisible(hWnd))
        {
            return true;
        }

        int length = WinOsUtil.GetWindowTextLength(hWnd);
        if (hWnd == IntPtr.Zero || length == 0) return true;

        System.Text.StringBuilder sb = new System.Text.StringBuilder(512);
        WinOsUtil.GetWindowText(hWnd, sb, sb.Capacity);

        uint processId = 0;// IntPtr.Zero;
        WinOsUtil.GetWindowThreadProcessId(hWnd, out processId);

        string windowTitle = sb.ToString();


        if (!_allWindowsCached.ContainsKey(windowTitle))
        {
            _allWindowsCached.Add(windowTitle, hWnd);
        }
        else
        {
            //TODO : support duplicate window names
#if UNITY_EDITOR
            //Debug.LogError("Multiple windows with title: " + windowTitle);
#endif
        }

        return true;
    }

    // returns true if the 'hWnd' is managed by the 'processId'
    static bool DoesWindowMatchProcessId(IntPtr hWnd, int processId)
    {
        //int threadId = GetWindowThreadProcessId( new HandleRef(new object(), hWnd), out winProcId);
        uint uwinProcId;
        WinOsUtil.GetWindowThreadProcessId(hWnd, out uwinProcId);
        //winProcId = uwinProcId.ToString();

        bool matched = uwinProcId.ToString().Equals(processId.ToString());
        return matched;
    }

    //https://stackoverflow.com/questions/1888863/how-to-get-main-window-handle-from-process-id
    /// <summary>
    /// Best guess at the running game process's main window
    /// </summary>
    static List<IntPtr> _runningWindowHandlesScratch = new List<IntPtr>();
    public static IntPtr runningPrimaryWindowGuess(int processId)
    {
   
            _runningWindowHandlesScratch.Clear();
            CollectProcessWindows(processId, _runningWindowHandlesScratch);

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

    public delegate bool WindowTitleFilter(string winTitle);
    public static IntPtr GetWindowByTitle(WindowTitleFilter filter)
    {
        IntPtr ret = IntPtr.Zero;
        foreach (string wt in _allWindowsCached.Keys)
        {
            if (filter(wt))
            {
                return _allWindowsCached[wt];
            }
        }
        return ret;
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

    //https://stackoverflow.com/questions/17879890/understanding-attachthreadinput-detaching-lose-focus


    // Forces the given window to show in the foreground
    public static void ForceBringToForeground(IntPtr hWnd)
    {
        if (hWnd == IntPtr.Zero)
        {
#if UNITY_EDITOR
            Debug.Log("Got bad window handle for foreground control");
#endif
            return;
        }

        IntPtr fgWnd = WinOsUtil.GetForegroundWindow();

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


        WinOsUtil.SetForegroundWindow(hWnd);
        //SetFocus(hWnd);
        //https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-showwindow
        //ShowWindow(hWnd, 3);// 3); //orig by itself
        WinOsUtil.ShowWindow(hWnd, 9);


        //if (currentThread != activeThread)
        //    AttachThreadInput(currentThread, activeThread, false);
        //if (windowThread != currentThread)
        //    AttachThreadInput(windowThread, currentThread, false);
    }



    /*static IntPtr _thisPrimaryWindow = IntPtr.Zero;
    public static IntPtr thisPrimaryWindow
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
    } */



}
