using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SwitcherPreferences : XUGenericPeristentDataSingleton<SwitcherPreferences.Prefs>
{
    [System.Serializable]
    public class Prefs
    {
        public int nPlayers = 1;

        public DataLocations dataLocations;
   
        [System.Serializable]
        public class DataLocations
        {
            public string gamesDirectory;
            public string joyToKeyDirectory;
            public string bgMusicDirectory;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SwitcherPreferences))]
public class SwitcherPrefsEditor : XUGenericPersistentDataEditor<SwitcherPreferences, SwitcherPreferences.Prefs>
{

}
#endif