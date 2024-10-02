using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using UnityEngine.EventSystems;
using System;

public class SpeedyListItem : MonoBehaviour {

    [SerializeField]
    TextMeshProUGUI _titleText;

    [SerializeField]
    TextMeshProUGUI _designerText;

    [System.NonSerialized]
    GameData _gameData = null;
    [SerializeField]
    SizeByText _barSizer;

    [SerializeField] Graphic _clickableArea;

    Vector3 _initialPosition;
    public float hoveredAnimAmount = 0;

    public Vector3 initialPosition => _initialPosition;

    [SerializeField]
    Image _tabImage;

    CanvasGroup _canvasGroup;

    readonly Vector3[] _scaleFlips = new Vector3[]
    {
        new Vector3( 1, 1, 1),
        new Vector3(-1,-1, 1),
        new Vector3(-1, 1, 1),
        new Vector3( 1,-1, 1),

    };

    public void SetTabFlipIndex(int i)
    {
        this._tabImage.transform.localScale = _scaleFlips[i % _scaleFlips.Length]; 
    }

    StringBuilder _sbShared = new StringBuilder(128);
    public GameData gameData
    {
        get { return _gameData; }
        set
        {
            _gameData = value;
            title = gameData?.title;
            _sbShared.Clear();
            _sbShared.Append(gameData?.designers);
           // _designerText.text = gameData?.designers;
            if (!string.IsNullOrEmpty(gameData?.year))
            {
                _sbShared.Append(" (");
                _sbShared.Append(gameData.year);
                _sbShared.Append(")");
            }
            _designerText.text = _sbShared.ToString();
           _barSizer?.ForceUpdate();
        }
    }

    public string title
    {
        get
        {
            return _titleText.text;
        }

        set
        {
            _titleText.text = value;
            _barSizer?.ForceUpdate();
        }
    }


    public Color titleColor
    {
        get
        {
            return _titleText.color;
        }

        set
        {
            _titleText.color = value;
        }
    }

    public float alpha
    {
        get
        {
            return this._canvasGroup.alpha;
        }

        set
        {
            this._canvasGroup.alpha = value;
        }

    }

    // Use this for initialization
    void Awake () {
        _barSizer = this.GetComponentInChildren<SizeByText>(true);
        _canvasGroup = this.GetComponent<CanvasGroup>();
        if (_clickableArea != null)
        {
            var et = _clickableArea.gameObject.AddComponent<EventTrigger>();
            {
                var ete = new EventTrigger.Entry();
                ete.eventID = EventTriggerType.PointerClick;
                ete.callback.AddListener(OnClicked);
                et.triggers.Add(ete);
            }

            {
                var ete = new EventTrigger.Entry();
                ete.eventID = EventTriggerType.PointerEnter;
                ete.callback.AddListener((evt) => OnPointerEnter(true));
                et.triggers.Add(ete);
            }

            {
                var ete = new EventTrigger.Entry();
                ete.eventID = EventTriggerType.PointerExit;
                ete.callback.AddListener((evt) => OnPointerEnter(false));
                et.triggers.Add(ete);
            }
        }
        Reinit();
    }

    private void OnPointerEnter(bool hovered)
    {
        SpeedyListView.instance.NotifyItemHovered(this, hovered);
    }

    void OnClicked(BaseEventData e)
    {
        SpeedyListView.instance.setActiveItem(this);
        MenuVisualsGeneric.Instance.openArbitraryGame(this.gameData);
    }

    public void Reinit()
    {
        _initialPosition = this.transform.localPosition;
    }
	

	// Update is called once per frame
	void Update () {
		
	}

    private void OnValidate()
    {
        //if (_barSizer == null)
        //{
        //    _barSizer = this.GetComponentInChildren<SizeByText>(true);
        //}

        //if (_tabImage == null)
        //{
        //    _tabImage = _barSizer.GetComponentInChildren<Image>();
        //}
    }
}
