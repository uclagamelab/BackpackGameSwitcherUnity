using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpeedyListItem : MonoBehaviour {

    [SerializeField]
    TextMeshProUGUI _titleText;

    [SerializeField]
    TextMeshProUGUI _designerText;

    GameData _gameData = null;

    public GameData gameData
    {
        get { return _gameData; }
        set
        {
            _gameData = value;
            title = gameData?.title;
            _designerText.text = gameData?.designers;
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
        }
    }


    public Color color
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

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //private void OnValidate()
    //{
    //    if (_text == null)
    //    {
    //        _text = this.GetComponentInChildren<TextMeshProUGUI>();
    //    }
    //}
}
