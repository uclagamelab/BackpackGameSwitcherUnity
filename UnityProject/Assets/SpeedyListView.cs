using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class SpeedyListView : MonoBehaviour
{

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

    List<PersonalGameThing> _things = new List<PersonalGameThing>();

    [SerializeField]
    List<TextMeshProUGUI> _texts;
    int idx = 3;

    [SerializeField]
    TextMeshProUGUI _alphaHelper;

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
        container = _texts[0].transform.parent.GetComponent<RectTransform>();
        startAp = container.anchoredPosition;
        height = _texts[0].GetComponent<RectTransform>().sizeDelta.y;
        middleIdxOffset = _texts.Count / 2;
    }
    void Start()
    {
        OnRepopulated();
    }
    int dir = 0;
    // Update is called once per frame

    GameData currentGame
    {
        get;
         set;

    }

    int middleIdxOffset = 1;


    float speedAccumulator = 0; 
    const float normalSpeed = 6;
    const float quickSpeed = 11;

    void switchToCurrentGame()
    {
       
        int setIdx =  Mathf.FloorToInt(fuzzIdxSelectionOffetted);
        MenuVisualsGeneric.Instance.cycleToNextGame(setIdx - MenuVisualsGeneric.Instance.gameIdx, false, false);
    }

    void Update()
    {
        float speed = Mathf.Lerp(normalSpeed, quickSpeed, Mathf.InverseLerp(.25f, 1,speedAccumulator));
        float lastFuzz = fuzzyIdx;
        bool keyHeld = false;


        
        if (Input.GetKey(KeyCode.RightArrow))
        {
            dir = 1;
            keyHeld = true;
            fuzzyIdx += Time.deltaTime * speed;
 
            //idx = (idx + 1) % GameCatalog.Instance.gameCount;
            //OnRepopulated();
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            dir = -1;
            keyHeld = true;
            fuzzyIdx -= Time.deltaTime * speed;
           
            //idx = (idx - 1 + GameCatalog.Instance.gameCount) % GameCatalog.Instance.gameCount;
            //OnRepopulated();
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

        if ((int)lastFuzz != (int)fuzzyIdx)
        {
            idx = (int)fuzzyIdx;
            OnRepopulated();
        }

        float fuzzIdxOffset = fuzzyIdx % 1;
        container.anchoredPosition = startAp + Vector2.up * fuzzIdxOffset * height;
        UpdateTextViz();

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

        char firstLetter = currentGame.title[0];
        firstLetter = char.IsLetter(firstLetter) ? char.ToUpper(firstLetter) : '#';
        _alphaHelper.text = "" + firstLetter;
    }

    void UpdateTextViz()
    {

        float listHeightHalf = _texts.Count / 2;

        for (int i = 0; i < _texts.Count; i++)
        {
            float offsetSkew = fuzzyIdx % 1;
            float rawDiff = Mathf.Abs(i - offsetSkew - middleIdxOffset);
            float scaleFactor = Mathf.InverseLerp(listHeightHalf, 0, rawDiff); 
            _texts[i].transform.localScale = Vector3.one * Mathf.Lerp(.8f, 1.45f, Mathf.Pow(scaleFactor, 2));
            _texts[i].color = Color.Lerp(Color.red, new Color(1,.5f, 1), Mathf.Pow(Mathf.InverseLerp(1.1f, 0, rawDiff), 2));

           //_texts[i].transform.localEulerAngles = Vector3.up * 80 * (Mathf.Pow(1 - Mathf.InverseLerp(listHeightHalf * .75f, 2, rawDiff), 2));

            _texts[i].transform.localPosition = _texts[i].transform.localPosition.withX((1-Mathf.Pow(1 - Mathf.InverseLerp(listHeightHalf * 1.25f, 0, rawDiff), 2)) * 75);
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
        for (int i = 0; i < _texts.Count; i++)
        {

            int effIdx = (idx + i + _things.Count) % _things.Count;

            _texts[i].text = _things[effIdx].cleanTitle;

        }
    }

    [ContextMenu("Get Texts")]
    void popTexts()
    {
        this.GetComponentsInChildren(_texts);
    }
}
