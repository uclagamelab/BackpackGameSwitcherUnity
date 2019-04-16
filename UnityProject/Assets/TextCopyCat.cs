using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextCopyCat : MonoBehaviour {

    [SerializeField]
    SpeedyListItem _textToCopy;

    [SerializeField]
    TMPro.TextMeshProUGUI _text;

    [SerializeField]
    TMPro.TextMeshProUGUI _authorText;

    // Use this for initialization
    void Start () {
		
	}

    string _lastAppliedText = null;
	// Update is called once per frame
	void LateUpdate ()
    {
        this.transform.position = _textToCopy.transform.position;
        this.transform.rotation = _textToCopy.transform.rotation;
        this.transform.localScale = _textToCopy.transform.localScale;
        _text.color = _text.color.withAlpha(_textToCopy.color.a);
        _authorText.color = _authorText.color.withAlpha(_textToCopy.color.a);
        
        if (_textToCopy.title != _lastAppliedText)
        {
            _lastAppliedText = _textToCopy.title;
            _text.text = _textToCopy.title;
            int yearFaked = 1990 + Random.Range(0, 35);
            _authorText.text = _textToCopy.gameData.designers + " (" + yearFaked + ")";

        }

    }
}
