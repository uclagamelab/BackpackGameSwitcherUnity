/*
 
Kinds of entity 

single key: key text, action desc. key width
key range key text 1+2, action desc (fixed key width)
mouse (left click, right click, stretch: scroll wheel, scroll click)
Arrow Keys, WASD : (whether / arrows or wasd) action desciption

how to encode? list of generic json objects?
 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class KeyboardControlsDesc
{
    public List<Entry> leftColumn = new List<Entry>()
    { 
        new Entry(new string[]{"mouse"}, new string[]{"move", "left click action"}),
        new Entry(new string[]{"arrow_keys"}, new string[]{"arrow key action"}),
    };
    
    public List<Entry> rightColumn = new List<Entry>()
    {
        new Entry(new string[]{"Space"}, new string[]{"space action"}),
        new Entry(new string[]{"E"}, new string[]{"E key action"}),
    };

    [System.Serializable]
    public class Entry 
    {
        public string[] keys;
        public string key => keys == null || keys.Length < 1 ? null : keys[0];
        public string[] actions = { };
        public string action => keys == null || actions.Length < 1 ? null : actions[0];

        public Entry(string[] keys = null, string[] actions = null)
        {
            this.keys = keys != null ? keys: new string[0];
            this.actions = actions != null ? actions : new string[0];
        }
    }
}
public class PCControlsInstructionDisplayer : MonoBehaviour, IInstructionsDisplayer
{
    public GameObject singleKeyPrefab;
    public GameObject doubleKeyPrefab;
    public GameObject mousePrefab;
    public GameObject arrowKeysPrefab;
    public GameObject wasdPrefab;

    public Transform _leftColumn, _rightColumn;

    Dictionary<GameObject, List<GameObject>> _prefabPool = new();

    void Awake()
    {

    }

    int IInstructionsDisplayer.IsHandlerFor(GameData gameData)
    {
        return gameData.displayedControls == GameData.DisplayedControls.keyboard ? 100 : -1;
    }

    bool IInstructionsDisplayer.ShowGame(GameData game)
    {
        this.Clear();
        game.getKeyboardControlsDes(true);

        KeyboardControlsDesc desc = new();

        foreach(var entry in desc.leftColumn)
        {
            this.addFromEntry(entry, 0);
        }

        foreach (var entry in desc.rightColumn)
        {
            this.addFromEntry(entry, 1);
        }
        return true;
    }

    void addFromEntry(KeyboardControlsDesc.Entry e, int column)
    {
        var prefab = this.getPrefabFromKey(e);
        if (prefab != null)
        {
            var displayInst = getPooledCopy(prefab);
            displayInst.transform.SetParent(column == 0 ? _leftColumn : _rightColumn);
            displayInst.SetActive(true);
            DisplayOnObj(prefab, displayInst, e);
        }
    }



    GameObject getPooledCopy(GameObject prefab) 
    {
        var pool = _prefabPool.GetOrCreate(prefab);
        GameObject displayInst = null;
        foreach (var inst in pool)
        {
            if (!inst.gameObject.activeSelf)
            {
                displayInst = inst;
                break;
            }
        }

        if (displayInst == null)
        {
            displayInst = GameObject.Instantiate(prefab, prefab.transform.parent);
            pool.Add(displayInst);
            displayInst.gameObject.SetActive(false);
        }
        return displayInst;
    }

    public void DisplayOnObj(GameObject prefab, GameObject instance, KeyboardControlsDesc.Entry e)
    {
        if (prefab == singleKeyPrefab)
        {
            float defaultW = 120;
            var actionStr = e.action;
            var keyStr = e.key;
            find(instance.transform, "action_text_0").GetComponent<TMPro.TextMeshProUGUI>().text = actionStr;
            var keyText = find(instance.transform, "key_text_0").GetComponent<TMPro.TextMeshProUGUI>();
            keyText.text = keyStr;
            keyText.ForceMeshUpdate();
            var keyTextRenSize = keyText.GetRenderedValues(false);

            var keyRt = find(instance.transform, "key_0").transform as RectTransform;
            var sz = keyRt.sizeDelta;
            sz.x = keyTextRenSize.x + 60;
            if (keyStr.ToLower() == "space")
            {
                sz.x *= 2f;
            }
            keyRt.sizeDelta = sz;

            var border = keyRt.transform.GetChild(0);

            border.eulerAngles = Vector3.forward * ((coinFlip() ? 180 : 0) + Random.Range(-2.5f, 2.5f));
            border.localScale = new Vector3(coinFlip() ? 1 : -1, coinFlip() ? 1 : -1, 1);
        }
        else if (prefab == mousePrefab)
        {
            find(instance.transform, "action_text_0").GetComponent<TMPro.TextMeshProUGUI>().text = e.actions.GetOrDefault(0, "");

            var clickLAction = e.actions.GetOrDefault(1, "");
            find(instance.transform, "click_1_line").SetActive(!string.IsNullOrEmpty(clickLAction));
            find(instance.transform, "action_text_1").GetComponent<TMPro.TextMeshProUGUI>().text = clickLAction;
        }
        else if (prefab == wasdPrefab || prefab == arrowKeysPrefab)
        {
            find(instance.transform, "action_text_0").GetComponent<TMPro.TextMeshProUGUI>().text = e.actions.GetOrDefault(0, "");
        }
    }

    bool coinFlip() => Random.Range(0f, 1f) > .5f;

    static List<Transform> _scratch = new();
    GameObject find(Transform root, string name)
    {
        _scratch.Clear();
        root.GetComponentsInChildren<Transform>(true, _scratch);
        foreach(var t in _scratch)
        {
            if (t.name == name) return t.gameObject;
        }
        _scratch.Clear();
        return null;
    }

    public GameObject getPrefabFromKey(KeyboardControlsDesc.Entry e)
    {
        if (e.key == "mouse")
        {
            return mousePrefab;
        }
        else if (e.key == "arrow_keys")
        {
            return arrowKeysPrefab;
        }
        else if (e.key == "wasd")
        {
            return wasdPrefab;
        }
        else if (e.keys.Length == 3) //TODO match reg expresssion? "keyrange(k1,sep,k2)
        {
            return doubleKeyPrefab;
        }
        else if (e.keys.Length == 1)
        {
            return singleKeyPrefab;
        }
        return null;
    }

    void OnDisable()
    {
        Clear();
    }

    void Clear()
    {
        foreach(var l in _prefabPool.Values)
        {
            foreach(var o in l)
            {
                o.SetActive(false);
            }    
        }
    }

    [Space(25)]
    public KeyboardControlsDesc.Entry dbgEntry = new();
    public bool dbgrightCol = false;

#if UNITY_EDITOR
    [CustomEditor(typeof(PCControlsInstructionDisplayer))]
    public class Ed : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var script = target as PCControlsInstructionDisplayer;


            if(Application.isPlaying)
            {
                if (GUILayout.Button("clear"))
                {
                    script.Clear();
                }

                if (GUILayout.Button("add thing"))
                {
                    script.addFromEntry(script.dbgEntry, script.dbgrightCol ? 1 : 0);
                }
            }
        }
    }
#endif
}
