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
    public string GamesFolder;
    public string JoyToKeyFolder;
    public string BGMusicFolder;
    #endregion


}