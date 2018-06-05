﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

public class BlurredOverlay : MonoBehaviour {

    [SerializeField]
    CameraBlurrer bgBlurrer;

    [SerializeField]
    RawImage _rawImg;

    [SerializeField]
    RawImage _dimmer;
    Color _dimmerFullColor;

    [SerializeField]
    RectTransform _buttonHighlight;
    Image _highlightImg;

    [SerializeField]
    RectTransform _backButton;

    [SerializeField]
    RectTransform _playButton;

    bool animatedIn = false;

	// Use this for initialization
	void Start () {
        //_rawImg = this.GetComponent<RawImage>();
        _rawImg.enabled = false;
        _dimmer.enabled = false;
        _dimmerFullColor = _dimmer.color;
        _highlightImg = _buttonHighlight.GetComponentInChildren<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("y"))
        {

            animateIn(!animatedIn);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            TakeInput(-1);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            TakeInput(1);
        }
    }

    void TakeInput(int direction)
    {
        if (getHighlighted() == _backButton && direction == 1)
        {
            setHighlighted(_playButton);
        }
        else if (getHighlighted() == _playButton && direction == -1)
        {
            setHighlighted(_backButton);
        }
    }

    void setHighlighted(RectTransform toHighlight)
    {
        if (toHighlight == null)
        {
            _highlightImg.color = Color.white.withAlpha(0);
        }

        if (toHighlight == _playButton || toHighlight == null)
        {
            _buttonHighlight.anchoredPosition = _playButton.anchoredPosition;
        }

        if (toHighlight == _backButton)
        {
            _buttonHighlight.anchoredPosition = _backButton.anchoredPosition;
        }
    }

    RectTransform getHighlighted()
    {
        if (_highlightImg.color.a < .9f)
        {
            return null;
        }

        if (_buttonHighlight.anchoredPosition == _playButton.anchoredPosition)
        {
            return _playButton;
        }
        else //if (_buttonHighlight.anchoredPosition == _backButton.anchoredPosition)
        {
            return _backButton;
        }


    }

    void animateIn(bool forward)
    {
        animatedIn = forward;
        _rawImg.enabled = true;
        bgBlurrer.enabled = true;
        _dimmer.enabled = true;
        if (forward)
        {
            setHighlighted(null);
        }
       this.varyWithT((rawT) =>
        {
            float t = forward ? rawT : 1 - rawT;
            Color color = Color.white;
            color.a = Mathf.InverseLerp(0, .75f, t);
            float elastT = EasingFunctions.Calc(t, EasingFunctions.ExpoEaseOut);
        this.transform.localScale = Vector3.LerpUnclamped(Vector3.one * 3, Vector3.one, elastT);// EasingFunctions.Calc(t, EasingFunctions.QuadEaseOut));
            _rawImg.material.color = color;
            _rawImg.material.SetFloat("_BlurAmt", Mathf.InverseLerp(1, 0.9f, t));
            _rawImg.material.SetFloat("_MipsBias", 6*Mathf.InverseLerp(1, 0, t));
            _rawImg.material.SetFloat("_BlurDist", 12 * (1-t));

            bgBlurrer.blurAmount = t;
            _dimmer.color = Color.Lerp(_dimmerFullColor.withAlpha(0), _dimmerFullColor, t);

            _highlightImg.color = Color.white.withAlpha(Mathf.InverseLerp(.5f, 1,t));
        }, .5f);

        
    }
}