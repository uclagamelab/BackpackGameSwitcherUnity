using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandLineArguments : MonoBehaviour
{
    public static bool AdminMode = false;

	void Awake ()
    {
        string[] cmdArgs = System.Environment.GetCommandLineArgs();

        for (int i = 0; i < cmdArgs.Length; i++)
        {
            string arg = cmdArgs[i];
            if (arg == "-adminMode")
            {
                AdminMode = true;
            }
        }
	}
	
	void Update ()
    {
		
	}
}
