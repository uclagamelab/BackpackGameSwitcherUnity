using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hid = SharpLib.Hid;
using SharpLib.Win32;
using System.Runtime.InteropServices;
using System;
using System.Windows.Forms;

public class BackgroundGamepadInput : MonoBehaviour
{

    public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    public IntPtr interactionWindow;
    IntPtr hMainWindow;
    IntPtr oldWndProcPtr;
    IntPtr newWndProcPtr;
    WndProcDelegate newWndProc;
    bool isrunning = false;

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern System.IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll")]
    static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    void Start()
    {
        if (isrunning) return;

        hMainWindow = GetForegroundWindow();
        newWndProc = new WndProcDelegate(wndProc);
        newWndProcPtr = Marshal.GetFunctionPointerForDelegate(newWndProc);
        oldWndProcPtr = SetWindowLongPtr(hMainWindow, -4, newWndProcPtr);
        isrunning = true;
        HidStart();
    }

    private static IntPtr StructToPtr(object obj)
    {
        var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(obj));
        Marshal.StructureToPtr(obj, ptr, false);
        return ptr;
    }
    IntPtr rawInputBuffer = IntPtr.Zero;
    IntPtr wndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        var aMessage = Message.Create(hWnd, (int)msg, wParam, lParam);
        iHidHandler.ProcessInput(ref aMessage);
        if (msg == Const.WM_INPUT)
        {
            Debug.Log("!!!");
            Hid.Event hidEvent = new Hid.Event(aMessage, OnHidEventRepeat, false, -1, -1);
            var iRawInput = new RAWINPUT();
            if (!SharpLib.Win32.RawInput.GetRawInputData(aMessage.LParam, ref iRawInput, ref rawInputBuffer))
            {
                Debug.Log("GetRawInputData failed!");
            }
            else
            {
                //bool b = iRawInput.hid.
                if (iRawInput.header.dwType == RawInputDeviceType.RIM_TYPEHID)
                {
                    Debug.Log("GP!");
                }
            }
            Marshal.FreeHGlobal(rawInputBuffer);

        }
        
        return CallWindowProc(oldWndProcPtr, hWnd, msg, wParam, lParam);
    }

    private void OnHidEventRepeat(Hid.Event aHidEvent)
    {
        
    }

    void OnDisable()
    {
        Debug.Log("Uninstall Hook");
        if (!isrunning) return;
        SetWindowLongPtr(hMainWindow, -4, oldWndProcPtr);
        hMainWindow = IntPtr.Zero;
        oldWndProcPtr = IntPtr.Zero;
        newWndProcPtr = IntPtr.Zero;
        newWndProc = null;
        isrunning = false;
    }

    //--------------------
    //It seems this library needs iHidHandler.ProcessInput(..) called
    //This link has some info about hacking into that:
    //https://forum.unity.com/threads/recieve-window_commands-in-unity.213741/

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern System.IntPtr GetActiveWindow();

    private Hid.Handler iHidHandler;

    /// <summary>
    /// Just using another handler to check that one can use the parser without registering.
    /// That's useful cause only one windows per application can register for a range of WM_INPUT apparently.
    /// See: http://stackoverflow.com/a/9756322/3288206
    /// </summary>
    private Hid.Handler iHidParser;
    void HidStart()
    {
        //SharpLib.Win32.RawInput.PopulateDeviceList(treeViewDevices);
        //RAWINPUTDEVICE rid;
        RAWINPUTDEVICE[] rid = new RAWINPUTDEVICE[1];
        
        int i = -1;
        ///*
        i++;
        rid[i].usUsagePage = (ushort)SharpLib.Hid.UsagePage.GenericDesktopControls;
        rid[i].usUsage = (ushort)SharpLib.Hid.UsageCollection.GenericDesktop.GamePad;
        rid[i].dwFlags = Const.RIM_INPUT;//0;// flags;
        rid[i].hwndTarget = GetActiveWindow(); //IntPtr.Zero;
        // */

        /*i++;
        rid[i].usUsagePage = 1;// (ushort)SharpLib.Hid.UsagePage.GenericDesktopControls;
        rid[i].usUsage = 6;// (ushort)SharpLib.Hid.UsageCollection.GenericDesktop.Keyboard;
        rid[i].dwFlags = 0;// Const.RIM_INPUT;//0;// flags;
        rid[i].hwndTarget = IntPtr.Zero;
        //GetActiveWindow();*/


        iHidHandler = new SharpLib.Hid.Handler(rid);
        if (!iHidHandler.IsRegistered)
        {
            Debug.Log("Failed to register raw input devices: " + Marshal.GetLastWin32Error().ToString());
        }


          iHidParser = iHidHandler;

        
        iHidParser.OnHidEvent += HandleHidEventThreadSafe;


    }

    private void HandleHidEventThreadSafe(object aSender, Hid.Event aHidEvent)
    {
        Debug.Log("!!!!");
        //throw new NotImplementedException();
    }

    
}
