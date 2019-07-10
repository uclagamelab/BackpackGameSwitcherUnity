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
        //_generateJoyToKeyAppAssociationFileButton.onClick.AddListener(GenerateJoyToKeyExeAssociationFile);


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
        this.gamesDirInputField.text = SwitcherSettings.Data._GamesFolder;
        this.joyToKeyDirInputField.text = SwitcherSettings.Data._JoyToKeyFolder;
        this.bgMusicDirInputField.text = SwitcherSettings.Data._BGMusicFolder;
    }

    public void saveCurrentValuesToSettings()
    {
        SwitcherSettings.Data._GamesFolder = this.gamesDirInputField.text;
        SwitcherSettings.Data._JoyToKeyFolder = this.joyToKeyDirInputField.text;
        SwitcherSettings.Data._BGMusicFolder = this.bgMusicDirInputField.text;
        SwitcherSettings.ApplyChanges();

        //Apply the settings
        GameCatalog.Instance.repopulateCatalog(SwitcherSettings.Data.GamesFolder);
    }

    void Update()
    {
        if (CrockoInput.GetAdminMenuKeyComboDown())
        {
            this.showSetup(!this.allMenu.gameObject.activeSelf);
        }
    }

    public void restoreDefaults()
    {
        SwitcherSettings.ClearSaveData();
        loadValuesFromSettings();
    }

    System.Text.StringBuilder _auditStringBuilder = new System.Text.StringBuilder();
    public void auditGames()
    {
        _auditStringBuilder.Clear();
        foreach (GameData dat in GameCatalog.Instance.games)
        {

            dat.Audit(_auditStringBuilder);
        }

        resultMessage.text = _auditStringBuilder.ToString();
        Debug.Log(_auditStringBuilder);
    }
    
}
