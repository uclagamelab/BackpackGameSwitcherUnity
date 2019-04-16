using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpeedyListItem : MonoBehaviour {

    [SerializeField]
    TextMeshProUGUI _text;
    public GameData gameData = null;

    public string title
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

    public Color color
    {
        get
        {
            return _text.color;
        }

        set
        {
            _text.color = value;
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
