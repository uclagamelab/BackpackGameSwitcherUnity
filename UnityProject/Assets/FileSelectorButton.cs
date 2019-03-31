using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Crosstales.FB;
using System.IO;

public class FileSelectorButton : MonoBehaviour
{
    public enum BaseDirectoryType
    {
        JoyToKey,
        GamesDirectory,
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

    // Start is called before the first frame update
    void Awake()
    {
        this.GetComponent<Button>().onClick.AddListener(onClick);
        _targetField = this.transform.parent.GetComponentInChildren<InputField>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void onClick()
    {
        if (string.IsNullOrEmpty(startDirectory) || !Directory.Exists(startDirectory))
        {
            if (_baseDirectory == BaseDirectoryType.GamesDirectory)
            {
                startDirectory = SwitcherSettings.GamesFolder;
            }
            else if (_baseDirectory == BaseDirectoryType.JoyToKey)
            {
                startDirectory = SwitcherSettings.JoyToKeyFolder;
            }
            else if (_baseDirectory == BaseDirectoryType.Other)
            {
                startDirectory = Path.GetDirectoryName(Application.dataPath);
            }
            
                
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
            if (_setFileNameInsteadOfFullPath)
            {
                _targetField.text = Path.GetFileName(result);//exclude the path
            }
            else
            {
                _targetField.text = result;//exclude the path
            }

            
        }
    }
}
