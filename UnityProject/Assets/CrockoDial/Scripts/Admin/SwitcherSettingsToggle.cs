using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;
using UnityEngine.Events;

public class SwitcherSettingsToggle : MonoBehaviour
{
    Toggle _tog;
    [SerializeField] public string _propertyName;

    public UnityEvent onToggleOn = new();
    public UnityEvent onToggleOff = new();

    private void Start()
    {
        _tog = this.GetComponent<Toggle>();
        refreshFromSettings();
        _tog.onValueChanged.AddListener(applyToSettings);
        _tog.onValueChanged.AddListener((togVal) => 
        {
            if (togVal) 
                onToggleOn.Invoke();
            else 
                onToggleOff.Invoke();
        });

        if (_tog.isOn) 
            onToggleOn.Invoke();
        else 
            onToggleOff.Invoke();

    }

    void refreshFromSettings()
    {
        var dat = SwitcherSettings.Data;
        Type ty = dat.GetType();
        FieldInfo fi = ty.GetField(_propertyName);
        if (fi == null)
        {
            _tog.interactable = false;
        }
        else
        {
            bool curValue = (bool) fi.GetValue(dat);
            if (_tog.isOn != curValue)
            {
                _tog.isOn = curValue;
            }
        }
    }

    void applyToSettings(bool newVal)
    {
        var dat = SwitcherSettings.Data;
        Type ty = dat.GetType();
        FieldInfo fi = ty.GetField(_propertyName);
        bool newValue = _tog.isOn;
        if (fi == null)
        {
            _tog.interactable = false;
        }
        else
        {
            fi.SetValue(dat, newValue);
            SwitcherSettings.ApplyChanges();
        }
    }

}
