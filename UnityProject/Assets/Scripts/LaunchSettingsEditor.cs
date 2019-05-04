using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LaunchSettingsEditor : MonoBehaviour
{
    
    public class Droption : Dropdown.OptionData
    {
        public GameLaunchType gameLaunchType
        {
            get;
            private set;
        }
        public Droption(GameLaunchType type) : base(type.ToString())
        {
            gameLaunchType = type;
        }
    }

    [SerializeField]
    UnityLaunchSettingsEditor _unityLaunchSettingsEditor;

    [SerializeField]
    GenericLaunchSettingsEditor _genericSettingsEditor;

    [SerializeField]
    Dropdown _typeDropDown;

    // Start is called before the first frame update
    void Awake()
    {
        List<Dropdown.OptionData> l = new List<Dropdown.OptionData>();
        foreach (GameLaunchType o in System.Enum.GetValues(typeof(GameLaunchType)))
        {
            l.Add(new Droption(o));
            
        }
        l.Reverse();
        _typeDropDown.ClearOptions();
        _typeDropDown.AddOptions(l);
        _typeDropDown.onValueChanged.AddListener(OnDifferentTypeChosen);
    }

    private void Start()
    {
        GameInfoEditor.events.OnSelectedGameChanged += OnSelectedGameChanged;
    }
    void OnDifferentTypeChosen(int optIdx)
    {
        _unityLaunchSettingsEditor.gameObject.SetActive(currentSelectedLaunchType == GameLaunchType.Unity);
        _genericSettingsEditor.gameObject.SetActive(currentSelectedLaunchType == GameLaunchType.Generic);
    }
    void OnSelectedGameChanged()
    {
        for (int i = 0; GameInfoEditor.instance.currentSelectedGame != null && i < _typeDropDown.options.Count; i++)
        {
            if ((_typeDropDown.options[i] as Droption).gameLaunchType == GameInfoEditor.instance.currentSelectedGame.launchSettings.type)
            {
                _typeDropDown.value = i;
                break;
            }
        }
    }

    GameLaunchType currentSelectedLaunchType
    {
        get
        {
            Droption droption = ((Droption)_typeDropDown.options[_typeDropDown.value]);
        if (droption != null)
            {
                return droption.gameLaunchType;
            }
            return GameLaunchType.Unity;
                
        }
    }

    public void ApplyChangesToGameDataInMemory()
    {
        GameInfoEditor.instance.currentSelectedGame.launchSettings.type = currentSelectedLaunchType;

        //Probably, they should all just subscribe to an event
        _unityLaunchSettingsEditor.ApplyChangesToInMemoryGameData();
        _genericSettingsEditor.ApplyChangesToInMemoryGameData();
    }

    private void OnEnable()
    {
        OnSelectedGameChanged();
    }

}
