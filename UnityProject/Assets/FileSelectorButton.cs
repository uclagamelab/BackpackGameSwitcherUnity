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
        GamesDirectory
    }

    public BaseDirectoryType _baseDirectory;
    public string windowTitle = "";
    public string startDirectory = null;
    public string extensions = "*";
    InputField _targetField;
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
            startDirectory = _baseDirectory == BaseDirectoryType.GamesDirectory ? SwitcherSettings.GamesFolder : SwitcherSettings.JoyToKeyFolder;
        }

        string result = FileBrowser.OpenSingleFile(windowTitle, startDirectory, extensions);
        if (!string.IsNullOrEmpty(result))
        {
            startDirectory = Path.GetDirectoryName(result);
            _targetField.text = Path.GetFileName(result);//exclude the path
        }
    }
}
