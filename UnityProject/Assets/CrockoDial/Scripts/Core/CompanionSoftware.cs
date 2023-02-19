using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class CompanionSoftware  
{
    static string mainFolder
    {
        get 
        {
            string overrideFolder = SwitcherSettings.Data.CompanionSoftwareFolder;
            if (!string.IsNullOrEmpty(overrideFolder))
            {
                return overrideFolder;
            }
            else
            {
                return Path.Combine(XuFileUtil.GetParentDirectory(Application.dataPath, 1), "CompanionSoftware");
            }
        }
    }
    
    //maybe a const var instead of property would be ok???
    //C:\Program Files\Rainmeter\Rainmeter.exe
    public static string Rainmeter => Path.Combine(mainFolder, "Rainmeter/Rainmeter.exe");
    public static string AutoIt => Path.Combine(mainFolder, "AutoIt3/AutoIt3.exe");
    public static string JoyToKey => Path.Combine(SwitcherSettings.Data.JoyToKeyFolder, "JoyToKey.exe");
    public static string BrowserLauncher => Path.Combine(mainFolder, "BrowserLauncher.bat");
}
