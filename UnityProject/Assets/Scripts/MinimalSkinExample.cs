using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimalSkinExample : MonoBehaviour
{
    public Button _playGameButton;
    public Button _nextGameButton;
    public RawImage _videoDisplay;
    public TMPro.TextMeshProUGUI _descriptionText;
    public TMPro.TextMeshProUGUI _titleText;

    int _currentSelectedGameIdx = 0;
    GameData currentGameData => GameCatalog.Instance.games[_currentSelectedGameIdx];

    void Start()
    {
        //_playGameButton.onClick.AddListener();
        _playGameButton.onClick.AddListener(() => 
        {
            ProcessRunner.instance.StartGame(currentGameData);
        });

        _nextGameButton.onClick.AddListener(() =>
        {
            _currentSelectedGameIdx++;
            _currentSelectedGameIdx %= GameCatalog.Instance.gameCount;
            refreshVisuals();
        });
        refreshVisuals();
    }

    void refreshVisuals()
    {
        _titleText.text = currentGameData.title;
        _descriptionText.text = currentGameData.description;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
