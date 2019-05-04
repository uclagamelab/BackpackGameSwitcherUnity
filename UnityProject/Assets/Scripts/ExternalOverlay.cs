using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using Debug = UnityEngine.Debug;

public class ExternalOverlay : MonoBehaviour {

    static ExternalOverlay _instance;
    static bool on = true;
    private void Awake()
    {
        _instance = this;

        on = !CommandLineArguments.AdminMode;
    }

   

    static void ShowOverlay()
    {
        if (!on)
        {
            return;
        }

        //"C:\Program Files\Rainmeter\Rainmeter.exe"[!ActivateConfig "CrockoDial\Main" "Main.ini"][!Move "448" "0"][!Draggable 0]
        ProcessStartInfo startInfo = new ProcessStartInfo("C:\\Program Files\\Rainmeter\\Rainmeter.exe");
        startInfo.Arguments = "[!ActivateConfig \"CrockoDial\\Main\" \"Main.ini\"][!Move \"260\" \"0\"][!Draggable 0]";
        //startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

        Process process = new System.Diagnostics.Process();
        process.StartInfo = startInfo;
        process.Start();
    }

    static void HideOverlay()
    {
        if (!on)
        {
            return;
        }
        //"C:\Program Files\Rainmeter\Rainmeter.exe"[!ActivateConfig "CrockoDial\Main" "Main.ini"][!Move "448" "0"][!Draggable 0]
        ProcessStartInfo startInfo = new ProcessStartInfo("C:\\Program Files\\Rainmeter\\Rainmeter.exe");
        startInfo.Arguments = "[!DeactivateConfig \"CrockoDial\\Main\" \"Main.ini\"]";
        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

        Process process = new System.Diagnostics.Process();
        process.StartInfo = startInfo;
        process.Start();
    }

   static bool _okToShow = true;
    public static void DoShowAndHide()
    {
        if (_okToShow)
        {
            _okToShow = false;
            ShowOverlay();
            _instance.delayedFunction(()=>
            {
                HideOverlay();
                _okToShow = true;
            }, 5);
        }
    }
}
