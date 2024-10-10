#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DateTime = System.DateTime;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class XUAutoVersionUpdater : IPreprocessBuildWithReport
{

    public static void UpdateVersion()
    {
        PlayerSettings.bundleVersion = $"1.{System.DateTime.Now.ToString("yy.MM.dd.hh.mm.ss")}";
        Debug.LogError(PlayerSettings.bundleVersion);
    }

    int IOrderedCallback.callbackOrder => int.MaxValue;

    void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report)
    {
        UpdateVersion();
    }
}
#endif