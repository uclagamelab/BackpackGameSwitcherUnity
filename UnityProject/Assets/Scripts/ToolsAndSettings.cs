using Crosstales.FB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ToolsAndSettings : MonoBehaviour {



    [SerializeField]
    Transform allMenu;
    public Text resultMessage;

    [SerializeField]
    InputField gamesDirInputField;

    [SerializeField]
    InputField joyToKeyInputField;

	void Awake ()
    {
        loadValuesFromSettings();

        string[] args = System.Environment.GetCommandLineArgs();
        if (System.Array.Find<string>(args, (string s)=> { return s.Equals("--setup"); }) != null)
        {
            showSetup(true);
        }

    }

    public void showSetup(bool show)
    {
        this.allMenu.gameObject.SetActive(show);
        if (show)
        {
            loadValuesFromSettings();
        }
        
    }

    void loadValuesFromSettings()
    {
        this.gamesDirInputField.text = SwitcherSettings.GamesFolder;
        this.joyToKeyInputField.text = SwitcherSettings.JoyToKeyFolder;
    }

    public void saveCurrentValuesToSettings()
    {
        SwitcherSettings.GamesFolder = this.gamesDirInputField.text;
        SwitcherSettings.JoyToKeyFolder = this.joyToKeyInputField.text;

        //Apply the settings
        GameCatalog.Instance.repopulateCatalog(this.gamesDirInputField.text);
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

    public void browseAndSetInputField(InputField directoryField)
    {
        string path = FileBrowser.OpenSingleFolder("Open Folder");
        directoryField.text = path;
    }

    public void restoreDefaults()
    {
        SwitcherSettings.ResetToDefaults();
        loadValuesFromSettings();
    }

}
