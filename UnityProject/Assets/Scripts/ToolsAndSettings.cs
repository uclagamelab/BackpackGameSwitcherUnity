using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ToolsAndSettings : MonoBehaviour {
    [SerializeField]
    Transform allMenu;
    public Text resultMessage;
	// Use this for initialization
	void Awake () {
        string[] args = System.Environment.GetCommandLineArgs();
        if (System.Array.Find<string>(args, (string s)=> { return s.Equals("--setup"); }) != null)
        {
            this.allMenu.gameObject.SetActive(true);
        }
    }
	
    public void Audit()
    {
        resultMessage.text = "No problems found...\nbut didn't actually check.";
    }

    public void GenerateJoyToKeyExeAssociationFile()
    {
        resultMessage.text = "Done!, \n\nsaved existing file as : <nothing yet>";
    }
}
