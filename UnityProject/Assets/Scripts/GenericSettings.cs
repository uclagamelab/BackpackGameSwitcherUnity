using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class GenericSettings : MonoBehaviour
{
    static List<GenericSettings> allGenericSettings = new List<GenericSettings>(128);
    public string prefKey;
    public float defaultValue;

    public Slider sliderSource;
    public InputField inputFieldSource;

    [SerializeField]
    AudioMixerGroup mixerGroupForVolumeSetting;

    public static Alextensions.NoArgNoRetFunction OnSettingsUpdated = () => { };

    public static void SaveAllGenericSettings()
    {
        allGenericSettings.RemoveAll((gs) => { return gs == null; });
        foreach (GenericSettings gs in allGenericSettings)
        {
            gs.SaveGUIValToPreferences(false);   
        }

        OnSettingsUpdated.Invoke();


    }

    public static bool TryGetValue(string key, out float val, float defaultVal)
    {
        if (!PlayerPrefs.HasKey(key))
        {
            val = defaultVal;
            return false;
        }

        val = PlayerPrefs.GetFloat(key);

        return true;
    }

    public static void ResetAllGenericSettings()
    {
        allGenericSettings.RemoveAll((gs) => { return gs == null; });
        foreach (GenericSettings gs in allGenericSettings)
        {
            gs.applyValToGUI(gs.defaultValue);
        }

        SaveAllGenericSettings();
    }

    void Start()
    {
        allGenericSettings.Add(this);

        if (!PlayerPrefs.HasKey(prefKey))
        {
            PlayerPrefs.SetFloat(prefKey, defaultValue);
            applyValToGUI(defaultValue);
        }
        else
        {
            float val = PlayerPrefs.GetFloat(prefKey);
            applyValToGUI(val);
        }

        if (sliderSource != null)
        {
            sliderSource.onValueChanged.AddListener(valueUpdated);
        }

        if (inputFieldSource != null)
        {
            inputFieldSource.onValueChanged.AddListener(valueUpdated);
        }
    }

    void valueUpdated(string val)
    {
        float parsedVal;
        if (float.TryParse(val, out parsedVal))
        {
            valueUpdated(parsedVal);
        }
    }

    void valueUpdated(float val)
    {
        if (mixerGroupForVolumeSetting != null)
        {
            float decibelLev = -100;
            if (val > .01f)
            {
                decibelLev = Mathf.Log(val) * 20;
            }
            mixerGroupForVolumeSetting.audioMixer.SetFloat(prefKey, decibelLev);
        }
    }

    void SaveGUIValToPreferences(bool sendEvent = true)
    {
        float val = getVal();

        if (!float.IsNaN(val))
        {
            PlayerPrefs.SetFloat(prefKey, val);
        }

        if (sendEvent)
        {
            OnSettingsUpdated.Invoke();
        }
    }

    float getVal()
    {
        if (sliderSource != null)
        {
            return sliderSource.value;
        }

        if (inputFieldSource != null)
        {
            float outVal = 0;
            bool success = float.TryParse(inputFieldSource.text, out outVal);
            if (success)
            {
                return outVal;
            }
        }

        return float.NaN;
    }

    void applyValToGUI(float val)
    {
        applyValToSlider(val);
        applyValInputField(val);
    }

    void applyValToSlider(float val)
    {
    if (sliderSource != null)
    {
        sliderSource.value = val;

    }
}

     void applyValInputField(float val)
    {
        if (inputFieldSource != null)
        {
            inputFieldSource.text = val.ToString();// "#####.##");
        }
    }


    // Update is called once per frame
    void Update () {
		
	}
}
