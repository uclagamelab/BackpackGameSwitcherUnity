using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;
using TMPro;

public class SwitcherSettingsDropdown : MonoBehaviour
{
    [SerializeField] string _enumTypeName;
    TMPro.TMP_Dropdown _dropdown;
    [SerializeField] public string _propertyName;
    int[] _enumVals;
    private void Start()
    {
        _dropdown = this.GetComponent<TMPro.TMP_Dropdown>();
        _enumVals = Enumz.AllValues(_enumTypeName);
        _dropdown.options = new();
        foreach (var val in _enumVals)
        {
            var op = new TMP_Dropdown.OptionData();
            op.text = Enumz.NameFromIntValue(val, System.Type.GetType(_enumTypeName));
            _dropdown.options.Add(op);
        }
        _dropdown.RefreshShownValue();
        _dropdown.onValueChanged.AddListener(applyToSettings);
        refreshFromSettings();
    }

    void refreshFromSettings()
    {
        var dat = SwitcherSettings.Data;
        Type ty = dat.GetType();
        FieldInfo fi = ty.GetField(_propertyName);
        if (fi == null)
        {
            _dropdown.interactable = false;
        }
        else
        {
            int curValue = (int) fi.GetValue(dat);
            int idx = 0;
            for (int i = 0; i < _enumVals.Length; i++)
            {
                if (_enumVals[i] == curValue)
                {
                    idx = i; break;
                }
            }
            if (_dropdown.value != idx)
            {
                _dropdown.SetValueWithoutNotify(idx);
            }
        }
    }

    void applyToSettings(int dontCare)
    {
        var dat = SwitcherSettings.Data;
        Type ty = dat.GetType();
        FieldInfo fi = ty.GetField(_propertyName);
        int newValue = _enumVals.GetOrDefault(_dropdown.value, _enumVals[0]);
        if (fi == null)
        {
            _dropdown.interactable = false;
        }
        else
        {
            fi.SetValue(dat, newValue);
            SwitcherSettings.ApplyChanges();
        }
    }

}
