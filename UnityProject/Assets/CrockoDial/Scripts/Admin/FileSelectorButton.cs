using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Crosstales.FB;
using System.IO;
using System;
using System.Reflection;

public class FileSelectorButton : MonoBehaviour
{
    public string _fieldName;
    bool detectIfExeRelative => _baseDirectory != BaseDirectoryType.JoyToKey;
    public enum BaseDirectoryType
    {
        JoyToKey,
        GamesDirectory,
        CompanionSoftwareDirectory,
        Other,
    }

    public enum DialogSelectionType
    {
        SingleFile,
        SingleFolder
    }

    public DialogSelectionType _dialogTarget = DialogSelectionType.SingleFile;
    public BaseDirectoryType _baseDirectory;
    public string windowTitle = "";
    public string startDirectory = null;
    public string extensions = "*";
    InputField _targetField;

    public bool _setFileNameInsteadOfFullPath = true;
    public System.Action<string> OnValidPathChosen = (string chosenFile) => { };
    

    void Awake()
    {
        this.GetComponent<Button>().onClick.AddListener(onClick);
        _targetField = this.transform.parent.GetComponentInChildren<InputField>();
        if (!string.IsNullOrEmpty(_fieldName))
        {
            _targetField.onEndEdit.AddListener(
                (string newVal) => {
                    this.applyToSettings(newVal, false);
                });
        }
    }

    private void OnEnable()
    {
        refreshFromSettings();
    }

    public string StartingDirectoryIfNoHistory()
    {
       return Directory.Exists(XuFileUtil.RunningAppDirectory) ? XuFileUtil.RunningAppDirectory :  Path.GetDirectoryName(Application.dataPath);
    }
    
    void onClick()
    {
        if (string.IsNullOrEmpty(startDirectory) || !Directory.Exists(startDirectory))
        {
            if (_baseDirectory == BaseDirectoryType.GamesDirectory)
            {
                startDirectory = SwitcherSettings.Data.GamesFolder;
            }
            else if (_baseDirectory == BaseDirectoryType.JoyToKey)
            {
                startDirectory = SwitcherSettings.Data.JoyToKeyFolder;
            }
            else if (_baseDirectory == BaseDirectoryType.CompanionSoftwareDirectory)
            {
                startDirectory = SwitcherSettings.Data.JoyToKeyFolder;
            }
            else if (_baseDirectory == BaseDirectoryType.Other)
            {
                startDirectory = StartingDirectoryIfNoHistory();
            }  
        }

        if (!Directory.Exists(startDirectory))
        {
            startDirectory = StartingDirectoryIfNoHistory();
        }

            string result = null;
        if (_dialogTarget == DialogSelectionType.SingleFile)
        {
            result = FileBrowser.OpenSingleFile(windowTitle, startDirectory, extensions);
        }
        else if (_dialogTarget == DialogSelectionType.SingleFolder)
        {
            result = FileBrowser.OpenSingleFolder(windowTitle, startDirectory);
        }

        if (!string.IsNullOrEmpty(result))
        {
            startDirectory = Path.GetDirectoryName(result);
            string finalVal = result;
            if (_setFileNameInsteadOfFullPath)
            {
                finalVal = Path.GetFileName(result);//exclude the path
            }

            if (detectIfExeRelative)
            {
                if (XuFileUtil.IsSubdirectory(finalVal, XuFileUtil.RunningAppDirectory))
                {
                    finalVal = XuFileUtil.ComputeRelativePath(finalVal, XuFileUtil.RunningAppDirectory);
                    finalVal = Path.Combine(".", finalVal);
                }
            }

            _targetField.text = finalVal;
            OnValidPathChosen.Invoke(finalVal);
            applyToSettings(finalVal);
        }
    }

    void refreshFromSettings()
    {
        if (string.IsNullOrEmpty(_fieldName))
        {
            return;
        }
        var dat = SwitcherSettings.Data;
        Type ty = dat.GetType();
        FieldInfo fi = ty.GetField(_fieldName);
        if (fi == null)
        {
            _targetField.interactable = false;
        }
        else
        {
            string curValue = (string)fi.GetValue(dat);
 
            if (_targetField.text != curValue)
            {
                _targetField.text = curValue;
            }
        }
    }

    void applyToSettings(string newVal, bool writeToDisk = true)
    {
        if (string.IsNullOrEmpty(_fieldName))
        {
            return;
        }
        var dat = SwitcherSettings.Data;
        Type ty = dat.GetType();
        FieldInfo fi = ty.GetField(_fieldName);
        string newValue = _targetField.text;
        if (fi == null)
        {
            _targetField.interactable = false;
        }
        else
        {
            fi.SetValue(dat, newValue);
            if (writeToDisk)// || (fi.GetValue(dat) as string) != newVal)
            {
                SwitcherSettings.ApplyChanges();
            }
        }
    }

}
