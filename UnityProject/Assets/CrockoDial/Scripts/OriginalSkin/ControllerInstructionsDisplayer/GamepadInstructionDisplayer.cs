using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamepadInstructionDisplayer : MonoBehaviour, IInstructionsDisplayer
{
    public Transform container => this.transform.GetChild(0);
    void Awake()
    {
        //build dict
        foreach(var cName in ALL_CONTROL_NAMES)
        {
            ControlDisplay cd = new();
            cd.name = cName;
            cd.defaultImg = this.container.Find($"default/{cName}")?.gameObject;
            cd.highlightImg = this.container.Find($"highlight/{cName}")?.gameObject;
            cd.indicatorLineImg = this.container.Find($"line/{cName}")?.gameObject;
            cd.text = this.container.Find($"text/{cName}")?.GetComponent<TMPro.TextMeshProUGUI>();
            //dbgList.Add( cd );
            _lookups[cName] = cd;
        }
    }


    [ContextMenu("FILL IN TEXT")]
    public void SetUp()
    {
        this.Awake();
        foreach (var keyPair in _lookups)
        {
            if (keyPair.Value.text != null)
            {
                keyPair.Value.text.text = $"{keyPair.Value.name}\n{keyPair.Value.name}\n{keyPair.Value.name}";
            }
        }

    }

    public void Set(string control, string actionDesc)
    {
        if (_lookups.TryGetValue(control, out var c))
        {
            bool shouldBeOn = !string.IsNullOrEmpty(actionDesc);
            c.text?.gameObject?.SetActive(shouldBeOn);
            if (c.text != null)
            {
                c.text.text = actionDesc;
            }

            c.indicatorLineImg?.SetActive(shouldBeOn);
            c.highlightImg?.SetActive(shouldBeOn);
            c.defaultImg?.SetActive(!shouldBeOn);
        }
    }

    public void clear()
    {
        foreach (var keyPair in _lookups)
        {
            keyPair.Value.defaultImg?.SetActive( true);
            keyPair.Value.highlightImg?.SetActive( false);
            keyPair.Value.indicatorLineImg?.SetActive( false);
            if (keyPair.Value.text != null)
            {
                keyPair.Value.text.gameObject?.SetActive( false);
                keyPair.Value.text.text = "";
            }
            
        }
    }

    //public List<ControlDisplay> dbgList = new();

    [System.Serializable]
    public class ControlDisplay
    {
        public string name;
        public TMPro.TextMeshProUGUI text;
        public GameObject defaultImg;
        public GameObject highlightImg;
        public GameObject indicatorLineImg;
    }

    public Dictionary<string, ControlDisplay> _lookups = new();


    public static readonly string[] ALL_CONTROL_NAMES =
    {
            "d-pad",
            "d-pad-left",
            "d-pad-right",
            "d-pad-down",
            "d-pad-up",
            "stick_l",
            "stick_r",
            "start",
            "select",
            "a",
            "b",
            "x",
            "y",
            "home",
            "shoulder_l",
            "shoulder_r",
            "trigger_l",
            "trigger_r",
    };

    [System.Serializable]
    public class GamepadDefinition
    {
        public Define[] controlDefinitions =
        {
            new("d-pad", ""),
            new("stick_l", ""),
            new("stick_r", ""),
            new("start", ""),
            new("select", ""),
            new("a", ""),
            new("b", ""),
            new("x", ""),
            new("y", ""),
            new("home", ""),
            new("shoulder_l", ""),
            new("shoulder_r", ""),
            new("trigger_l", ""),
            new("trigger_r", ""),
        };

        [System.Serializable]
        public class Define
        {
            public string control;
            public string action;
            public Define(string part, string action)
            {
                this.control = part;
                this.action = action;
            }
        }
    }

    int IInstructionsDisplayer.IsHandlerFor(GameData gameData)
    {
        if (gameData.displayedControls == GameData.DisplayedControls.gamepad)
        {
            return 10;
        }
        return -1;
    }

    bool IInstructionsDisplayer.ShowGame(GameData game)
    {
        this.clear();
        var controls = game.GetControlDesc<GamepadDefinition>(true);
        bool ok = controls != null;
        try
        {
            foreach(var desc in controls.controlDefinitions)
            {
                this.Set(desc.control, desc.action);
            }
        }
        catch (System.Exception ex)
        {
            ok = false;
            Debug.LogException(ex);
        }
        return ok;
    }

#if UNITY_EDITOR

    [ContextMenu("Fix Names")]
    void removeGimpPoundSigns()
    {
        UnityEditor.Undo.RegisterFullObjectHierarchyUndo(this.gameObject, "Remove '#1' from names");
        foreach (var t in this.GetComponentsInChildren<Transform>(true))
        {
            t.name = t.name.Replace(" #1", "");
        }
    }

    //this can be used to convert a PSD imported with layers / sprite renderers
    //into a new hierarchy with ui.images and rect transforms suitable for a canvas
    [ContextMenu("convertSpriteRendererHierarchyToCanvas")]
    void convertSpriteRendererHierarchyToCanvas()
    {
        UnityEditor.Undo.RegisterFullObjectHierarchyUndo(this.gameObject, "Fix");
        for (int i = 0; i < this.transform.childCount; i++)
        {
            var g = this.transform.GetChild(i).gameObject;
            var sr = g.GetComponent<SpriteRenderer>();
            var img = g.AddComponent<UnityEngine.UI.Image>();
            img.sprite = sr.sprite;
            DestroyImmediate(sr);
            //t.gameObject.AddComponent<RectTransform>();
        }
    }
#endif
}
