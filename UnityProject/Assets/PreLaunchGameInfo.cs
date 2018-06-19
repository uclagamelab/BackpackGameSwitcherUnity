using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

public class PreLaunchGameInfo : MonoBehaviour {
    public static PreLaunchGameInfo Instance;

    public bool animating
    {
        get;
        private set;
    }

    GameObject _highlightedObject;

    [SerializeField]
    Texture[] overlays;

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

    public bool open
    {
        get;
        private set;
    }

    CanvasGroup _canvasGroup;

    private void Awake()
    {
        Instance = this;
    }

    // Use this for initialization
    void Start () {
        //_rawImg = this.GetComponent<RawImage>();
        _rawImg.enabled = false;
        _dimmer.enabled = false;
        _dimmerFullColor = _dimmer.color;
        _highlightImg = _buttonHighlight.GetComponentInChildren<Image>();

        _canvasGroup = this.GetComponent<CanvasGroup>();
    }

    // Update is called once per frame
    void Update()
    {

        int idx = MenuVisualsGeneric.Instance.gameIdx;
        idx = Mathf.Clamp(idx, 0, this.overlays.Length - 1);
        Texture newTexture = this.overlays[idx];
     
        this._rawImg.texture = newTexture;

        /*if (Input.GetKeyDown("y"))
        {

            AnimateOpen(!open);
        }*/

        /*if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            TakeDirectionInput(-1);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            TakeDirectionInput(1);
        }*/

        if (!animating && open && MenuVisualsGeneric.Instance.state == MenuVisualsGeneric.MenuState.ChooseGame)
        {
            this.AnimateOpen(false);
        }
    }

    public void TakeDirectionInput(int direction)
    {
        if (animating)
        {
            return;
        }


        if (backButtonHighighted && direction == 1)
        {
            setHighlighted(_playButton);
        }
        else if (!backButtonHighighted && direction == -1)
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

        if (toHighlight == _playButton)
        {
            _highlightedObject = _playButton.gameObject;
            _buttonHighlight.anchoredPosition = _playButton.anchoredPosition;
        }

        if (toHighlight == _backButton || toHighlight == null)
        {
            _highlightedObject = _backButton.gameObject;
            _buttonHighlight.anchoredPosition = _backButton.anchoredPosition;
        }
    }

    public bool startButtonHighighted
    {
        get
        {
            return getHighlighted() == _playButton.gameObject;
        }
    }

    public bool backButtonHighighted
    {
        get
        {
            return getHighlighted() == null || getHighlighted() == _backButton.gameObject;
        }
    }

    GameObject getHighlighted()
    {

        return _highlightedObject;


    }

    public void AnimateOpen(bool forward)
    {
        if (animating)
        {
            return;
        }

        animating = true;
        open = forward;
        _rawImg.enabled = true;
        //bgBlurrer.enabled = true;
        _dimmer.enabled = true;
        if (forward)
        {

            setHighlighted(_backButton);
        }
       this.varyWithT((rawT) =>
        {
            float t = forward ? rawT : 1 - rawT;

            _canvasGroup.alpha = Mathf.InverseLerp(0, .75f, t);
            Color color = Color.white;
            color.a = Mathf.InverseLerp(0, .75f, t);
            float elastT = EasingFunctions.Calc(t, EasingFunctions.ExpoEaseOut);
        this.transform.localScale = Vector3.LerpUnclamped(Vector3.one * 3, Vector3.one, elastT);// EasingFunctions.Calc(t, EasingFunctions.QuadEaseOut));
            /*_rawImg.material.color = color;
            _rawImg.material.SetFloat("_BlurAmt", Mathf.InverseLerp(1, 0.9f, t));
            _rawImg.material.SetFloat("_MipsBias", 6*Mathf.InverseLerp(1, 0, t));
            _rawImg.material.SetFloat("_BlurDist", 12 * (1-t));*/

            //bgBlurrer.blurAmount = t;
            _dimmer.color = Color.Lerp(_dimmerFullColor.withAlpha(0), _dimmerFullColor, t);

            _highlightImg.color = Color.white.withAlpha(Mathf.InverseLerp(.5f, 1,t));

            if (t == 1)
            {
                animating = false;
            }
        }, .5f);

        
    }
}
