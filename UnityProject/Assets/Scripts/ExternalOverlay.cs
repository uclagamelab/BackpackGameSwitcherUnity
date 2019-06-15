using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using Debug = UnityEngine.Debug;

public class ExternalOverlay : MonoBehaviour, MenuVisualsGeneric.Listener
{

    static ExternalOverlay _instance;
    float _autoTimeout = 0;
    const float MAX_OVERLAY_ON_TIME = 10;
    static bool _overlayAllowed = true;

    bool _overlayShowing = false;
    const string RAINMETER_PATH = "C:\\Program Files\\Rainmeter\\Rainmeter.exe";
    private void Awake()
    {
        _instance = this;
        bool rainmeterInstalled = File.Exists(RAINMETER_PATH);
        _overlayAllowed = rainmeterInstalled && !SwitcherSettings.AdminMode;
    }

    private void Start()
    {
        MenuVisualsGeneric.Instance.addListener(this);
    }


    static void ShowOverlay(float displayDuration = MAX_OVERLAY_ON_TIME)
    {
        if (!_overlayAllowed) //-------- Don't show if disabled ---------------
        {
            return;
        }

        if (_instance._overlayShowing) //--- Don't show if already showing ----
        {
            return;
        }

        _instance._overlayShowing = true;

        _instance._autoTimeout = displayDuration;

        //"C:\Program Files\Rainmeter\Rainmeter.exe"[!ActivateConfig "CrockoDial\Main" "Main.ini"][!Move "448" "0"][!Draggable 0]
        ProcessStartInfo startInfo = new ProcessStartInfo(RAINMETER_PATH);
        startInfo.Arguments = "[!ActivateConfig \"CrockoDial\\Main\" \"Main.ini\"][!Move \"260\" \"0\"][!Draggable 0]";
        //startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

        Process process = new System.Diagnostics.Process();
        process.StartInfo = startInfo;
        process.Start();
    }

    static void HideOverlay()
    {
        if (!_overlayAllowed)
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
        _instance._overlayShowing = false;
        _instance._autoTimeout = 0;//doesn't really matter
    }

    private void Update()
    {
        if (!_overlayAllowed)
        {
            return;
        }
        //--- AUTO TURN OFF OVERLAY if open too long ------------
        if (_overlayShowing && _autoTimeout > 0)
        {
            _autoTimeout -= Time.deltaTime;

            if (_autoTimeout <= 0)
            {
                _autoTimeout = 0;
                HideOverlay();
            }
        }
 
        
    }


    void MenuVisualsGeneric.Listener.onLeaveAttract()
    {

    }

    void MenuVisualsGeneric.Listener.onEnterAttract()
    {

    }

    void MenuVisualsGeneric.Listener.onCycleGame(int direction, bool userInitiated)
    {

    }

    void MenuVisualsGeneric.Listener.onStartGame()
    {
        //TODO: handle overlay differently, depending on game
        float defaultOverlayShowDuration = 5;
        ShowOverlay(defaultOverlayShowDuration);
    }

    void MenuVisualsGeneric.Listener.onQuitGame()
    {
        if (_overlayShowing)
        {
            HideOverlay();
        }
    }
}
