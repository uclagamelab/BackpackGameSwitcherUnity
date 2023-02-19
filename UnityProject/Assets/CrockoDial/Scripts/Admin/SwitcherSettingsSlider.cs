using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;

public class SwitcherSettingsSlider : MonoBehaviour
{
    Slider _slider;
    [SerializeField] public string _propertyName;
    //public float minValue;
    //public float maxValue;
    public float _preSaveScale;
    float _dirtyTimer = -1;
    private void Start()
    {
        _slider = this.GetComponent<Slider>();
        refreshFromSettings();
        _slider.onValueChanged.AddListener(applyToSettings);
    }

    void refreshFromSettings()
    {
        var dat = SwitcherSettings.Data;
        Type ty = dat.GetType();
        FieldInfo fi = ty.GetField(_propertyName);
        if (fi == null)
        {
            _slider.interactable = false;
        }
        else
        {
            float curValue = (float) fi.GetValue(dat);
            if (_slider.value != curValue)
            {
                _slider.value = curValue;
            }
        }
    }

    private void Update()
    {
        if (_dirtyTimer >= 0)
        {
            _dirtyTimer -= Time.deltaTime;
            if (_dirtyTimer < 0)
            {
                _dirtyTimer = -1;
                writeValue(_slider.value);
            }
        }
    }

    private void OnDisable()
    {
        if (_dirtyTimer >= 0)
        {
            _dirtyTimer = -1;
            writeValue(_slider.value);
        }
    }

    void applyToSettings(float newValue)
    {
        _dirtyTimer = 1;
        writeValue(newValue, false);
    }

    void writeValue(float valToWrite, bool writeToDiskImmediately = true)
    {
        var dat = SwitcherSettings.Data;
        Type ty = dat.GetType();
        FieldInfo fi = ty.GetField(_propertyName);
        if (fi == null)
        {
            _slider.interactable = false;
        }
        else
        {
            fi.SetValue(dat, valToWrite);
            if (writeToDiskImmediately)
            {
                SwitcherSettings.ApplyChanges();
            }
        }
    }

}
