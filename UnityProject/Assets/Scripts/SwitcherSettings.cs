using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitcherSettings : XUGenericPeristentDataSingleton<SwitcherPrefData>
{
    protected override string fileName => "settings.json";
}

[System.Serializable]
public class SwitcherPrefData
{
    public string GamesFolder;
    public string JoyToKeyFolder;
    public string BGMusicFolder;
}