using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimalSkinExample : MonoBehaviour
{
    public Button _playGameButton;
    public Button _prevGameButton;
    public Button _nextGameButton;
    public RawImage _videoDisplay;
    GamePreviewDisplayTexture _previewer;
    public TMPro.TextMeshProUGUI _descriptionText;
    public TMPro.TextMeshProUGUI _titleText;

    int _currentSelectedGameIdx = 0;
    [SerializeField] string _streamingAssetsPlaceholderVideoName = "placeholder_video.mp4";
    GameData currentGameData => GameCatalog.Instance.games[_currentSelectedGameIdx];

    void Start()
    {
        _streamingAssetsPlaceholderVideoName = System.IO.Path.Combine(Application.streamingAssetsPath, _streamingAssetsPlaceholderVideoName);
        _previewer = new GamePreviewDisplayTexture(_videoDisplay);

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
        _prevGameButton.onClick.AddListener(() =>
        {
            _currentSelectedGameIdx--;
            _currentSelectedGameIdx += GameCatalog.Instance.gameCount;
            _currentSelectedGameIdx %= GameCatalog.Instance.gameCount;
            refreshVisuals();
        });
        refreshVisuals();

    }

    void refreshVisuals()
    {
        _titleText.text = currentGameData.title;
        _descriptionText.text = currentGameData.description;
        _previewer.setVideo(currentGameData, _streamingAssetsPlaceholderVideoName);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
