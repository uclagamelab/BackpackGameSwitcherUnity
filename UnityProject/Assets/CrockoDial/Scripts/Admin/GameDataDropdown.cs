using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;
using TMPro;

public class GameDataDropdown : MonoBehaviour, IGameInfoEditorListener
{
    [SerializeField] string _enumTypeName;
    TMPro.TMP_Dropdown _dropdown;
    TMPro.TMP_Dropdown dropdown
    {
        get
        {
            if (_dropdown == null)
            {
                _dropdown = this.GetComponent<TMPro.TMP_Dropdown>();
            }
            return _dropdown;
        }
    }
    [SerializeField] public string _propertyName;

    public enum FieldType { field, property};
    public FieldType fieldType = FieldType.field;

    int[] _enumVals;
    int[] enumVals
    {
        get
        {
            if (_enumVals == null)
            {
                _enumVals = Enumz.AllValues(_enumTypeName);
                if (_enumVals == null )
                {
                    _enumVals= new int[0];
                }
            }
            return _enumVals;
        }
    }

    [ContextMenu("testTypeName")]
    void testTypeName()
    {
        Debug.Log(typeof(GameData.DisplayedControls).FullName);
        var e = Enumz.AllValues(_enumTypeName);
        Debug.Log($"{(e != null ? e.Length : -1)}");
    }

    private void Start()
    {
        dropdown.options = new();
        foreach (var val in enumVals)
        {
            var op = new TMP_Dropdown.OptionData();
            op.text = Enumz.NameFromIntValue(val, System.Type.GetType(_enumTypeName));
            dropdown.options.Add(op);
        }
        dropdown.RefreshShownValue();
        dropdown.onValueChanged.AddListener(applyToData);
        refreshFromData(GameInfoEditor.instance?.currentSelectedGame);
    }

    void refreshFromData(GameData dat)
    {
        if (dat == null) return;

        Type ty = dat.GetType();
        MemberInfo fi = fieldType == FieldType.field ? 
            ty.GetField(_propertyName)
            :
            ty.GetProperty(_propertyName);
       

        if (fi == null)
        {
            dropdown.interactable = false;
        }
        else
        {
            int curValue = (int) GetValue(fi, dat);
            int idx = 0;
            for (int i = 0; i < enumVals.Length; i++)
            {
                if (enumVals[i] == curValue)
                {
                    idx = i; break;
                }
            }
            if (dropdown.value != idx)
            {
                dropdown.SetValueWithoutNotify(idx);
            }
        }
    }

    public int testI = 0;
    [ContextMenu("ttt")]
    public void setTest()
    {
        dropdown.SetValueWithoutNotify(testI);
    }

    public object GetValue(MemberInfo mi, object inst)
    {
        var fi = mi as FieldInfo;
        var pi = mi as PropertyInfo;
        if (fi != null) return fi.GetValue(inst);
        else if (pi != null) return pi.GetValue(inst);
        else return null;
    }

    public void SetValue(MemberInfo mi, object inst, int newValue)
    {
        var fi = mi as FieldInfo;
        var pi = mi as PropertyInfo;
        if (fi != null) fi.SetValue(inst, newValue);
        else if (pi != null) pi.SetValue(inst, newValue);
    }

    int getCurrentEnumValue()
    {
        return Enumz.AllValues(this._enumTypeName)[dropdown.value];
    }

    void applyToData(int currentDropDown)
    {


        var dat = currentGame;
        Type ty = dat.GetType();
        MemberInfo fi = fieldType == FieldType.field ?
            ty.GetField(_propertyName)
            :
            ty.GetProperty(_propertyName);

        if (fi == null)
        {
            dropdown.interactable = false;
        }
        else
        {
            SetValue(fi, dat, getCurrentEnumValue());
            SwitcherSettings.ApplyChanges();
        }
    }

    GameData currentGame;
    void IGameInfoEditorListener.OnGameChange(GameData newGame)
    {
        currentGame = newGame;
        this.refreshFromData(currentGame);
    }
}
