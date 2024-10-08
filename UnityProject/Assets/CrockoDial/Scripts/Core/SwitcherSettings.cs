﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SwitcherSettings : XUGenericPeristentDataSingleton<SwitcherPrefData>
{
    #region NON-SERIALIZED
    public static bool AdminMode => false;
    #endregion

    string settingsFile = "settings.json";
    protected override string fileName => settingsFile;


    protected override void Awake()
    {
        string overrideFile = XUCommandLineArguments.GetArgValue("-settings");
        if (!string.IsNullOrEmpty(overrideFile))
        {
            if (!overrideFile.ToLower().EndsWith(".json"))
            {
                overrideFile = overrideFile + ".json";
            }
            settingsFile = overrideFile;
        }

        base.Awake();
    }

}

[System.Serializable]
public class SwitcherPrefData
{
    #region SERIALIZED
    public string _GamesFolder = "";
    public string _JoyToKeyFolder = "";
    public string _CompanionSoftwareFolder = "";
    public string _BGMusicFolder = "";
    public float _BGMusicVolume;
    public float _PreviewVideoVolume = 0;
    public float _SFXVolume = 1;
    public bool _ShutDownExplorerWhileRunning;
    public bool _EnableRainmeter;
    public string _JoyToKeyMenuConfig = ProcessRunner.DEFAULT_SWITCHER_JOYTOKEY_CONFIG;
    public CrockoInputMode _controlMode = CrockoInputMode.joystick;
    //public DisplaySettings displaySettings;
    #endregion

    public System.Action OnValuesUpdated = () => { };

    public string JoyToKeyFolder => ConvertIfExeRelative(_JoyToKeyFolder);
    public string CompanionSoftwareFolder => ConvertIfExeRelative(_CompanionSoftwareFolder);
    public string BGMusicFolder => ConvertIfExeRelative(_BGMusicFolder);

    static string ConvertIfExeRelative(string rawPath)
    {
        if (!Path.IsPathRooted(rawPath))
        {
            return System.IO.Path.Combine(XuFileUtil.RunningAppDirectory, rawPath);//.Substring(2));
        }
        else
        {
            return rawPath;
        }
    }

    public string GamesFolder => ConvertIfExeRelative(_GamesFolder);

    [System.Serializable]
    public class DisplaySettings
    {
        public int resolutionWidth = 1920;
        public int resolutionHeight = 1080;
        public bool fullScreen = true;
    }
}