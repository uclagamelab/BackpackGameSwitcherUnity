using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameListItem : MonoBehaviour
{
    [NonSerialized]
    public GameData game;

    [SerializeField]
    Image _highlightImage;

    [SerializeField]
    Text _text;

    [SerializeField]
    CanvasGroup _canvasGroup;

    public string text
    {
        get
        {
            return _text.text;
        }

        set
        {
            _text.text = value;
        }
    }
    bool _isHighlighted = false;
    // Start is called before the first frame update

    bool _wasSelectedGame = false;

    void Start()
    {
      
        EventTrigger.Entry mouseEnter = new EventTrigger.Entry();
        mouseEnter.eventID = EventTriggerType.PointerEnter;
        mouseEnter.callback.AddListener(onMouseEnter);

        EventTrigger.Entry mouseExit = new EventTrigger.Entry();
        mouseExit.eventID = EventTriggerType.PointerExit;
        mouseExit.callback.AddListener(onMouseExit);

        EventTrigger.Entry mouseClick = new EventTrigger.Entry();
        mouseClick.eventID = EventTriggerType.PointerClick;
        mouseClick.callback.AddListener(onMouseClicked);

        EventTrigger et = this.GetComponent<EventTrigger>();

        et.triggers.Add(mouseEnter);
        et.triggers.Add(mouseExit);
        et.triggers.Add(mouseClick);

    }

    
    void Update()
    {
        if (this.game == null)
        {
            return;
        }

        bool isSelectedGame = (GameInfoEditor.instance.currentSelectedGame == this.game);
        bool shouldBeHighlighted = isSelectedGame || _isHighlighted;

        if (shouldBeHighlighted != _highlightImage.enabled)
        {
            _highlightImage.color = isSelectedGame ? Color.magenta.withAlpha(.5f) : Color.green.withAlpha(.5f);

            _highlightImage.enabled = _isHighlighted;
        }

        if (isSelectedGame && _wasSelectedGame)
        {
            _highlightImage.color = Color.magenta.withAlpha(.5f);
        }

        _wasSelectedGame = isSelectedGame;

        this._canvasGroup.alpha = this.game.isFilteredOut ? .6f : 1;

    }

     void onMouseEnter(BaseEventData bed)
    {
        _isHighlighted = true;
    }

     void onMouseExit(BaseEventData bed)
    {
        _isHighlighted = false;
    }

    void onMouseClicked(BaseEventData bed)
    {
        GameInfoEditor.instance.SetSelectedGame(this.game);
    }
}
