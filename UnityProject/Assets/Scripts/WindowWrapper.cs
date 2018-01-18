using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
//using System.Windows;

    /*
     This class is based on the 'Window' class from the processing version of 
     the switcher
     */


public class WindowWrapper  {

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr SetForegroundWindow(IntPtr differentWnd);

    [DllImport("user32.dll")]
    private static extern void ShowWindow(IntPtr window, int nCmdShow);

    Process process;
    IntPtr hWnd
    {
        get { return this.process.MainWindowHandle; }
    }


    //pass process instead??
    public WindowWrapper(Process process)
    {
        this.process = process;

        UnityEngine.Debug.Log("========================= MY HANDLE : " + isNull());
    }

    public bool isNull()
    {
        return hWnd == null || hWnd.GetHashCode() == 0;// || hWnd.hashCode() == 0;
    }

    //bring this window to the foreground
    public void setForeground()
    {
        SetForegroundWindow(this.hWnd);
    }

    public static IntPtr p()
    {
        return GetForegroundWindow();
    }


    public void minimize()
    {
        ShowWindow(this.hWnd, 6);
    }

    public void maximize()
    {
        ShowWindow(this.hWnd, 3);
    }

    public void restore()
    {
        ShowWindow(this.hWnd, 9);
    }
}
