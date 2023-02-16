using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitcherSettings : XUGenericPeristentDataSingleton<SwitcherPrefData>
{
    #region NON-SERIALIZED
    public static bool AdminMode => false;
    #endregion
    protected override string fileName => "settings.json";
}

[System.Serializable]
public class SwitcherPrefData
{
    #region SERIALIZED
    public string _GamesFolder;
    public string _JoyToKeyFolder;
    public string _BGMusicFolder;
    public bool _ShutDownExplorerWhileRunning;
    public CrockoInputMode _controlMode = CrockoInputMode.joystick;
    //public DisplaySettings displaySettings;
    #endregion

    public string JoyToKeyFolder => ConvertIfExeRelative(_JoyToKeyFolder);
    public string BGMusicFolder => ConvertIfExeRelative(_BGMusicFolder);

    static string ConvertIfExeRelative(string rawPath)
    {
        if (rawPath.StartsWith(".\\")) //is relative
        {
            return System.IO.Path.Combine(XuFileUtil.RunningAppDirectory, rawPath.Substring(2));
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