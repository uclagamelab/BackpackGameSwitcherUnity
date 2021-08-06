#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DateTime = System.DateTime;

public static class CrockoEdUtil 
{
    [MenuItem("CrockoDial/Update Verision")]
    public static void UpdateVersion()
    {
        PlayerSettings.bundleVersion = $"1.{ (DateTime.Now.Year % 100).ToString("00")}.{DateTime.Now.Month}.{DateTime.Now.Day}";
    }
}
#endif