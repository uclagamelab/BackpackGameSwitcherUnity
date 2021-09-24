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
    WebsiteLaunchSettingsEditor _websiteSettingsEditor;

    [SerializeField]
    Dropdown _typeDropDown;

    [SerializeField]
    Button _launchAndRecordMouseButton;

    // Start is called before the first frame update
    void Awake()
    {
        List<Dropdown.OptionData> l = new List<Dropdown.OptionData>();
        foreach (GameLaunchType o in System.Enum.GetValues(typeof(GameLaunchType)))
        {
            l.Add(new Droption(o));
        }
        //l.Reverse();
        _typeDropDown.ClearOptions();
        _typeDropDown.AddOptions(l);
        _typeDropDown.value = 0;
        _typeDropDown.onValueChanged.AddListener(OnDifferentTypeChosen);
        _launchAndRecordMouseButton.onClick.AddListener(LaunchAndRecordClicks);
    }

    void LaunchAndRecordClicks()
    {
        ProcessRunner.instance.StartGame(GameInfoEditor.instance.currentSelectedGame);

        GameLaunchSettings launchSettings = GameInfoEditor.instance.currentSelectedGame.launchSettings;
        if (launchSettings != null)
        {

            Debug.Log("Hiho!");
            launchSettings.mouseStartupOptions = new MouseStartUpOptions();
            MouseRecorder.instance.StartRecording(launchSettings.mouseStartupOptions);
        }

    }
    private void Start()
    {
        GameInfoEditor.events.OnSelectedGameChanged += OnSelectedGameChanged;
    }
    void OnDifferentTypeChosen(int optIdx)
    {
        _unityLaunchSettingsEditor.gameObject.SetActive(currentSelectedLaunchType == GameLaunchType.Unity);
        _genericSettingsEditor.gameObject.SetActive(currentSelectedLaunchType == GameLaunchType.Generic);
        _websiteSettingsEditor.gameObject.SetActive(currentSelectedLaunchType == GameLaunchType.Website);
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
        _websiteSettingsEditor.ApplyChangesToInMemoryGameData();
    }

    private void OnEnable()
    {
        OnSelectedGameChanged();
    }

}
