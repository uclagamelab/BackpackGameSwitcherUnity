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
        _saveRawJsonButton.onClick.AddListener(onSaveRawJsonButtonClicked);
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
}
