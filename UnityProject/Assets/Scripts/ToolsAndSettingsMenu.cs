using Crosstales.FB;
using Microsoft.Win32;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ToolsAndSettingsMenu : MonoBehaviour {



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


        //print(a);
        //Debug.Break();
    }

    public void showSetup(bool show)
    {
        this.resultMessage.text = "";
        this.allMenu.gameObject.SetActive(show);
        if (show)
        {
            loadValuesFromSettings();

            bool noGames = GameCatalog.Instance.gameCount == 0;
            if (noGames)
            {
                this.resultMessage.text = "Couldn't load any games!\n";
            }
        }

        checkForegroundLockout();
    }

    void checkForegroundLockout()
    {
        try
        {
            if (System.Environment.OSVersion.Version.Major != 7)
            {
                int ForegroundLockTimeout = (int)Registry.GetValue("HKEY_CURRENT_USER\\Control Panel\\Desktop", "ForegroundLockTimeout", -10);

                if (ForegroundLockTimeout != 0)
                {
                    this.resultMessage.text += "\n" 
                        +
                        "If you are running windows 10, you need to open " +
                        "regedit, and set following value to 0:\n\n"
                        + "HKEY_CURRENT_USER\\Control Panel\\Desktop\\ForegroundLockTimeout";
                }
            }


        }
        catch (System.Exception e)
        {

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

        string cleanPath = this.gamesDirInputField.text;
        cleanPath = cleanPath.Replace('\\', '/');
        GameCatalog.Instance.repopulateCatalog(cleanPath);

        GenericSettings.SaveAllGenericSettings();
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
