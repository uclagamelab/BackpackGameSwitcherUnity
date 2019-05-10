using Crosstales.FB;
using Microsoft.Win32;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;


public class ToolsAndSettingsMenu : MonoBehaviour {

    public static bool isOpen
    {
        get;
        private set;
    }

    [SerializeField]
    Transform allMenu;
    public Text resultMessage;

    [SerializeField]
    InputField gamesDirInputField;

    [SerializeField]
    [UnityEngine.Serialization.FormerlySerializedAs("joyToKeyInputField")]
    InputField joyToKeyDirInputField;

    [SerializeField]
    InputField bgMusicDirInputField;

    [Header("Windows")]
    [SerializeField]
    GameObject _mainSettingsWindow;

    [SerializeField]
    GameObject _gameListWindow;

    [Header("Buttons")]
    [SerializeField]
    Button _editGamesButton;

    [SerializeField]
    Button _overallSettingsButton;

    [SerializeField]
    Button _generateJoyToKeyAppAssociationFileButton;

    [SerializeField]
    Button _auditButton;

    void Awake ()
    {
        loadValuesFromSettings();

        string[] args = System.Environment.GetCommandLineArgs();
        if (System.Array.Find<string>(args, (string s)=> { return s.Equals("--setup"); }) != null)
        {
            showSetup(true);
        }


        _editGamesButton.onClick.AddListener(onEditGamesButtonClicked);
        _overallSettingsButton.onClick.AddListener(onOverallSettingsButtonClicked);
        _auditButton.onClick.AddListener(auditGames);
        _generateJoyToKeyAppAssociationFileButton.onClick.AddListener(GenerateJoyToKeyExeAssociationFile);


        isOpen = allMenu.gameObject.activeSelf; 

    }

    void onEditGamesButtonClicked()
    {
        _mainSettingsWindow.gameObject.SetActive(false);
        _gameListWindow.gameObject.SetActive(true);
    }

    void onOverallSettingsButtonClicked()
    {
        _mainSettingsWindow.gameObject.SetActive(true);
        _gameListWindow.gameObject.SetActive(false);
    }

    public void showSetup(bool show)
    {
        isOpen = show;
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
        this.joyToKeyDirInputField.text = SwitcherSettings.JoyToKeyFolder;
        this.bgMusicDirInputField.text = SwitcherSettings.BGMusicFolder;
    }

    public void saveCurrentValuesToSettings()
    {
        SwitcherSettings.GamesFolder = this.gamesDirInputField.text;
        SwitcherSettings.JoyToKeyFolder = this.joyToKeyDirInputField.text;
        SwitcherSettings.BGMusicFolder = this.bgMusicDirInputField.text;

        //Apply the settings

        string cleanPath = this.gamesDirInputField.text;
        cleanPath = cleanPath.Replace('\\', '/');
        GameCatalog.Instance.repopulateCatalog(cleanPath);

        GenericSettings.SaveAllGenericSettings();
    }

    void Update()
    {
        if (CrockoInput.GetAdminMenuKeyComboDown())
        {
            this.showSetup(!this.allMenu.gameObject.activeSelf);
        }
    }

    public void GenerateJoyToKeyExeAssociationFile()
    {
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            /*
             AppLinkData
    46
    0
             */
            sb.Append("AppLinkData\n46\n0\n\n");
            foreach (GameData dat in GameCatalog.Instance.games)
            {
                if (!string.IsNullOrEmpty(dat.exePath) && !string.IsNullOrEmpty(dat.joyToKeyConfig))
                {
                    //System.IO.Path.GetFileName();
                    sb.Append(dat.title);
                    sb.Append('|');
                    string j2kFile = dat.joyToKeyConfig;
                    if (j2kFile.EndsWith(".cfg"))
                    {
                        j2kFile = j2kFile.Substring(0, j2kFile.Length - ".cfg".Length);
                    }
                    sb.Append(j2kFile);
                    sb.Append('|');
                    sb.Append(dat.exePath);
                    sb.Append('\n');
                }
            }


            string finalPath = System.IO.Path.Combine(GameCatalog.Instance.joyToKeyData.directory, "AppLink.dat");
            if (File.Exists(finalPath))
            {
                int backUpNum = 1;
                string backupPath = null;
                while (backupPath == null || File.Exists(backupPath))
                {
                    backupPath = System.IO.Path.Combine(GameCatalog.Instance.joyToKeyData.directory, "AppLink_bak_" + backUpNum + ".dat");
                    backUpNum++;
                }
                File.Move(finalPath, backupPath);
            }
            XuFileSystemUtil.WriteText(sb.ToString(), finalPath);
        }

        //resultMessage.text = "Done!, \n\nsaved existing file as : <nothing yet>";
    }


    public void restoreDefaults()
    {
        SwitcherSettings.ResetToDefaults();
        loadValuesFromSettings();
    }

    System.Text.StringBuilder _auditStringBuilder = new System.Text.StringBuilder();
    public void auditGames()
    {
        _auditStringBuilder.Clear();
        foreach (GameData dat in GameCatalog.Instance.games)
        {

            dat.Audit(_auditStringBuilder);
            _auditStringBuilder.AppendLine();
        }

        resultMessage.text = _auditStringBuilder.ToString();
        Debug.Log(_auditStringBuilder);
    }
    
}
