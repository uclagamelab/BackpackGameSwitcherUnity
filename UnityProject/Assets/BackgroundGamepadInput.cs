using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hid = SharpLib.Hid;
using SharpLib.Win32;
using System.Runtime.InteropServices;
using System;

public class BackgroundGamepadInput : MonoBehaviour
{
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
    void Start()
    {
        //SharpLib.Win32.RawInput.PopulateDeviceList(treeViewDevices);
        //RAWINPUTDEVICE rid;
        RAWINPUTDEVICE[] rid = new RAWINPUTDEVICE[1];
        
        int i = -1;
        /*i++;
        rid[i].usUsagePage = (ushort)SharpLib.Hid.UsagePage.GenericDesktopControls;
        rid[i].usUsage = (ushort)SharpLib.Hid.UsageCollection.GenericDesktop.Keyboard;
        rid[i].dwFlags = Const.RIM_INPUT;//0;// flags;
        rid[i].hwndTarget = IntPtr.Zero;*/

        i++;
        rid[i].usUsagePage = 1;// (ushort)SharpLib.Hid.UsagePage.GenericDesktopControls;
        rid[i].usUsage = 6;// (ushort)SharpLib.Hid.UsageCollection.GenericDesktop.Keyboard;
        rid[i].dwFlags = 0;// Const.RIM_INPUT;//0;// flags;
        rid[i].hwndTarget = IntPtr.Zero;
        //GetActiveWindow();


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
