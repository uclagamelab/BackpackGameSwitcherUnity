using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;


public class SpeedyListView : MonoBehaviour
{
   public static SpeedyListView instance
    {
        get;
        private set;
    }
    struct ListItemGameData
    {
        public string titleSansNewlines => data.title.Replace('\n', ' ');
        public string title => data.title;
        public int realIdx;
        public GameData data;
        public ListItemGameData(GameData dat, int idx)
        {
            realIdx = idx;
            this.data = dat;
        }
    }
    [Range(0, 1)]
    public float _attractAmt = 0;
    [SerializeField]
    Color _defaultItemColor = Color.black;
    [SerializeField]
    Color _selectedItemColor = Color.magenta;



    List<ListItemGameData> _listGameDatas = new List<ListItemGameData>();

    [SerializeField]
    List<SpeedyListItem> _listItems;

    public int stopIndex
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
    public float fuzzyIdx => _fuzzyIdx;
    float fuzzIdxSelectionOffsetted => (_fuzzyIdx + _onScreenSelectionOffset) % _listGameDatas.Count;

    float _height;
    // Start is called before the first frame update
    float _timeOfLastSwitch = 0;

    int _scrollMomentumDirection = 0;

    float _autoKeyCounter = 0;

    [SerializeField]
    int _onScreenSelectionOffset = 8;
    
    float _speedAccumulator = 0;
    public const float _normalSpeed = 5;
    public const float _quickSpeed = 11;
    #region PUBLIC THINGS FOR SOUND
    public float speed => _speedAccumulator ;
    public float speedNormalizedIsh =>
        Mathf.Abs(_speedAccumulator) <= _normalSpeed
        ?
        Mathf.InverseLerp(0, _normalSpeed, Mathf.Abs(_speedAccumulator))
        :
        1 + Mathf.InverseLerp(_normalSpeed, _quickSpeed, Mathf.Abs(_speedAccumulator));

    public event System.Action OnStoppedAtItem = () => { };
    public event System.Action OnPassedItem = () => { };
    #endregion

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
            if (_listGameDatas.Count > 0)
            {
                int effIdx = (stopIndex + _onScreenSelectionOffset) % _listGameDatas.Count;
                if (effIdx < _listGameDatas.Count)
                {
                    return _listGameDatas[effIdx].data;
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

    float _stabilizedIdx;
    float _stabilizedStartTime = 0;
    static StringBuilder sb = new StringBuilder();
    void LateUpdate()
    {

        sb.Clear();
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

        float speed = Mathf.Lerp(_normalSpeed, _quickSpeed, Mathf.InverseLerp(.25f, 1, _speedAccumulator));
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


        if (GameCatalog.Instance.gameCount > 0)
        {
            _fuzzyIdx %= GameCatalog.Instance.gameCount;
        }

        if (_fuzzyIdx < 0)
        {
            _fuzzyIdx += GameCatalog.Instance.gameCount;
        }


        int lastSelectedIdx = stopIndex;

        if (!keyHeld)
        {          
            _fuzzyIdx = Mathf.MoveTowards(_fuzzyIdx, stopIndex, Time.deltaTime * speed);

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

            OnPassedItem.Invoke();
            OnRepopulated();
        }

        if (_stabilizedStartTime <= 0 && stopIndex != _stabilizedIdx && (Mathf.Approximately(_fuzzyIdx, stopIndex)) && Mathf.Abs(_speedAccumulator) <= _quickSpeed)
        {
            _stabilizedStartTime = .25f;
            _stabilizedIdx = stopIndex;
            OnStoppedAtItem.Invoke();
        }
        if (_stabilizedStartTime > 0)
        {
            _stabilizedStartTime = Mathf.Max(0, _stabilizedStartTime - Time.deltaTime);
        }


        float fuzzIdxOffset = _fuzzyIdx % 1;
        _container.anchoredPosition = _startAp + Vector2.up * fuzzIdxOffset * _height;

        float prevSpeed = _speedAccumulator;
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

        //if (Mathf.Abs(_speedAccumulator) == 0 && Mathf.Abs(prevSpeed) != 0)
        //{
        //    OnItemPassed.Invoke();
        //}

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
        _listGameDatas.Clear();
        int gidx = 0;
        foreach (GameData gd in GameCatalog.Instance.games)
        {
            _listGameDatas.Add(new ListItemGameData(gd, gidx));
            gidx++;
        }
        _listGameDatas.Sort((a, b) => string.Compare(a.title, b.title));
       // int fuzzFloored = (int)_fuzzyIdx;
        for (int i = 0; i < _listItems.Count; i++)
        {
            int effIdx = ((int) _fuzzyIdx + i + _listGameDatas.Count) % Mathf.Max(1,_listGameDatas.Count);
            if (effIdx < _listGameDatas.Count)
            {
                _listItems[i].SetTabFlipIndex(_listGameDatas[effIdx].realIdx);
                _listItems[i].gameData = _listGameDatas[effIdx].data;
                _listItems[i].title = _listGameDatas[effIdx].titleSansNewlines;
            }

        }

        if (currentGame != null)
        {
            char firstLetter = currentGame.title[0];
            firstLetter = char.IsLetter(firstLetter) ? char.ToUpper(firstLetter) : '#';
            sb.Clear();
            sb.Append(firstLetter);
            _alphaHelper.text = sb.ToString();
        }
    }

    [ContextMenu("Get Texts")]
    void popTexts()
    {
        this.GetComponentsInChildren(_listItems);
    }
}
