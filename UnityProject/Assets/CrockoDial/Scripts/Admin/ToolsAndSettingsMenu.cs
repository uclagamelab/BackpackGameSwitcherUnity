﻿using Crosstales.FB;
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

    [SerializeField]
    FileSelectorButton _gamesDirectoryFileSelector;

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

        //TODO: this feels a little gross, repopulating the catalog should be better encapsulated?
        //there should be something more automatic determining that the games path has been changed???
        _gamesDirectoryFileSelector.OnValidPathChosen += (newPath) => 
        {
            GameCatalog.Instance.repopulateCatalogFromDisk(newPath);
        };

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

        //repopulating should only happen if the games directories change (?)
        //Apply the settings
        //GameCatalog.Instance.repopulateCatalogFromDisk(SwitcherSettings.Data.GamesFolder);
    }


    int _passwordCheckIdx = 0;
    bool _allowPassPress = true;


    bool passwordRequired => !string.IsNullOrEmpty(SwitcherSettings.Data._SecurityModePassword);
    //NOTE: following password checking needs to be in late update,
    ///for to 'BackgroundKeyboardInput.Instance.keyDownThisFrame'
    //to work correctly.



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

        checkForSecurityModePassword();
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

    #region Password Related
    private void checkForSecurityModePassword()
    {
        var password = SwitcherSettings.Data._SecurityModePassword;
        if (isOpen || !passwordRequired)
        {
            return;
        }

        char passChar = password[_passwordCheckIdx];

        if (Input.anyKeyDown)
        {
            //Debug.LogError(passChar + "!");
            if (KeyDownFromChar(passChar, ActionPhase.down))
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
            //for this to work, would have to store the whole string one entered
            //else if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
            //{
            //    _passwordCheckIdx = Mathf.Max(0, _passwordCheckIdx - 1);
            //}
            else if (disqualifyingKeyDown())
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
    public enum ActionPhase { down=1, held=0, up=-1 }

    static readonly KeyCode[] allowedKeys = { KeyCode.LeftShift, KeyCode.RightShift, KeyCode.Delete };
    public static bool disqualifyingKeyDown()
    {
        foreach (var kc in allowedKeys) {
            if (Input.GetKeyDown(kc)) return false;
        }
        return true;
    }

    public static bool KeyDownFromChar(char c, ActionPhase phase)
    {
        bool ret = false;

        //bool isTopRowSymbol = c >= '!' && c <= ')';
        //bool isNumber = c >= '0' && c <= '9';
        bool isLowerCaseLetter = c >= 'a' && c <= 'z';
        bool isUpperCaseLetter = c >= 'A' && c <= 'Z';
        bool isNumber = c >= '0' && c <= '9';
        KeyCode keyCode;
        bool needsShift = false;
        if (isUpperCaseLetter)
        {
            keyCode = (KeyCode) (c + ('a' - 'A'));
            needsShift = true;
        }
        else if (isLowerCaseLetter)
        {
            keyCode = (KeyCode)c;
            needsShift = false;
        }
        else
        {
            keyCode = (KeyCode)c;
        }
    

        if (phase == ActionPhase.down)
        {
            ret = Input.GetKeyDown(keyCode);
        }
        else if (phase == ActionPhase.held)
        {
            ret = Input.GetKey(keyCode);
        }
        else if (phase == ActionPhase.up)
        {
            ret = Input.GetKeyUp(keyCode);
        }

        bool shiftOK = !needsShift || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        ret &= shiftOK;

        return ret;
    }
    #endregion
}

