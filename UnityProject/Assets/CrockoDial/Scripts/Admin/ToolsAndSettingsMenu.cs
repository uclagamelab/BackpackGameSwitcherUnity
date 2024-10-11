using Crosstales.FB;
using Microsoft.Win32;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;


public class ToolsAndSettingsMenu : MonoBehaviour {

    public static bool isOpen => _I.allMenu.gameObject.activeSelf;

    static ToolsAndSettingsMenu _I = null;

    [SerializeField]
    Transform allMenu;
    public Text resultMessage;
    InputField resultMessageOutput;

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

    [SerializeField]
    Button _quitButton;

    void Awake ()
    {
        _I = this;

        string[] args = System.Environment.GetCommandLineArgs();
        if (System.Array.Find<string>(args, (string s)=> { return s.Equals("--setup"); }) != null)
        {
            showSetup(true);
        }


        _editGamesButton.onClick.AddListener(onEditGamesButtonClicked);
        _overallSettingsButton.onClick.AddListener(onOverallSettingsButtonClicked);
        _auditButton.onClick.AddListener(auditGames);
        //_generateJoyToKeyAppAssociationFileButton.onClick.AddListener(GenerateJoyToKeyExeAssociationFile);

        _quitButton.onClick.AddListener(()=> Application.Quit());

        resultMessageOutput = resultMessage.GetComponent<InputField>();

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
        this.resultMessageOutput.text = "";
        this.allMenu.gameObject.SetActive(show);
        if (show)
        {
            bool noGames = GameCatalog.Instance.gameCount == 0;
            if (noGames)
            {
                this.resultMessageOutput.text = "Couldn't load any games!\n";
            }
        }

        checkForegroundLockout();
        checkAutoRestart();
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
                    this.resultMessageOutput.text += "\n" 
                        +
                        "If you are running windows 10, you need to open " +
                        "regedit, and set following value to 0:\n\n"
                        + "HKEY_CURRENT_USER\\Control Panel\\Desktop\\ForegroundLockTimeout";
                }
            }
        }
        catch (System.Exception)
        {

        }
    }

    [ContextMenu("Check Autorestart")]
    void checkAutoRestart()
    {
        try
        {
            int autoRestartShellOn = (int)Registry.GetValue("HKEY_LOCAL_MACHINE\\Software\\Microsoft\\Windows NT\\CurrentVersion\\WinLogon", "AutoRestartShell", -1);

            if (autoRestartShellOn != 0)
            {
                this.resultMessageOutput.text += 
                    "\n\n"
                    + "Full support for hiding the taskbar, and windows explorer requires you open regedit and set the value to 0:"
                    + "\n\nHKEY_LOCAL_MACHINE\\Software\\Microsoft\\Windows NT\\CurrentVersion\\WinLogon\\AutoRestartShell"
                    ;
            }

        }
        catch (System.Exception e)
        {
            Debug.LogError("Error checking registry values for restarting shell: \n" + e);
        }
    }

    public void saveCurrentValuesToSettings()
    {
        SwitcherSettings.ApplyChanges();

        //Apply the settings
        GameCatalog.Instance.repopulateCatalog(SwitcherSettings.Data.GamesFolder);
    }


    int _passwordCheckIdx = 0;
    bool _allowPassPress = true;


    bool passwordRequired => !string.IsNullOrEmpty(SwitcherSettings.Data._SecurityModePassword);
    //NOTE: following password checking needs to be in late update,
    ///for to 'BackgroundKeyboardInput.Instance.keyDownThisFrame'
    //to work correctly.
    private void LateUpdate()
    {
        var password = SwitcherSettings.Data._SecurityModePassword;
        if (isOpen || !passwordRequired)
        {
            return;
        }

        char passChar = password[_passwordCheckIdx];
        
        if (Input.anyKeyDown)
        {
            if (BackgroundKeyboardInput.Instance.keyDownThisFrame(passChar))
            {
                //Debug.Log(passChar + "!");
                _passwordCheckIdx++;
                if (_passwordCheckIdx >= password.Length)
                {
                    //Debug.LogError("YAY");
                    _passwordCheckIdx = 0;
                    this.showSetup(true);
                }
            }
            else
            {
                _passwordCheckIdx = 0;
                passChar = password[0];
                if (BackgroundKeyboardInput.Instance.keyDownThisFrame(passChar))
                {
                    _passwordCheckIdx = 1;
                }

            }
        }
        
    }

    void Update()
    {
        bool allowMenu = !SwitcherSettings.Data._SecurityMode || !passwordRequired;
        #if UNITY_EDITOR
        allowMenu = true;
        #endif



        if (CrockoInput.GetAdminMenuKeyComboDown() && (isOpen || allowMenu))
        {
            this.showSetup(!isOpen);
        }
    }

    public void restoreDefaults()
    {
        SwitcherSettings.ClearSaveData();
        SwitcherSettings.Data.OnValuesUpdated();
    }

    System.Text.StringBuilder _auditStringBuilder = new System.Text.StringBuilder();
    public void auditGames()
    {
        _auditStringBuilder.Clear();
        foreach (GameData dat in GameCatalog.Instance.games)
        {

            dat.Audit(_auditStringBuilder);
        }

        resultMessageOutput.text = _auditStringBuilder.ToString();
        Debug.Log(_auditStringBuilder);
    }
    
}
