using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitcherSettings
{

    public static string GamesFolder
    {
        get
        {
            if (PlayerPrefs.HasKey("GamesFolder"))
            {
                return PlayerPrefs.GetString("GamesFolder");
            }
            else
            {
                return Application.streamingAssetsPath + "/~Special/games";
            }
        }

        set
        {
            PlayerPrefs.SetString("GamesFolder", value);
        }
    }

    public static string JoyToKeyFolder
    {
        get
        {
            if (PlayerPrefs.HasKey("JoyToKeyFolder"))
            {
                return PlayerPrefs.GetString("JoyToKeyFolder");
            }
            else
            {
                return Application.streamingAssetsPath + "/~Special/JoyToKey";
            }
        }

        set
        {
            PlayerPrefs.SetString("JoyToKeyFolder", value);
        }
    }

    public static bool AttractMusicOn;

    public static void ResetToDefaults()
    {
        PlayerPrefs.DeleteAll();
    }
}
