using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class SpeedyListView : MonoBehaviour
{
    [SerializeField]
    int _centerIdxOffset = 0;
    struct PersonalGameThing
    {
        public string cleanTitle;
        public GameData data;
        public PersonalGameThing(GameData dat)
        {
            this.data = dat;
            this.cleanTitle = dat.title.Replace('\n', ' ');
        }
    }

    [SerializeField]
    Color _defaultItemColor = Color.black;
    [SerializeField]
    Color _selectedItemColor = Color.magenta;

    List<PersonalGameThing> _things = new List<PersonalGameThing>();

    [SerializeField]
    List<SpeedyListItem> _listItems;
    int idx = 3;

    [Space(10)]
    [SerializeField]
    TextMeshProUGUI _alphaHelper;

    [SerializeField]
    CanvasGroup _alphaHelperCanvasGroup;

    RectTransform container;
    Vector2 startAp;

    float fuzzyIdx = 0;
    float fuzzIdxSelectionOffetted => (fuzzyIdx + middleIdxOffset) % _things.Count;

    float height;
    // Start is called before the first frame update
    float timeOfLastSwitch = 0;
    private void Awake()
    {
        GameCatalog.Events.OnRepopulated += OnRepopulated;
        container = _listItems[0].transform.parent.GetComponent<RectTransform>();
        startAp = container.anchoredPosition;
        height = _listItems[0].GetComponent<RectTransform>().sizeDelta.y;
        middleIdxOffset = (_listItems.Count / 2) + _centerIdxOffset;
        //_alphaHelperCanvasGroup = _alphaHelper.GetComponent<CanvasGroup>();
        SwitcherApplicationController.OnAttractCycleNextGame += AttractCycleNextGame;
    }
    void Start()
    {
        OnRepopulated();
    }
    int dir = 0;
    // Update is called once per frame

    float autoKeyCounter = 0;

    void AttractCycleNextGame()
    {
        autoKeyCounter = .1f;
    }

    GameData currentGame
    {
        get;
         set;

    }

    int middleIdxOffset = 1;


    float speedAccumulator = 0; 
    const float normalSpeed = 5;
    const float quickSpeed = 11;

    void switchToCurrentGame()
    {
       
        int setIdx =  Mathf.FloorToInt(fuzzIdxSelectionOffetted);
        MenuVisualsGeneric.Instance.cycleToNextGame(setIdx - MenuVisualsGeneric.Instance.gameIdx, false, false);
    }

    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            AttractCycleNextGame();
        }
        float speed = Mathf.Lerp(normalSpeed, quickSpeed, Mathf.InverseLerp(.25f, 1,speedAccumulator));
        float lastFuzz = fuzzyIdx;
        bool keyHeld = false;

        bool autoRight = autoKeyCounter > 0;
        bool autoLeft = autoKeyCounter < 0;
        autoKeyCounter = Mathf.MoveTowards(autoKeyCounter, 0, Time.deltaTime);


        if (!PreLaunchGameInfo.Instance.open)
        {
        if (Input.GetKey(KeyCode.RightArrow) || autoRight)
        {
            dir = 1;
            keyHeld = true;
            fuzzyIdx += Time.deltaTime * speed;
 
            //idx = (idx + 1) % GameCatalog.Instance.gameCount;
            //OnRepopulated();
        }
        else if (Input.GetKey(KeyCode.LeftArrow) || autoLeft)
        {
            dir = -1;
            keyHeld = true;
            fuzzyIdx -= Time.deltaTime * speed;
           
            //idx = (idx - 1 + GameCatalog.Instance.gameCount) % GameCatalog.Instance.gameCount;
            //OnRepopulated();
        }
        }

        fuzzyIdx %= GameCatalog.Instance.gameCount;

        if (fuzzyIdx < 0)
        {
            fuzzyIdx += GameCatalog.Instance.gameCount;
        }


        if (!keyHeld)
        {
            int closestInt = Mathf.CeilToInt(fuzzyIdx);
            if (dir < 0)
            {
                closestInt = Mathf.FloorToInt(fuzzyIdx);
            }
            
            fuzzyIdx = Mathf.MoveTowards(fuzzyIdx, closestInt, Time.deltaTime * speed);

            if (fuzzIdxSelectionOffetted == (int)fuzzIdxSelectionOffetted && fuzzIdxSelectionOffetted != MenuVisualsGeneric.Instance.gameIdx)
            {
                if (Time.time > timeOfLastSwitch + 1)
                {
                    timeOfLastSwitch = Time.time;
                    switchToCurrentGame();
                }
            }
        }

        _alphaHelperCanvasGroup.alpha = Mathf.MoveTowards(_alphaHelperCanvasGroup.alpha, 
            (keyHeld ? 1 : 0), 
            (keyHeld ? Time.deltaTime * 2 : Time.deltaTime * 1f));

        if ((int)lastFuzz != (int)fuzzyIdx)
        {
            idx = (int)fuzzyIdx;
            OnRepopulated();
        }

        float fuzzIdxOffset = fuzzyIdx % 1;
        container.anchoredPosition = startAp + Vector2.up * fuzzIdxOffset * height;
       

        int gameIdx = Mathf.CeilToInt((fuzzyIdx + middleIdxOffset) % _things.Count);
        if (gameIdx >= 0 && gameIdx < _things.Count)
        {
            currentGame = _things[gameIdx].data;
        }

        if (keyHeld)
        {
            speedAccumulator = Mathf.MoveTowards(speedAccumulator, 1, Time.deltaTime  / 1.75f);
        }
        else
        {
            speedAccumulator = Mathf.MoveTowards(speedAccumulator, 0,  Time.deltaTime * 2f);
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
            float offsetSkew = fuzzyIdx % 1;
            float rawDiffSigned = i - offsetSkew - middleIdxOffset;
            float rawDiff = Mathf.Abs(rawDiffSigned);
            float scaleFactor = Mathf.InverseLerp(listHeightHalf * .4f, 0, rawDiff); 
            _listItems[i].transform.localScale = Vector3.one * Mathf.Lerp(.5f, 1f, Mathf.Pow(scaleFactor, 2));
            _listItems[i].color = Color.Lerp(_defaultItemColor, _selectedItemColor, Mathf.Pow(Mathf.InverseLerp(1.1f, 0, rawDiff), 2));

            _listItems[i].transform.localEulerAngles = 30 * Mathf.InverseLerp(0, 1.1f, -rawDiffSigned) * Vector3.forward;// Vector3.up * 80 * (Mathf.Pow(1 - Mathf.InverseLerp(listHeightHalf * .75f, 2, rawDiff), 2));

            float postSelectedPenalty = Mathf.InverseLerp(1, .1f, rawDiffSigned);
            _listItems[i].color = _listItems[i].color.withAlpha(postSelectedPenalty);
            //_texts[i].transform.localPosition = _texts[i].transform.localPosition.withX((1-Mathf.Pow(1 - Mathf.InverseLerp(listHeightHalf * 1.25f, 0, rawDiff), 2)) * 35);
        }
    }

    void OnRepopulated()
    {
        _things.Clear();
        foreach (GameData gd in GameCatalog.Instance.games)
        {
            _things.Add(new PersonalGameThing(gd));
        }
        //_things.Sort((a, b) => string.Compare(a.cleanTitle, b.cleanTitle ));

        int selectedIdx = 5;
        for (int i = 0; i < _listItems.Count; i++)
        {
            int effIdx = (idx + i + _things.Count) % _things.Count;

            _listItems[i].title = _things[effIdx].cleanTitle;
            _listItems[i].gameData = _things[effIdx].data;

        }
    }

    [ContextMenu("Get Texts")]
    void popTexts()
    {
        this.GetComponentsInChildren(_listItems);
    }
}
