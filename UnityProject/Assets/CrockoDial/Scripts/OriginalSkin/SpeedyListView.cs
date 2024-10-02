/*
 
This class displays the collection of games in srolling list of tabs.

It was called 'speedy' in that it is much quicker to navigate compared
to the first sliding video game selection made for the VNA version of 
this software.
 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


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

    public int lastMovedDirection
    {
        get;
        private set;
    }

    List<ListItemGameData> _listGameDatas = new List<ListItemGameData>();

    [SerializeField]
    List<SpeedyListItem> _listItems;
    [SerializeField]
    VerticalLayoutGroup _listLayoutGroup;

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
        lastMovedDirection = 1;
        GameCatalog.Events.OnRepopulated += OnRepopulated;
        _container = _listItems[0].transform.parent.GetComponent<RectTransform>();
        _startAp = _container.anchoredPosition;
        _height = _listItems[0].GetComponent<RectTransform>().sizeDelta.y;

        //_alphaHelperCanvasGroup = _alphaHelper.GetComponent<CanvasGroup>();
        MenuVisualsGeneric.OnAttractCycleNextGame += AttractCycleNextGame;

        //MenuVisualsGeneric.OnStateChange += (newState, prevState) => 
        PreLaunchGameInfo.OnPrelaunchCanvasOpenChange += (newState) =>
        {
            if (newState == PreLaunchGameInfo.OpenState.closed)
            {
                _currentGameOverride = null;
            }
        };

    }
    IEnumerator Start()
    {
        int nItems = 10;
        var template = this._listItems[0];
        _listLayoutGroup.enabled = true;
        for (int i = 1; i < nItems; i++)
        {
            var nuItem = GameObject.Instantiate(template.gameObject, template.transform.parent).GetComponent<SpeedyListItem>();
            _listItems.Add(nuItem);
        }
        yield return null;
        //_listLayoutGroup.CalculateLayoutInputVertical();
        foreach (var li in _listItems)
        {
            li.Reinit();
        }
        yield return null;
        _listLayoutGroup.enabled = false;
        OnRepopulated();
    }
    
    void AttractCycleNextGame()
    {
        _autoKeyCounter = -.15f;
        _currentGameOverride = null;
    }

    GameData _currentGameOverride = null;
    public GameData currentGame
    {
        get
        {
            if (_currentGameOverride != null)
            {
                return _currentGameOverride;
            }
            else if (_listGameDatas.Count > 0)
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
   

    float _keyHeldTimer = 0;

    public bool keyHeld
    {
        get;
        private set;
    }

    float _stabilizedIdx;
    float _stabilizedStartTime = 0;
    static StringBuilder sb = new StringBuilder();

    bool smallNumberOfGames => GameCatalog.Instance.gameCount < _listItems.Count - 3;
    bool _firedStopEvent = false;
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

        _autoKeyCounter = Mathf.MoveTowards(_autoKeyCounter, 0, Time.deltaTime);


        if (!PreLaunchGameInfo.Instance.open)
        {

            float scrollAmount = Mathf.Clamp(CrockoInput.GetListScroll() + _autoKeyCounter,-1,1);

            if (Mathf.Abs(scrollAmount) > 0)
            {

                _scrollMomentumDirection = scrollAmount < 0 ? -1 : 1;
                lastMovedDirection = _scrollMomentumDirection;
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
            float fuzzPrev = _fuzzyIdx;
            _fuzzyIdx = Mathf.MoveTowards(_fuzzyIdx, stopIndex, Time.deltaTime * speed);

            float plinkDistance = .05f;

            if (!_firedStopEvent && fuzzPrev != _fuzzyIdx && Mathf.Abs(_fuzzyIdx - stopIndex) < plinkDistance)
            {
                _firedStopEvent = true;
                OnStoppedAtItem.Invoke();
            }
        }
        else
        {
            _firedStopEvent = speed == 0;
        }

        bool shoulShowAlphaHelper = keyHeld && _keyHeldTimer > .75f && !smallNumberOfGames;
        _alphaHelperCanvasGroup.alpha = Mathf.MoveTowards(_alphaHelperCanvasGroup.alpha, 
            (shoulShowAlphaHelper ? 1 : 0), 
            (keyHeld ? Time.deltaTime * 2 : Time.deltaTime * 1f));

        if (DirectionalRound(lastFuzz, _scrollMomentumDirection) != DirectionalRound(_fuzzyIdx, _scrollMomentumDirection))
        {
            if (!_firedStopEvent) OnPassedItem.Invoke();
        }

        if ((int)lastFuzz != (int)_fuzzyIdx)
        {

           
            OnRepopulated();
        }

        if (_stabilizedStartTime <= 0 && stopIndex != _stabilizedIdx && (Mathf.Approximately(_fuzzyIdx, stopIndex)) && Mathf.Abs(_speedAccumulator) <= _quickSpeed)
        {
            _stabilizedStartTime = .25f;
            _stabilizedIdx = stopIndex;
            //OnStoppedAtItem.Invoke();
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

    static int DirectionalRound(float n, float direction)
    {
        return direction <= 0 ? (int)n : Mathf.CeilToInt(n);
    }

    void UpdateTextViz()
    {

        float listHeightHalf = _listItems.Count / 2;

        for (int i = 0; i < _listItems.Count; i++)
        {
            bool isMouseHovered = mouseHoveredItem == _listItems[i];
            _listItems[i].hoveredAnimAmount = Mathf.MoveTowards(_listItems[i].hoveredAnimAmount, isMouseHovered ? 1 : 0, Time.deltaTime * 6);

            float offsetSkew = _fuzzyIdx % 1;
            float rawDiffSigned = i - offsetSkew - _onScreenSelectionOffset;
            float rawDiff = Mathf.Abs(rawDiffSigned);
            float scaleFactor = Mathf.InverseLerp(listHeightHalf * .4f, 0, rawDiff); 
            _listItems[i].transform.localScale = Vector3.one * Mathf.Lerp(.5f, 1f, Mathf.Pow(scaleFactor, 2));

            var colorHilightAmt = Mathf.Pow(Mathf.InverseLerp(1.1f, 0, rawDiff), 2);
            if (isMouseHovered)
            {
                colorHilightAmt = 1;
            }

            _listItems[i].titleColor = Color.Lerp(_defaultItemColor, _selectedItemColor, colorHilightAmt);


            float rotationAmount = Mathf.Clamp(-rawDiffSigned / 4.5f, -1, 1);
            _listItems[i].transform.localEulerAngles = 30 * rotationAmount * Vector3.forward;// Vector3.up * 80 * (Mathf.Pow(1 - Mathf.InverseLerp(listHeightHalf * .75f, 2, rawDiff), 2));

            //If too few games, only show 1 list item at a time (looks stupid to have 3 games repeating over the list)
            //Also, only show the current selected game list item while in a attract mode (better view of video)
            float tooFewGamesPenalty =  smallNumberOfGames ? 1 : 0;
            float fadeOutNonSelectedGamesCoeff = Mathf.Lerp(1, Mathf.InverseLerp(.95f ,.15f, rawDiff),(_attractAmt + tooFewGamesPenalty));

            float postSelectedPenalty = Mathf.InverseLerp(1, .1f, rawDiffSigned);
            _listItems[i].alpha = postSelectedPenalty * fadeOutNonSelectedGamesCoeff;

            //Lift the tabs up a little bit as they approach the selected tab, to give it some extra margin

            float slideOff = Mathf.Max(rawDiff - 1, 0) / Mathf.Max(1, _listItems.Count - 1);
            //slideOff = 1 - slideOff;
            slideOff *= slideOff;
            //slideOff = 1 - slideOff;

            float finalApproach =
                rawDiffSigned < -1 ?
                Mathf.InverseLerp(-4.5f, -1,  rawDiffSigned)
                :
                Mathf.InverseLerp(-.25f, -1, rawDiffSigned);
            Vector3 upwardsBump = finalApproach * 50 * Vector3.up;

            Vector3 hoveredBump = _listItems[i].hoveredAnimAmount * 25 * (_listItems[i].transform.rotation * Vector3.right);

            _listItems[i].transform.localPosition =
                upwardsBump
                +
                hoveredBump
                +
                _listItems[i].initialPosition.withX(Mathf.Pow(Mathf.InverseLerp(0, 1.1f, rawDiff), 2) * -35)
                //+ Vector3.left * slideOff * 450
                ;
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

    internal void setActiveItem(SpeedyListItem speedyListItem)
    {
        _currentGameOverride = speedyListItem.gameData;
        //Worked for setting instaneously, though ugly
        //int i = -1;

        //foreach(var g in _listGameDatas)
        //{
        //    i++;
        //    if (g.data == speedyListItem.gameData)
        //    {
        //        this._fuzzyIdx = i - _onScreenSelectionOffset;
        //        this._speedAccumulator = 0;
        //    }
        //}
    }

    SpeedyListItem mouseHoveredItem = null;
    public void NotifyItemHovered(SpeedyListItem speedyListItem, bool hovered)
    {
        if (hovered)
        {
            mouseHoveredItem = speedyListItem;
        }
        else if (mouseHoveredItem == speedyListItem)
        {
            mouseHoveredItem = null;
        }
    }
}
