using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpeedyListItem : MonoBehaviour {

    [SerializeField]
    TextMeshProUGUI _titleText;

    [SerializeField]
    TextMeshProUGUI _designerText;

    GameData _gameData = null;
    [SerializeField]
    SizeByText _barSizer;

    Vector3 _initialPosition;
    public Vector3 initialPosition => _initialPosition;

    [SerializeField]
    Image _tabImage;

    CanvasGroup _canvasGroup;

    public GameData gameData
    {
        get { return _gameData; }
        set
        {
            _gameData = value;
            title = gameData?.title;
            _designerText.text = gameData?.designers;
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

    public float darkenedAmount
    {
        set
        {
            this._tabImage.color = Color.Lerp(Color.white, new Color(.6f,.8f, 1), value);
            this._titleText.color = this._titleText.color.withAlpha(1 - value * .3f);
            this._designerText.color = this._titleText.color.withAlpha(1 - value * .3f);
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
