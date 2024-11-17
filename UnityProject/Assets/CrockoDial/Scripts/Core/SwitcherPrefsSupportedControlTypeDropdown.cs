using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class SwitcherPrefsSupportedControlTypeDropdown : MonoBehaviour, IGameInfoEditorListener
{
    [SerializeField] GameObject template;
    List<Toggle> _toggles = new List<Toggle>();
    Toggle _allToggle;
    Toggle _nonToggle;
    [SerializeField] Button _allButton;
    [SerializeField] Button _noneButton;

    public bool isForFilterByControlSetting = false;

    void Start()
    {
        var enumz = Enumz.AllValues<CrockoInputMode>();

        if (_allButton != null)
        {
            _allButton.onClick.AddListener(() => setAll(true));
        }
        if (_noneButton != null)
        {
            _noneButton.onClick.AddListener(() => setAll(false));
        }

        //int ALL = -2;
        //int NONE = -1;

        for (int i = 0; i < enumz.Length; i++)
        {
            GameObject copy = GameObject.Instantiate(template);
            copy.transform.SetParent(template.transform.parent, false);
            var toggle = copy.GetComponentInChildren<Toggle>(true);
            var text = copy.GetComponentInChildren<Text>();

            _toggles.Add(toggle);
            CrockoInputMode enumVal = (CrockoInputMode) enumz[i];
            text.text = SplitCamelCase(enumVal.ToString());
            toggle.onValueChanged.AddListener(MakeOnToggleChangeCallback(toggle, enumVal));
        }

        template.gameObject.SetActive(false);
        
        if (isForFilterByControlSetting)
        {
            this.Refresh(serializedControlFilter);
        }
        else
        {
            OnGameChange(GameInfoEditor.instance?.currentSelectedGame);
        }
    }

    public void setAll(bool on)
    {
        foreach (var toggle in _toggles)
        {
            toggle.isOn = on;
        }
    }

    GameData.GamePlayInfo serializedControlFilter => this.isForFilterByControlSetting ? 
        null //SwitcherSettings.Data._shownGameTypes 
        : 
        GameInfoEditor.instance?.currentSelectedGame?.playabilityInfo;

    UnityAction<bool> MakeOnToggleChangeCallback (Toggle toggle, CrockoInputMode enumSafe) 
    {
        return (bool togVal) =>
        {
            var g = serializedControlFilter;
            if (g != null)
            {
                serializedControlFilter.SetIsSupported(enumSafe, toggle.isOn);
            }

            GameCatalog.Instance.UpdateFilters();
        };
    }

    public void OnGameChange(GameData newGame)
    {
        if (newGame != null)
        {
            Refresh(newGame.playabilityInfo);
        }
    }
    public void Refresh(GameData.GamePlayInfo src)
    {
        if (_toggles.CountNullRobust() == 0) return;


        int i = -1;
        foreach (CrockoInputMode enumVal in Enumz.AllValues<CrockoInputMode>())
        {
            i++;
            _toggles[i].isOn = src.IsSupported(enumVal);

        }
    }

    public static string SplitCamelCase(string input)
    {
        var ret = System.Text.RegularExpressions.Regex.Replace(input, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
        ret = char.ToUpperInvariant(ret[0]) + ret.Substring(1);
        return ret;
    }
}
