using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class SpeedyListView : MonoBehaviour
{
   public static SpeedyListView instance
    {
        get;
        private set;
    }
    struct PersonalGameThing
    {
        public string cleanTitle;
        public int realIdx;
        public GameData data;
        public PersonalGameThing(GameData dat, int idx)
        {
            realIdx = idx;
            this.data = dat;
            this.cleanTitle = dat.title.Replace('\n', ' ');
        }
    }
    [Range(0, 1)]
    public float _attractAmt = 0;
    [SerializeField]
    Color _defaultItemColor = Color.black;
    [SerializeField]
    Color _selectedItemColor = Color.magenta;



    List<PersonalGameThing> _things = new List<PersonalGameThing>();

    [SerializeField]
    List<SpeedyListItem> _listItems;

    int _selectedTopIndex
    {
        get
        {
            int ret = Mathf.CeilToInt(_fuzzyIdx);
            if (_scrollMomentumDirection < 0)
            {
                ret = Mathf.FloorToInt(_fuzzyIdx);
            }
            return ret;
        }
    }


    [Space(10)]
    [SerializeField]
    TextMeshProUGUI _alphaHelper;

    [SerializeField]
    CanvasGroup _alphaHelperCanvasGroup;

    RectTransform _container;
    Vector2 _startAp;

    float _fuzzyIdx = 0;
    float fuzzIdxSelectionOffsetted => (_fuzzyIdx + _onScreenSelectionOffset) % _things.Count;

    float _height;
    // Start is called before the first frame update
    float _timeOfLastSwitch = 0;

    int _scrollMomentumDirection = 0;

    float _autoKeyCounter = 0;

    [SerializeField]
    int _onScreenSelectionOffset = 8;
    
    float _speedAccumulator = 0;
    const float _normalSpeed = 5;
    const float _quickSpeed = 11;

    private void Awake()
    {
        instance = this;
        GameCatalog.Events.OnRepopulated += OnRepopulated;
        _container = _listItems[0].transform.parent.GetComponent<RectTransform>();
        _startAp = _container.anchoredPosition;
        _height = _listItems[0].GetComponent<RectTransform>().sizeDelta.y;

        //_alphaHelperCanvasGroup = _alphaHelper.GetComponent<CanvasGroup>();
        SwitcherApplicationController.OnAttractCycleNextGame += AttractCycleNextGame;
    }
    void Start()
    {
        OnRepopulated();
    }
    
    void AttractCycleNextGame()
    {
        _autoKeyCounter = .1f;
    }

    public GameData currentGame
    {
        get
        {
            if (_things.Count > 0)
            {
                int effIdx = (_selectedTopIndex + _onScreenSelectionOffset) % _things.Count;
                if (effIdx < _things.Count)
                {
                    return _things[effIdx].data;
                }
            }

            return null;
        }
    }
    
    void UpdateVideoToCurrentGame()
    {
       
        int setIdx =  Mathf.FloorToInt(fuzzIdxSelectionOffsetted);
        MenuVisualsGeneric.Instance.cycleToNextGame(setIdx - MenuVisualsGeneric.Instance.gameIdx, false, false);
    }

    float _keyHeldTimer = 0;

    public bool keyHeld
    {
        get;
        private set;
    }

    void LateUpdate()
    {
        int targetIdleOpacity = SwitcherApplicationController.isIdle ? 1 : 0;
        if (this._attractAmt != targetIdleOpacity)
        {
            this._attractAmt = Mathf.MoveTowards(this._attractAmt, targetIdleOpacity, Time.deltaTime);
        }
       

        //Test attract cycling of games
        //if (Input.GetKeyDown(KeyCode.Alpha0))
        //{
        //    AttractCycleNextGame();
        //}

        float speed = Mathf.Lerp(_normalSpeed, _quickSpeed, Mathf.InverseLerp(.25f, 1,_speedAccumulator));
        float lastFuzz = _fuzzyIdx;
        keyHeld = false;

        bool autoRight = _autoKeyCounter > 0;
        bool autoLeft = _autoKeyCounter < 0;
        _autoKeyCounter = Mathf.MoveTowards(_autoKeyCounter, 0, Time.deltaTime);


        if (!PreLaunchGameInfo.Instance.open)
        {

            float scrollAmount = CrockoInput.GetListScroll();

            if (Mathf.Abs(scrollAmount) > 0)
            {
     
                _scrollMomentumDirection = scrollAmount < 0 ? -1 : 1;
                keyHeld = true;
                _fuzzyIdx += scrollAmount * Time.deltaTime * speed;
            }
        }



        _fuzzyIdx %= GameCatalog.Instance.gameCount;

        if (_fuzzyIdx < 0)
        {
            _fuzzyIdx += GameCatalog.Instance.gameCount;
        }


        int lastSelectedIdx = _selectedTopIndex;

        if (!keyHeld)
        {          
            _fuzzyIdx = Mathf.MoveTowards(_fuzzyIdx, _selectedTopIndex, Time.deltaTime * speed);

            if (fuzzIdxSelectionOffsetted == (int)fuzzIdxSelectionOffsetted && fuzzIdxSelectionOffsetted != MenuVisualsGeneric.Instance.gameIdx)
            {
                if (Time.time > _timeOfLastSwitch + 1)
                {
                    _timeOfLastSwitch = Time.time;
                    //UpdateVideoToCurrentGame();
                }
            }
        }

        bool shoulShowAlphaHelper = keyHeld && _keyHeldTimer > .75f;
        _alphaHelperCanvasGroup.alpha = Mathf.MoveTowards(_alphaHelperCanvasGroup.alpha, 
            (shoulShowAlphaHelper ? 1 : 0), 
            (keyHeld ? Time.deltaTime * 2 : Time.deltaTime * 1f));

        if ((int)lastFuzz != (int)_fuzzyIdx)
        {
            OnRepopulated();
        }

        float fuzzIdxOffset = _fuzzyIdx % 1;
        _container.anchoredPosition = _startAp + Vector2.up * fuzzIdxOffset * _height;
       

        if (keyHeld)
        {
            _keyHeldTimer += Time.deltaTime;
            _speedAccumulator = Mathf.MoveTowards(_speedAccumulator, 1, Time.deltaTime  / 1.75f);
        }
        else
        {
            _keyHeldTimer = Mathf.Max(0, _keyHeldTimer - 2*Time.deltaTime);
            _speedAccumulator = Mathf.MoveTowards(_speedAccumulator, 0,  Time.deltaTime * 2f);
        }

        if (currentGame != null)
        {
            char firstLetter = currentGame.title[0];
            firstLetter = char.IsLetter(firstLetter) ? char.ToUpper(firstLetter) : '#';
            _alphaHelper.text = "" + firstLetter;
        }

        UpdateTextViz();
    }

    void UpdateTextViz()
    {

        float listHeightHalf = _listItems.Count / 2;

        for (int i = 0; i < _listItems.Count; i++)
        {
            float offsetSkew = _fuzzyIdx % 1;
            float rawDiffSigned = i - offsetSkew - _onScreenSelectionOffset;
            float rawDiff = Mathf.Abs(rawDiffSigned);
            float scaleFactor = Mathf.InverseLerp(listHeightHalf * .4f, 0, rawDiff); 
            _listItems[i].transform.localScale = Vector3.one * Mathf.Lerp(.5f, 1f, Mathf.Pow(scaleFactor, 2));
            _listItems[i].titleColor = Color.Lerp(_defaultItemColor, _selectedItemColor, Mathf.Pow(Mathf.InverseLerp(1.1f, 0, rawDiff), 2));


            float rotationAmount = Mathf.Clamp(-rawDiffSigned / 4.5f, -1, 1);
            _listItems[i].transform.localEulerAngles = 30 * rotationAmount * Vector3.forward;// Vector3.up * 80 * (Mathf.Pow(1 - Mathf.InverseLerp(listHeightHalf * .75f, 2, rawDiff), 2));

            float attractModePenalty = Mathf.Lerp(1, Mathf.InverseLerp(.95f ,.15f, rawDiff),_attractAmt);

            float postSelectedPenalty = Mathf.InverseLerp(1, .1f, rawDiffSigned);
            _listItems[i].alpha = postSelectedPenalty * attractModePenalty;

            //_listItems[i].darkenedAmount = Mathf.InverseLerp(0, .75f, rawDiff);


            //Lift the tabs up a little bit as they approach the selected tab, to give it some extra margin
            float finalApproach =
                rawDiffSigned < -1 ?
                Mathf.InverseLerp(-4.5f, -1,  rawDiffSigned)
                :
                Mathf.InverseLerp(-.25f, -1, rawDiffSigned);
            Vector3 upwardsBump = finalApproach * 50 * Vector3.up;
            _listItems[i].transform.localPosition =
                upwardsBump
                +
                _listItems[i].initialPosition.withX(Mathf.Pow(Mathf.InverseLerp(0, 1.1f, rawDiff), 2) * -35);
            //+ Mathf.Lerp(0, -800, rawDiff * _attractAmt));
        }
    }

    void OnRepopulated()
    {
        _things.Clear();
        int gidx = 0;
        foreach (GameData gd in GameCatalog.Instance.games)
        {
            _things.Add(new PersonalGameThing(gd, gidx));
            gidx++;
        }
        _things.Sort((a, b) => string.Compare(a.cleanTitle, b.cleanTitle ));
       // int fuzzFloored = (int)_fuzzyIdx;
        for (int i = 0; i < _listItems.Count; i++)
        {
            int effIdx = ((int) _fuzzyIdx + i + _things.Count) % _things.Count;
            _listItems[i].SetTabFlipIndex(_things[effIdx].realIdx);
            _listItems[i].gameData = _things[effIdx].data;
            _listItems[i].title = _things[effIdx].cleanTitle;

        }
    }

    [ContextMenu("Get Texts")]
    void popTexts()
    {
        this.GetComponentsInChildren(_listItems);
    }
}
