﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public interface IGameInfoEditorListener
{
    public void OnGameChange(GameData newGame);
}

public class GameInfoEditor : MonoBehaviour
{
    public static Events events = new Events();
      
    public static GameInfoEditor instance
    {
        get;
        private set;
    }

    [SerializeField]
    ScrollRect _gameListScrollRect;

    [SerializeField]
    GameListItem _gamesListItemTemplate;

    [SerializeField]
    InputField _jsonEditor;

    [Header("SubMenus")]
    [SerializeField]
    GameObject _jsonEditorPanel;
    [SerializeField]
    GameObject _ezEditorPanel;
    [SerializeField]
    LaunchSettingsEditor _startUpSettingsPanel;

    [SerializeField]
    EzEditor _ezEditor;
 

    [Header("Buttons")]
    [SerializeField]
    Button _rawJsonModeButton;

    [SerializeField]
    Button _ezEditModeButton;

    //[SerializeField]
    //Button _saveRawJsonButton;

    [SerializeField]
    Button _startupSettingsButton;

    public Button _applyButton;
    

    public GameData currentSelectedGame
    {
        get;
        private set;
    }

    IGameInfoEditorListener[] _listeners;

    void Start()
    {
        //GameCatalog.Instance.games
        _gamesListItemTemplate.gameObject.SetActive(false);

        //TODO: redo this this we repopulating/changing folder
        rebuildList();

        GameCatalog.Events.OnRepopulated += rebuildList;
    }

    void rebuildList()
    {
        foreach (Transform child in _gameListScrollRect.content.transform)
        {
            if (child.gameObject != _gamesListItemTemplate.gameObject)
            {
                Destroy(child.gameObject);
            }
        }

        foreach (GameData gam in GameCatalog.Instance.allGames)
        {
            GameListItem nuItem = GameObject.Instantiate(_gamesListItemTemplate.gameObject).GetComponent<GameListItem>();
            nuItem.game = gam;
            nuItem.gameObject.SetActive(true);
            nuItem.text = gam.title;
            nuItem.transform.SetParent(_gameListScrollRect.content, false);
        }
    }

    private void OnEnable()
    {
        if (_listeners == null)
        {
            _listeners = this.GetComponentsInChildren<IGameInfoEditorListener>(true);
        }

        if (MenuVisualsGeneric.Instance != null)
        {
            SetSelectedGame(MenuVisualsGeneric.Instance.currentlySelectedGame);
        }
    }

    public void SetSelectedGame(GameData nuSelection)
    {
        currentSelectedGame = nuSelection;
        events.OnSelectedGameChanged.Invoke();


        // -- If in raw json edit mode --------------------------
        //if (_jsonEditorPanel.activeInHierarchy)
        //{
            updateJsonEditorWithGame(nuSelection);
        //}

        //if (_ezEditorPanel.activeInHierarchy)
        //{
            _ezEditor.UpdateWithGame(nuSelection);
        //}

        //TODO: may use existing event?
        foreach(var l in _listeners)
        {
            l.OnGameChange(currentSelectedGame); 
        }
    }


    void updateJsonEditorWithGame(GameData nuSelection)
    {
        if (nuSelection == null)
        {
            _jsonEditor.text = "";

        }
        else
        {
            _jsonEditor.text = nuSelection.GetInfoJSON();
        }
    }

    private void Awake()
    {
        instance = this;


        _ezEditModeButton.onClick.AddListener(()=>ShowPanel(_ezEditorPanel));
        _rawJsonModeButton.onClick.AddListener(
            ()=>
            {
                ShowPanel(_jsonEditorPanel);
                if (currentSelectedGame != null)
                {
                    updateJsonEditorWithGame(currentSelectedGame);
                }
            }
            );
        _startupSettingsButton.onClick.AddListener(() => ShowPanel(_startUpSettingsPanel));

        _applyButton.onClick.AddListener(() =>
        {
            if (_jsonEditorPanel.activeInHierarchy)
            {
                onSaveRawJsonButtonClicked();
            }
            else
            {
                if (_ezEditorPanel.activeInHierarchy)
                {

                    _ezEditor.ApplyChangesToGameDataInMemory();
                }
                else if (_startUpSettingsPanel.gameObject.activeInHierarchy)
                {
                    _startUpSettingsPanel.ApplyChangesToGameDataInMemory();

                }
                GameInfoEditor.instance.currentSelectedGame.flushChangesToDisk();
            }
          
            });
        

        _ezEditor.SetUp();
    }

    public void ShowPanel(object panel)
    {
        this._ezEditorPanel.SetActive((object)_ezEditorPanel == panel);
        this._jsonEditorPanel.SetActive((object)_jsonEditorPanel == panel);
        this._startUpSettingsPanel.gameObject.SetActive((object)_startUpSettingsPanel == panel);
    }



    void onSaveRawJsonButtonClicked()
    {
        if (currentSelectedGame != null)
        {
            currentSelectedGame.WriteJSON(_jsonEditor.text);
            JsonUtility.FromJsonOverwrite(_jsonEditor.text, currentSelectedGame);
        }
    }


