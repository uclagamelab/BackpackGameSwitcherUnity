using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class XUCommandLineArguments 
{
    static List<string> args = new List<string>(System.Environment.GetCommandLineArgs());

    public static bool Contains(string flag)
    {
        return args.Contains(flag);
    }

    public static string GetArgValue(string keyArg)
    {
        for (int i = 0; i < args.Count; i++)
        {
            if (args[i] == keyArg && i+1 < args.Count)
            {
                return args[i + 1];
            }
        }
        return null;
    }
	

}
