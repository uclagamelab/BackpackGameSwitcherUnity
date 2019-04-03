using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

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
    EzEditor _ezEditor;

    [Header("Buttons")]
    [SerializeField]
    Button _rawJsonModeButton;

    [SerializeField]
    Button _ezEditModeButton;

    [SerializeField]
    Button _saveRawJsonButton;



    public GameData currentSelectedGame
    {
        get;
        private set;
    }
    public void SetSelectedGame(GameData nuSelection)
    {
        currentSelectedGame = nuSelection;
        events.OnSelectedGameChanged.Invoke();


        // -- If in raw json edit mode --------------------------
        if (_jsonEditorPanel.activeInHierarchy)
        {
            updateJsonEditorWithGame(nuSelection);
        }

        if (_ezEditorPanel.activeInHierarchy)
        {
            updateEzEditorWithGame(nuSelection);
        }
    }

    void updateEzEditorWithGame(GameData nuSelection)
    {
        _ezEditor.UpdateWithGame(nuSelection);
    }

    void updateJsonEditorWithGame(GameData nuSelection)
    {
        if (nuSelection == null)
        {
            _jsonEditor.text = "";

        }
        else
        {
            _jsonEditor.text = nuSelection.GetJSON();
        }
    }

    private void Awake()
    {
        instance = this;


        _ezEditModeButton.onClick.AddListener(OnEzEditModeButtonPressed);
        _rawJsonModeButton.onClick.AddListener(OnRawJSONEditModeButtonPressed);
        _ezEditor.SetUp();

        _saveRawJsonButton.onClick.AddListener(onSaveRawJsonButtonClicked);
    }


    void OnEzEditModeButtonPressed()
    {
        _ezEditorPanel.SetActive(true);

        _jsonEditorPanel.SetActive(false);
    }

    void OnRawJSONEditModeButtonPressed()
    {
        _jsonEditorPanel.SetActive(true);

        _ezEditorPanel.SetActive(false);
    }

    void onSaveRawJsonButtonClicked()
    {
        if (currentSelectedGame != null)
        {
            currentSelectedGame.WriteJSON(_jsonEditor.text);
            JsonUtility.FromJsonOverwrite(_jsonEditor.text, currentSelectedGame);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //GameCatalog.Instance.games
        _gamesListItemTemplate.gameObject.SetActive(false);

        foreach (GameData gam in GameCatalog.Instance.games)
        {
            GameListItem nuItem = GameObject.Instantiate(_gamesListItemTemplate.gameObject).GetComponent<GameListItem>();
            nuItem.game = gam;
            nuItem.gameObject.SetActive(true);
            nuItem.text = gam.title;
            nuItem.transform.parent = _gameListScrollRect.content;
        }
    }

    public void CopyFromLegacyJson()
    {
        string legacyJson = XuFileSystemUtil.LoadTextFromDisk(Path.Combine(currentSelectedGame.rootFolder.FullName, "gameInfo.json"));
        if (legacyJson != null)
        {
            JSONObject job = new JSONObject(legacyJson);
            job.GetField(ref currentSelectedGame.title, "title");
            job.GetField(ref currentSelectedGame.designers, "designers");
            job.GetField(ref currentSelectedGame.description, "description");
            job.GetField(ref currentSelectedGame.joyToKeyConfig_singlePlayer, "joytokey cfg");
            job.GetField(ref currentSelectedGame.windowTitle, "window title");
            _ezEditor.UpdateWithGame(currentSelectedGame);
            //currentSelectedGame.flushChangesToJson();
        }
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
        public InputField _windowTitleField;
        public InputField _joyToKeyField;
        public InputField _exePathField;
        public Button _exeBrowseFileButton;
        public InputField _descriptionField;
        public Button _saveChangesButton;

        //public InputField _exeNameField;

        GameData _currentGame;

        public void SetUp()
        {
            _saveChangesButton.onClick.AddListener(()=>flushToGame(GameInfoEditor.instance.currentSelectedGame));

            FileSelectorButton j2kFsb = _joyToKeyField.transform.parent.GetComponentInChildren<FileSelectorButton>();
            if (j2kFsb != null)
            {
                j2kFsb.OnValidPathChosen += (dontCare)=> flushCurrentGame();
            }

            _exeBrowseFileButton.onClick.AddListener(OnClickChooseExeButton);
            //FileSelectorButton exeFsb = _exePathField.transform.parent.GetComponentInChildren<FileSelectorButton>();
            //if (exeFsb != null)
            //{
            //    exeFsb.OnValidPathChosen += OnExeForGameChosen;
            //}
        }

        void OnClickChooseExeButton()
        {
            string exeFullPath = Crosstales.FB.FileBrowser.OpenSingleFile("Select Exe", _currentGame.rootFolder.FullName, "exe,bat,lnk");
            if (!string.IsNullOrEmpty(exeFullPath))
            {
                _exePathField.text = exeFullPath;
                Uri exeUri = new Uri(exeFullPath);
                Uri gameDirPath = new Uri(_currentGame.rootFolder.FullName);
                Uri relPathUri = gameDirPath.MakeRelativeUri(exeUri);
                string relPath = Uri.UnescapeDataString(relPathUri.ToString());//HttpUtility.HtmlDecode(relPathUri.ToString());
                int rootFolderPortion = (_currentGame.rootFolder.Name.Length + 1);
                string finalRelPath = relPath.Substring(rootFolderPortion, relPath.Length - rootFolderPortion);
                Debug.Log(_currentGame.rootFolder);
                Debug.Log(finalRelPath);
                _exePathField.text = finalRelPath;

                flushCurrentGame();
            }

        }

        public void UpdateWithGame(GameData game)
        {
            _currentGame = game;
            _titleField.text = game.title;
            _authorField.text = game.designers;
            _windowTitleField.text = game.windowTitle;
            _joyToKeyField.text = game.joyToKeyConfig;
            _descriptionField.text = game.description;
            _exePathField.text = game.exePath;
        }

        void flushCurrentGame()
        {
            if (_currentGame != null)
            {
                flushToGame(_currentGame);
            }
        }



        void flushToGame(GameData game)
        {
            game.title = _titleField.text;
            game.designers = _authorField.text;
            game.windowTitle = _windowTitleField.text;
            game.joyToKeyConfig = _joyToKeyField.text;
            game.description = _descriptionField.text;
            //game.exePath.isAbsolute = false;
            game.exePath = _exePathField.text;
            game.flushChangesToJson();
        }
    }


}
