using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using Debug = UnityEngine.Debug;

public class ExternalOverlay : MonoBehaviour {

    static void ShowOverlay()
    {
        //"C:\Program Files\Rainmeter\Rainmeter.exe"[!ActivateConfig "CrockoDial\Main" "Main.ini"][!Move "448" "0"][!Draggable 0]
        ProcessStartInfo startInfo = new ProcessStartInfo("C:\\Program Files\\Rainmeter\\Rainmeter.exe");
        startInfo.Arguments ="[!ActivateConfig \"CrockoDial\\Main\" \"Main.ini\"][!Move \"448\" \"0\"][!Draggable 0]";
        //startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

        Process process = new System.Diagnostics.Process();
        process.StartInfo = startInfo;
        process.Start();
    }

    static void HideOverlay()
    {
        //"C:\Program Files\Rainmeter\Rainmeter.exe"[!ActivateConfig "CrockoDial\Main" "Main.ini"][!Move "448" "0"][!Draggable 0]
        ProcessStartInfo startInfo = new ProcessStartInfo("C:\\Program Files\\Rainmeter\\Rainmeter.exe");
        startInfo.Arguments = "[!DeactivateConfig \"CrockoDial\\Main\" \"Main.ini\"]";
        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

        Process process = new System.Diagnostics.Process();
        process.StartInfo = startInfo;
        process.Start();
    }
    bool showed = false;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            showed = !showed;
            if (showed)
            {
                ShowOverlay();
            }
            else
            {
                HideOverlay();
            }
            
        }
    }
}
