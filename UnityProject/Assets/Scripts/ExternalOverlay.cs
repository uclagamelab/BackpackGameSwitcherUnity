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
    static bool _overlayAllowed = false;

    bool _overlayShowing = false;

    //"C:\\Program Files\\Rainmeter\\Rainmeter.exe";

    static Process _rainmeterProcess = null;

    private void Awake()
    {
        _instance = this;
        bool rainmeterInstalled = File.Exists(CompanionSoftware.Rainmeter);
        _overlayAllowed = rainmeterInstalled && !SwitcherSettings.AdminMode;
    }

    private void Start()
    {
        if (_overlayAllowed)
        {
            //ProcessStartInfo startInfo = new ProcessStartInfo(CompanionSoftware.Rainmeter);
            _rainmeterProcess = Process.Start(CompanionSoftware.Rainmeter);
        }
        MenuVisualsGeneric.Instance.addListener(this);
    }


    [ContextMenu("Show Overlay")]
    void showOverlay()
    {
        ShowOverlay();
    }
    static void ShowOverlay(float displayDuration = MAX_OVERLAY_ON_TIME)
    {
        #if UNITY_EDITOR
        if (Application.isPlaying)
        {
        #endif
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
        #if UNITY_EDITOR
        }
        #endif

        //"C:\Program Files\Rainmeter\Rainmeter.exe"[!ActivateConfig "CrockoDial\Main" "Main.ini"][!Move "448" "0"][!Draggable 0]
        ProcessStartInfo startInfo = new ProcessStartInfo(CompanionSoftware.Rainmeter);
        startInfo.Arguments = "[!ActivateConfig \"CrockoDial\\Main\" \"Main.ini\"][!Move \"260\" \"0\"][!Draggable 0]";
        //startInfo.WorkingDirectory = "";// System.IO.Path.GetDirectoryName(CompanionSoftware.Rainmeter);
        //startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

        var newProcess = Process.Start(startInfo);
        if (_rainmeterProcess.HasExited)
        {
            _rainmeterProcess = newProcess;
        }
        //Debug.Log("?? " + success);
    }

    [ContextMenu("Hide Overlay")]
    void hideOverlay()
    {
        HideOverlay();
    }
    static void HideOverlay()
    {
        if (!_overlayAllowed)
        {
            return;
        }
        //"C:\Program Files\Rainmeter\Rainmeter.exe"[!ActivateConfig "CrockoDial\Main" "Main.ini"][!Move "448" "0"][!Draggable 0]
        ProcessStartInfo startInfo = new ProcessStartInfo(CompanionSoftware.Rainmeter);
        startInfo.Arguments = "[!DeactivateConfig \"CrockoDial\\Main\" \"Main.ini\"]";

        var newProcess = Process.Start(startInfo);
        if (_rainmeterProcess.HasExited)
        {
            _rainmeterProcess = newProcess;
        }
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

    void OnApplicationQuit()
    {
        ProcessRunner.TerminateProcess(_rainmeterProcess);
    }
}
