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
            showSetup(true);
        }
    }

    public void showSetup(bool show)
    {
        this.allMenu.gameObject.SetActive(show);
    }

    void Update()
    {
        if ( (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.K))
        {
            this.showSetup(!this.allMenu.gameObject.activeSelf);
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
