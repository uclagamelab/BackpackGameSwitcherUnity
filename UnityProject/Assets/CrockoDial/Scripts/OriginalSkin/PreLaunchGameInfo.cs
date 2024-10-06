using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PreLaunchGameInfo : MonoBehaviour {
    static XUSingleTown<PreLaunchGameInfo> _InstanceHelper = new XUSingleTown<PreLaunchGameInfo>();
    public static PreLaunchGameInfo Instance => _InstanceHelper.instance;

    public enum OpenState {closed, opening, open, closing};
    public static event System.Action<OpenState> OnPrelaunchCanvasOpenChange = (state) => { };

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
        _backButton.SetUp(this);
        _playButton.SetUp(this);    
    }

    void Start () {

        _dimmer.enabled = false;
        _dimmerFullColor = _dimmer.color;
        _highlightImg = _buttonHighlight.GetComponentInChildren<Image>();

        _canvasGroup = this.GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (!animating && open && MenuVisualsGeneric.Instance.state == MenuVisualsGeneric.MenuState.ChooseGame)
        {
            this.AnimateOpen(false);
        }
        _canvasGroup.blocksRaycasts = _canvasGroup.alpha >= 1;
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
        var highLightPrev = this._highlightedObject;

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

        if (highLightPrev != this._highlightedObject && highLightPrev != null)
        {
            MenuVisualsGeneric.Instance.notifyInfoCursorMoved();
        }
    }


    public bool backButtonHighighted
    {
        get
        {
            return _highlightedObject == null || _highlightedObject == _backButton.rt.gameObject;
        }
    }

    public void AnimateOpen(bool forward)
    {
        if (animating)
        {
            return;
        }

        if (forward)
        {
            GameInfoUI.Instance.SetGame(MenuVisualsGeneric.Instance.currentlySelectedGame);
        }

        animating = true;
        open = forward;

        OnPrelaunchCanvasOpenChange.Invoke(open ? OpenState.opening : OpenState.closing);

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
                OnPrelaunchCanvasOpenChange.Invoke(open ? OpenState.open : OpenState.closed);
            }
        }, .5f);
    }

    [System.Serializable]
    public class PreLaunchButtons
    {
        public RectTransform rt;
        public GentleJitterAnim jitter;
        PreLaunchGameInfo _screen;

        public void SetUp(PreLaunchGameInfo screen)
        {
            this._screen = screen;
            var et = rt.gameObject.AddComponent<EventTrigger>();
            
            var entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((evt) =>
            {
                if (_screen._highlightedObject != this.rt.gameObject && !_screen.animating)
                {
                    _screen.setHighlighted(this.rt);
                }
            });
            

            var clickEntry = new EventTrigger.Entry();
            clickEntry.eventID = EventTriggerType.PointerClick;
            clickEntry.callback.AddListener((evt) =>
            {
                MenuVisualsGeneric.Instance.onStartGameButtonPress();
            });

            et.triggers = new List<EventTrigger.Entry> { clickEntry, entry };
            
        }
    }
}
