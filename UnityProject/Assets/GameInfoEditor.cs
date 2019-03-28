using System.Collections;
using System.Collections.Generic;
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
        public InputField _descriptionField;
        public Button _saveChangesButton;

        //public InputField _exeNameField;

        public void SetUp()
        {
            _saveChangesButton.onClick.AddListener(()=>flushToGame(GameInfoEditor.instance.currentSelectedGame));
        }

        public void UpdateWithGame(GameData game)
        {
            _titleField.text = game.title;
            _authorField.text = game.author;
            _windowTitleField.text = game.windowTitle;
            _joyToKeyField.text = game.joyToKeyConfigFile;
            _descriptionField.text = game.description;
        }

        void flushToGame(GameData game)
        {
            game.title = _titleField.text;
            game.author = _authorField.text;
            game.windowTitle = _windowTitleField.text;
            game.joyToKeyConfigFile = _joyToKeyField.text;
            game.description = _descriptionField.text;
            game.flushChangesToJson();
        }
    }
}
