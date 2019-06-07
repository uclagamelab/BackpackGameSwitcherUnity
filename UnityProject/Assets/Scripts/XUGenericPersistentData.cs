using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XUGenericPeristentDataSingleton<T> : XUGenericPersistentData<T> where T : new()
{

    internal static SingleTown<XUGenericPeristentDataSingleton<T>> _instanceHelper = new SingleTown<XUGenericPeristentDataSingleton<T>>(SingletownStrategy.CreateGameObjectAndAddComponentIfNonExisting);
    protected static XUGenericPeristentDataSingleton<T> instance
    {
        get
        {
            //#if UNITY_EDITOR
            //return Application.isPlaying ? _instanceHelper.instance : null;
            //#else
            return _instanceHelper.instance;
            //#endif
        }
    }

    public static T Data
    {
        get
        {
            return instance.currentSaveData;
        }
    }

    public static string SaveDataPath
    {
        get
        {
            return instance.saveDataPath;
        }
    }

    internal static void LoadDataFromDisk()
    {
        instance.loadDataFromDisk();
    }

    //TODO(ALEX) : change to a "mark dirty" mode where changes will be automatically flushed when stabilized.
    public static void ApplyChanges()
    {
        instance.writeCurrentStateToDisk();
    }

    public static void MarkDirty()
    {
        instance.markDirty();
    }

    public static void ClearSaveData()
    {
        instance.clearSaveData();
    }

}



public class XUGenericPersistentData<T> : MonoBehaviour where T : new()
{
    #region FIELDS AND PROPERTIES

    [Header("--- EDITOR ONLY SETTINGS ---")]
#if UNITY_EDITOR
    [SerializeField]
    bool _useInspectorSetValuesEditorOnly = false;

    [SerializeField]
    bool _suppresSaves = false;
#else
    const bool _useInspectorSetValuesEditorOnly = false;
    const bool _suppresSaves = false;
#endif

    [Header("--- SAVE DATA ---")]
    [SerializeField]
    protected T _currentSaveData;

    public T currentSaveData
    {
        get
        {
            if (_currentSaveData == null)
            {
                loadDataFromDisk();
            }

            return _currentSaveData;
        }

        set
        {
            _currentSaveData = value;
        }
    }

    protected virtual string fileName => this.GetType().ToString() + ".json";

    public virtual string saveDataPath 
        =>
#if UNITY_EDITOR
        System.IO.Path.Combine(Application.dataPath, fileName);
#else
        //adjacent to exe
        System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.dataPath), fileName);
        #endif

    static Events _events = new Events();
    public static Events events => _events;

    public T data
    {
        get
        {
            if (currentSaveData == null)
            {
                loadDataFromDisk();
            }
            return currentSaveData;
        }
    }
#endregion


    private void Awake()
    {
#if !UNITY_EDITOR
        _currentSaveData = new T();
#endif

        if (!_useInspectorSetValuesEditorOnly)
        {
            loadDataFromDisk();
        }

    }


    protected internal void loadDataFromDisk()
    {
        if (_currentSaveData == null)
        {
            _currentSaveData = new T();
        }

        string loadedSaveDataJson = XuFileSystemUtil.ReadText(saveDataPath);
        if (loadedSaveDataJson != null)
        {
            //This is a little naive longerterm, would be better to use SimpleJSON instead and get a more generic object
            JsonUtility.FromJsonOverwrite(loadedSaveDataJson, _currentSaveData);
        }
    }

    bool _isDirty = false;
    float _dirtyTimer = 0;

    public void markDirty()
    {
        _isDirty = true;
        _dirtyTimer = 1;
    }

    private void Update()
    {
        if (_isDirty && _dirtyTimer > 0)
        {
            _dirtyTimer -= Time.deltaTime;
            if (_dirtyTimer <= 0)
            {
                _isDirty = false;
                Debug.Log("DIRTY SAVE TIMER EXPIRED, Writing to disk");
                writeCurrentStateToDisk();
            }
        }
    }

    protected internal void writeCurrentStateToDisk()
    {
        if (!_suppresSaves || !Application.isPlaying) //allow saves outside play mode
        {
            XuFileSystemUtil.WriteText(JsonUtility.ToJson(this._currentSaveData, true), saveDataPath);
        }
        events.OnSaveDataUpdated.Invoke();
    }

    public void clearSaveData()
    {
        XuFileSystemUtil.DeleteFile(saveDataPath);
        currentSaveData = new T();
        events.OnSaveDataUpdated.Invoke();
    }


    public class Events
    {
        public System.Action OnSaveDataUpdated = () => { };
    }




}

#if UNITY_EDITOR
public class XUGenericPersistentDataEditor<T, V> : UnityEditor.Editor where T : XUGenericPersistentData<V> where V : new()
{
    public override void OnInspectorGUI()
    {
        T targetScript = (T)target;
        DrawDefaultInspector();
        //GUILayout.Label("Singletown?? : " + (targetScript == TLGameSave._instanceHelper.instance));
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save To Disk"))
        {
            targetScript.writeCurrentStateToDisk();
        }

        if (GUILayout.Button("Refresh From Disk"))
        {
            targetScript.loadDataFromDisk();
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Restore Defaults"))
        {
            targetScript.clearSaveData();
        }

        if (GUILayout.Button("Open In Explorer"))
        {
            XuFileSystemUtil.SelectFileInExplorer(targetScript.saveDataPath);
        }
        GUILayout.EndHorizontal();
    }
}
#endif