    public void OpenSelectedFolderInWindowsExplorer()
    {
        if (Screen.fullScreen)
        {
            Screen.fullScreen = false;
        }
        XuFileUtil.OpenPathInExplorer(currentSelectedGame.rootFolder.FullName);
    }
    


    public class Events
    {
        public System.Action OnSelectedGameChanged = () => { };
    }


    [System.Serializable]
    public class EzEditor
    {
        public InputField _titleField;
        public InputField _authorField;
        public InputField _yearField;
        public InputField _windowTitleField;
        public InputField _joyToKeyField;
        public InputField _exePathField;
        public Button _exeBrowseFileButton;


        public InputField _descriptionField;
        public InputField _instructionsField;
        public InputField _notesField;

        public Toggle _descriptionFieldToggle;
        public Toggle _instructionsFieldToggle;
        public Toggle _notesFieldToggle;

        //public InputField _exeNameField;

        GameData _currentGame;

        public void SetUp()
        {

            FileSelectorButton j2kFsb = _joyToKeyField.transform.parent.GetComponentInChildren<FileSelectorButton>();
            if (j2kFsb != null)
            {
                j2kFsb.OnValidPathChosen += (dontCare)=>
                {
                    if (_currentGame != null)
                    {
                        ApplyChangesToGameDataInMemory();
                        _currentGame.flushChangesToDisk();
                    }
                };
            }

            _exeBrowseFileButton.onClick.AddListener(OnClickChooseExeButton);
            //FileSelectorButton exeFsb = _exePathField.transform.parent.GetComponentInChildren<FileSelectorButton>();
            //if (exeFsb != null)
            //{
            //    exeFsb.OnValidPathChosen += OnExeForGameChosen;
            //}

            //_titleField.onEndEdit.AddListener((s)=> ApplyChangesToGameDataInMemory());
            //_authorField.onEndEdit.AddListener((s) => ApplyChangesToGameDataInMemory());
            //_windowTitleField.onEndEdit.AddListener((s) => ApplyChangesToGameDataInMemory());
            //_descriptionField.onEndEdit.AddListener((s) => ApplyChangesToGameDataInMemory());

            _instructionsFieldToggle.onValueChanged.AddListener(refreshActiveTextField);
            _descriptionFieldToggle.onValueChanged.AddListener(refreshActiveTextField);
            _notesFieldToggle.onValueChanged.AddListener(refreshActiveTextField);
        }
        static readonly Crosstales.FB.ExtensionFilter[] exeFilters = new Crosstales.FB.ExtensionFilter[]{

            new Crosstales.FB.ExtensionFilter("exe", "exe", "bat", "lnk")
        };
        void OnClickChooseExeButton()
        {
            string exeFullPath = Crosstales.FB.FileBrowser.OpenSingleFile("Select Exe", _currentGame.rootFolder.FullName, exeFilters);
            if (!string.IsNullOrEmpty(exeFullPath))
            {
                _exePathField.text = exeFullPath;
                Uri exeUri = new Uri(exeFullPath);
                Uri gameDirPath = new Uri(_currentGame.rootFolder.FullName);
                Uri relPathUri = gameDirPath.MakeRelativeUri(exeUri);
                string relPath = Uri.UnescapeDataString(relPathUri.ToString());//HttpUtility.HtmlDecode(relPathUri.ToString());
                int rootFolderPortion = (_currentGame.rootFolder.Name.Length + 1);
                string finalRelPath = relPath.Substring(rootFolderPortion, relPath.Length - rootFolderPortion);
                _exePathField.text = finalRelPath;

                ApplyChangesToGameDataInMemory();
            }

        }

        void refreshActiveTextField(bool _ = false)
        {
            _descriptionField.gameObject.SetActive(_descriptionFieldToggle.isOn);
            _instructionsField.gameObject.SetActive(_instructionsFieldToggle.isOn);
            _notesField.gameObject.SetActive(_notesFieldToggle.isOn);
        }

        public void UpdateWithGame(GameData game)
        {
            _currentGame = game;

            if (game == null)
            {
                return;
            }

            _titleField.text = game.title;
            _authorField.text = game.designers;
            _yearField.text = game.year;

            _windowTitleField.text = game.windowTitle;
            _joyToKeyField.text = game.joyToKeyConfig;

            _descriptionField.text = game.description;
            _instructionsField.text = game.howToPlay;
            _notesField.text = game.notes;

            _exePathField.text = game.exePath;

            refreshActiveTextField();
        }


        public void ApplyChangesToGameDataInMemory()
        {
            if (_currentGame == null)
            {
                return;
            }
            GameData game = _currentGame;
            game.title = _titleField.text;
            game.designers = _authorField.text;
            game.windowTitle = _windowTitleField.text;
            game.joyToKeyConfig = _joyToKeyField.text;
            game.year = _yearField.text;

            game.description = _descriptionField.text;
            game.howToPlay = _instructionsField.text;
            game.notes = _notesField.text;

            game.exePath = _exePathField.text;
            game.flushChangesToDisk();
        }
    }


}
