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
    RawImage _dimmer;
    Color _dimmerFullColor;

    [SerializeField]
    RectTransform _buttonHighlight;
    Image _highlightImg;

    [SerializeField]
    PreLaunchButtons _backButton;

    [SerializeField]
    PreLaunchButtons _playButton;



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

        _dimmer.enabled = false;
        _dimmerFullColor = _dimmer.color;
        _highlightImg = _buttonHighlight.GetComponentInChildren<Image>();

        _canvasGroup = this.GetComponent<CanvasGroup>();
    }

    // Update is called once per frame
    void Update()
    {

        int idx = MenuVisualsGeneric.Instance.gameIdx;
        //idx = Mathf.Clamp(idx, 0, this.overlays.Length - 1);
        //Texture newTexture = this.overlays[idx];
        

        if (!animating && open && MenuVisualsGeneric.Instance.state == MenuVisualsGeneric.MenuState.ChooseGame)
        {
            this.AnimateOpen(false);
        }
    }

    public bool TakeDirectionInput(int direction)
    {
        bool accepted = false;

        if (animating)
        {
            return false;
        }


        if (backButtonHighighted && direction == 1)
        {
            setHighlighted(_playButton.rt);
            accepted = true;
        }
        else if (!backButtonHighighted && direction == -1)
        {
            setHighlighted(_backButton.rt);
            accepted = true;
        }

        return accepted;
    }

    void setHighlighted(RectTransform toHighlightRaw)
    {
        RectTransform toHighlight = toHighlightRaw;
        if (toHighlight == null)
        {
            toHighlight = _backButton.rt;
        }

        bool hightlightBackButton = toHighlight == _backButton.rt || toHighlight == null;
        _playButton.rt.GetComponentInChildren<Text>().color = !hightlightBackButton ? Color.yellow : Color.white;
        _backButton.rt.GetComponentInChildren<Text>().color = hightlightBackButton ? Color.yellow : Color.white;

        _playButton.rt.transform.localScale = (!hightlightBackButton ? 1 : .8f) * Vector3.one;
        _backButton.rt.transform.localScale = ( hightlightBackButton ? 1 : .8f) * Vector3.one;

        _playButton.jitter.on = !hightlightBackButton;
        _backButton.jitter.on =  hightlightBackButton;

        if (toHighlight == _playButton.rt)
        {
            _highlightedObject = _playButton.rt.gameObject;
            _buttonHighlight.anchoredPosition = _playButton.rt.anchoredPosition;
            
        }

        if (hightlightBackButton)
        {
            _highlightedObject = _backButton.rt.gameObject;
            _buttonHighlight.anchoredPosition = _backButton.rt.anchoredPosition;
        }
    }

    public bool startButtonHighighted
    {
        get
        {
            return getHighlighted() == _playButton.rt.gameObject;
        }
    }

    public bool backButtonHighighted
    {
        get
        {
            return getHighlighted() == null || getHighlighted() == _backButton.rt.gameObject;
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
      
        //bgBlurrer.enabled = true;
        _dimmer.enabled = true;
        if (forward)
        {

            setHighlighted(_backButton.rt);
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

    [System.Serializable]
    public class PreLaunchButtons
    {
        public RectTransform rt;
        public GentleJitterAnim jitter;
    }
}
