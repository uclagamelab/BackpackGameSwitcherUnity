﻿/*
 This class also seems a little heavy...

 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class GameData
{
    #region SERIALIZED --------------------------------------------------
    public string title;

    public string designers;

    public string year = "";

    public string windowTitle = null;

    public string joyToKeyConfig_singlePlayer = null;

    public string exePath;
    public string exePathAbsolute => Path.Combine(this.rootFolder.FullName, this.exePath);

    public ControlInstructions instructions;

    public GameLaunchSettings launchSettings;

    [SerializeField] DisplayedControls _showInstructionsForControlScheme = DisplayedControls.auto;
    public GamePlayInfo playabilityInfo = new();

    //public ControllerSettings controllerSettings;
    #endregion ---------------------------------------------------------

    //this could be inferred by whether it's in allGames, but not in 'games', but mayber easier if just a cached bool?
    public bool isFilteredOut
    {
        get => !GameCatalog.Instance.games.Contains(this);
    }


    [System.Serializable]
    public class GamePlayInfo
    {
        public GamePlayInfo(bool allSupported = false)
        {
            if (allSupported)
            {
                foreach (var e in Enumz.AllValues<CrockoInputMode>())
                {
                    this.SetIsSupported((CrockoInputMode)e, true);
                }
            }
        }

        public CrockoInputMode this[int key]
        {
            get => _supportedControlSchemes[key];
        }

        public int Count => _supportedControlSchemes.Count;

        public int maxPlayers = 1;
        [SerializeField] List<CrockoInputMode> _supportedControlSchemes = new();
        public bool IsSupported(CrockoInputMode controlScheme)
            => _supportedControlSchemes.Contains(controlScheme);

        public void SetIsSupported(CrockoInputMode controlScheme, bool setSupported)
        {
            bool changed = false;
            if (!setSupported)
            {
                changed = true;
                _supportedControlSchemes.RemoveAll((el) => el == controlScheme);
            }
            else if (setSupported && !_supportedControlSchemes.Contains(controlScheme))
            {
                changed = true;
                _supportedControlSchemes.Add(controlScheme);
            }

            if (changed)
            {
                _supportedControlSchemes.Sort();
            }
        }

        internal bool intersects(GamePlayInfo shownGameTypes)
        {
            foreach (var gt1 in this._supportedControlSchemes)
            {
                foreach (var gt2 in shownGameTypes._supportedControlSchemes)
                {
                    var haveInvalidEnum = !Enum.IsDefined(typeof(CrockoInputMode), gt1) || !Enum.IsDefined(typeof(CrockoInputMode), gt2);
                    if (haveInvalidEnum)
                    {
                        continue;
                    }

                    if (gt1 == gt2) return true;
                }
            }
            return false;
        }

        public void Sanitize()
        {
            //shouldn't generally be a problem, but list can have invalid values in it...
            _supportedControlSchemes.RemoveAll((el) => !Enum.IsDefined(typeof(CrockoInputMode), el));
        }
    }


    #region --- UNSERIALIZED ------------------------------------------

    [System.NonSerialized]
    public string description;  //not included in JSON (has dedicated separate file)
    [System.NonSerialized]
    public string howToPlay;    //not included in JSON (has dedicated separate file)
    [System.NonSerialized]
    public string notes;    //not included in JSON (has dedicated separate file)
    public bool previewVideoHasAudio { get; private set; }

    [System.NonSerialized]
    DirectoryInfo _gameFolder = null;
    public DirectoryInfo rootFolder => _gameFolder;

    [System.NonSerialized]
    public string image;

    //[System.NonSerialized]
    //public Texture2D overrideInstructionsImage = null;

    [System.NonSerialized]
    public string videoUrl = null;

    [System.NonSerialized]
    public string executable;

    [System.NonSerialized]
    public Texture2D genericOverrideInstructionImage = null;
    [System.NonSerialized]
    public Texture previewImg;
    #endregion -----------------------------------------------

    public bool valid
    {
        get;
        private set;
    }

    #region ---CONSTRUCTOR ---------------------------------------------------------------------
    public GameData(string gameFolderPath)
    {
        valid = true;
        _gameFolder = new DirectoryInfo(gameFolderPath);//IMPORTANT that this gets set immediately

        if (_gameFolder.Exists)
        {
            descriptionFilePath = findOrCreateDoubleExtensionTextFile(".description.txt", "");
            instructionsFilePath = findOrCreateDoubleExtensionTextFile(".instructions.txt", "");
            notesFilePath = findOrCreateDoubleExtensionTextFile(".notes.txt", "[put admin and setup related notes here]");
            //launchSettingsFilePath = findOrCreateDoubleExtensionTextFile(".launch_settings.json", "");
        }
        // --- Find the JSON info file ---------

        //bool jsonInfoFound = File.Exists(jsonFilePath);
        //string[] jsonFiles = Directory.GetFiles(_gameFolder.FullName, "*.json");
        string gameDataJson = GetInfoJSON();
        string diskDescription = GetDescriptionText();
        string diskInstructions = GetInstructionText();
        string diskNotes = GetNotesText();

        try
        {
            JsonUtility.FromJsonOverwrite(gameDataJson, this);
        }
        catch (System.Exception e)
        {
            Debug.LogError("problem parsing json in " + _gameFolder.FullName + "\n" + e);
            valid = false;
            return;
        }


        this.description = diskDescription;
        this.howToPlay = diskInstructions;
        this.notes = diskNotes;

        this.previewVideoHasAudio = true;
        if (string.IsNullOrEmpty(videoUrl) || System.IO.Path.GetFileNameWithoutExtension(videoUrl).EndsWith("[NO_AUDIO]", System.StringComparison.InvariantCultureIgnoreCase))
        {
            previewVideoHasAudio = false;
        }


        if (string.IsNullOrEmpty(this.title))
        {
            this.title = _gameFolder.Name;
            flushChangesToDisk();
        }

        // --- Find the exe ------------------------
        //setUpExe_LEGACY(_gameFolder);
        if (!File.Exists(this.exePathAbsolute))
        {
            autoFindAndAssignAppropriateExe();
        }

        // --- Find the preview images ------------------------
        setUpImages(_gameFolder);

        // --- Find the instructions ------------------------

        //--- set up video ---------------
        string videoFolder = _gameFolder.FullName + "/video";
        if (Directory.Exists(videoFolder))
        {
            //try to find a .lnk
            //TODO : figure out valid video types...
            List<string> videosInDirectory = GetFilesMultipleSearchPattern(videoFolder, new string[] { "*.mp4", "*.mov", "*.ogv", "*.flv" });
            foreach (string v in videosInDirectory)
            {
                //Debug.Log(v + "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            }
            if (videosInDirectory.Count > 0)
            {
                this.videoUrl = videosInDirectory[0];
            }
        }
        this.previewVideoHasAudio = !string.IsNullOrEmpty(videoUrl) && !System.IO.Path.GetFileNameWithoutExtension(videoUrl).EndsWith("[NO_AUDIO]", System.StringComparison.InvariantCultureIgnoreCase);

        GameCatalog.Instance.StartCoroutine(setUpInstructionsOverlayRoutine(_gameFolder));
        cacheCustomControlImagePaths();

        this.launchSettings.SetUpWithGame(this);

        this.playabilityInfo.Sanitize();
    }
    #endregion ------------------------------------------------------------------------------------------

    public enum DisplayedControls { auto = 0, none = 50, arcade = 100, gamepad = 200, mouseAndKeyboard = 300 }


    public DisplayedControls showInstructionsForControlScheme
    {
        get => _showInstructionsForControlScheme;
        set => _showInstructionsForControlScheme = value;
    }

    [System.Serializable]
    public class ControlInstructions
    {
        public string joystickInstructions;

        public string[] buttonInstructions = new string[6];

        public string getButtonLabel(int buttonIdx)
        {
            return buttonInstructions[buttonIdx - 1];
        }
    }


    public T GetControlDesc<T>(bool createIfMissing = false) where T : class, new()
    {
        T ret = null;
        var path = $"{this._gameFolder}/{typeof(T).Name}.json";
        try
        {
            var json = XuFileUtil.ReadText(path);

            if (!string.IsNullOrEmpty(json))
            {
                ret = JsonUtility.FromJson<T>(json);
            }
            else
            {
                ret = new T();
                XuFileUtil.WriteText(JsonUtility.ToJson(ret, true), path);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }

        return ret;
    }

    public string GetDescriptionText()
    {
        return XuFileUtil.ReadText(this.descriptionFilePath);
    }

    public string GetInstructionText()
    {
        return XuFileUtil.ReadText(this.instructionsFilePath);
    }

    public string GetNotesText()
    {
        return XuFileUtil.ReadText(this.notesFilePath);
    }



    public string GetInfoJSON()
    {
        bool prettify = true;
        string rawJson = !File.Exists(jsonFilePath) ? null : File.ReadAllText(jsonFilePath);
        if (string.IsNullOrEmpty(rawJson))
        {
            rawJson = JsonUtility.ToJson(this, prettify);
            XuFileUtil.WriteText(rawJson, jsonFilePath);
        }

        return rawJson;
    }

    public void flushChangesToDisk()
    {
        string newJson = JsonUtility.ToJson(this, true);
        WriteJSON(newJson);
        XuFileUtil.WriteText(this.description, descriptionFilePath);
        XuFileUtil.WriteText(this.howToPlay, instructionsFilePath);
        XuFileUtil.WriteText(this.notes, notesFilePath);
        //GameLaunchSettings gls = this.launchSettings;
        //string glsLaunchSettingsJson = JsonUtility.ToJson(gls, true);
        //XuFileUtil.WriteText(glsLaunchSettingsJson, launchSettingsFilePath);
    }

    public void WriteJSON(string newJson)
    {
        XuFileUtil.WriteText(newJson, this.jsonFilePath);
    }


    //string _descriptionFilePathCached = null;
    string descriptionFilePath;

    //string _instructionsFilePathCached = null;
    string instructionsFilePath;
    string notesFilePath;

    string findOrCreateDoubleExtensionTextFile(string extension, string defaultVal)
    {
        string ret = null;
        //--- Look for a file ending in .instructions.text ----------------
        string[] filesWithInstructionExtension = Directory.GetFiles(_gameFolder.FullName, "*" + extension);
        if (filesWithInstructionExtension.Length > 0)
        {
            ret = filesWithInstructionExtension[0];
        }
        else //--- Create it, if it doesn't exist ---------------
        {
            string newPath = Path.Combine(_gameFolder.FullName, _gameFolder.Name + extension);
            XuFileUtil.WriteText(defaultVal, newPath);
            ret = newPath;
        }
        return ret;
    }



    string jsonFilePath
    {
        get
        {
            if (string.IsNullOrEmpty(_gameFolder?.FullName))
            {
                Debug.LogError("must set game folder before you can get jsonFilePath");
                return null;
            }
            return System.IO.Path.Combine(_gameFolder.FullName, "SwitcherGameInfo.json");
        }
    }


    void autoFindAndAssignAppropriateExe()
    {
        string chosenPath = null;
        XuFileUtil.ProcessAllFilesRecursive(this._gameFolder.FullName, (path) =>
        {
            if (chosenPath == null)
            {
                FileInfo fi = new FileInfo(path);
                bool seeminglyAppropriateExe = fi.Extension == ".exe"
                &&
                fi.Name != "UnityCrashHandler64.exe"
                &&
                !fi.Name.StartsWith("."); //mac drives generate a bunch of duplicate files that start with '.'

                if (seeminglyAppropriateExe)
                {
                    chosenPath = XuFileUtil.ComputeRelativePath(fi.FullName, _gameFolder.FullName);
                }
            }
        });

        if (chosenPath != null)
        {
            this.exePath = chosenPath;
        }
    }

    static readonly string[] overrideInstructionImgExtensions = new string[] { "*.png", "*.jpg" };
    IEnumerator setUpInstructionsOverlayRoutine(DirectoryInfo gameFolder)
    {
        string instructionsFolder = gameFolder.FullName + "/instructions";
        if (Directory.Exists(instructionsFolder))
        {

            List<string> imgsInDirectory = GetFilesMultipleSearchPattern(instructionsFolder, overrideInstructionImgExtensions);
            foreach (string v in imgsInDirectory)
            {
                //Debug.Log(v + "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            }
            if (imgsInDirectory.Count > 0)
            {
                string ovlUrl = imgsInDirectory[0];

                this.genericOverrideInstructionImage = null;
                yield return GameCatalog.Instance.StartCoroutine(loadImage(ovlUrl, (tex) => this.genericOverrideInstructionImage = tex));
            }
        }
    }

    static IEnumerator loadImage(string ovlUrl, System.Action<Texture2D> onComplete)
    {
        using (UnityWebRequest instOvlWww = UnityWebRequestTexture.GetTexture(ovlUrl))
        {
            yield return instOvlWww.SendWebRequest();

            var tex = DownloadHandlerTexture.GetContent(instOvlWww);

            onComplete.Invoke(tex);
        }
    }

    #region Custom Control Images
    CustomInstructionImage[] _customInstructionImages = null;
    void cacheCustomControlImagePaths()
    {
        List<CustomInstructionImage> tmp = new();
        //Enumz<CrockoInputMode>

        string instructionsFolder = _gameFolder.FullName + "/instructions";

        if (Directory.Exists(instructionsFolder)) foreach (var file in Directory.GetFiles(instructionsFolder))
        {
            if (Path.GetExtension(file) == ".png")
            {
                string fname = Path.GetFileNameWithoutExtension(file);
                foreach (DisplayedControls type in Enumz.AllValues<DisplayedControls>())
                {
                    if (type == DisplayedControls.none || type == DisplayedControls.auto)
                        continue;

                    if (fname.ToLower().EndsWith(type.ToString().ToLower()))
                    {
                        tmp.Add(new CustomInstructionImage(type, file));
                    }
                }
            }
        }
        _customInstructionImages = tmp.ToArray();
    }

    public bool hasCustomInstructionImage(GameData.DisplayedControls controlType)
    {
        if (_customInstructionImages != null)
            foreach (var el in _customInstructionImages)
            {
                if (el.controlType == controlType) return true;
            }
        return false;
    }

    public static event System.Action<GameData> OnGameDataImageLoad = (g) => {};

    public Texture2D GetCustomInstructionImage(GameData.DisplayedControls controlType)
    {
        foreach (var el in _customInstructionImages)
        {
            if (el.controlType == controlType)
            {
                return el.getTexture(this);
            }
        }
        return null;
    }

    class CustomInstructionImage
    {
        public DisplayedControls controlType;
        string path;
        bool haveLoadedTexture = false;
        Texture2D cachedTexture;
        public Texture2D getTexture(GameData srcGame)
        {
            if (!haveLoadedTexture)
            {
                haveLoadedTexture = true;
                GameCatalog.Instance.StartCoroutine(loadImage(path, (tex) =>
                {
                    cachedTexture = tex;
                    OnGameDataImageLoad.Invoke(srcGame);
                    //Debug.LogError("TODO: Likely need to manually refresh the prelaunch canvas, otherwise custom instruction won't be visible first time around");
                }
                ));

            }
            return cachedTexture;
        }
        public CustomInstructionImage(DisplayedControls controlType, string path)
        {
            this.controlType = controlType;
            this.path = path;
        }
    }


    #endregion


    void setUpImages(DirectoryInfo gameFolder)
    {
        List<string> previewImageFolderContents = GetFilesMultipleSearchPattern(Path.Combine(gameFolder.FullName, "image"), new string[] { "*.png", "*.jpg", "*.gif" });

        if (previewImageFolderContents.Count > 0)
        {
            string previewImgPath = previewImageFolderContents[0];

            ProcessRunner.instance.StartCoroutine(getImage(previewImgPath));
        }
    }

    public string joyToKeyConfig
    {
        get
        {
            if (string.IsNullOrEmpty(joyToKeyConfig_singlePlayer))
            {
                return "default.cfg";
            }
            else
            {
                return joyToKeyConfig_singlePlayer;
            }
        }

        set
        {
            joyToKeyConfig_singlePlayer = value;
        }
    }

    public void Audit(System.Text.StringBuilder auditMsgStringBuilder)
    {
        GameData dat = this;
        if (string.IsNullOrEmpty(dat.exePath))
        {
            auditMsgStringBuilder.AppendLine(dat.title + " has empty exe path");
        }
        else if (!System.IO.File.Exists(dat.exePathAbsolute))
        {
            auditMsgStringBuilder.AppendLine(dat.title + ", no file found at specified exe path");
        }

        if (string.IsNullOrEmpty(dat.joyToKeyConfig))
        {
            auditMsgStringBuilder.AppendLine(dat.title + " doesn't specify joy to key config");
        }
        else if (!System.IO.File.Exists(Path.Combine(SwitcherSettings.Data.JoyToKeyFolder, dat.joyToKeyConfig)))
        {
            auditMsgStringBuilder.AppendLine(dat.title + ", joytokey config: ;" + dat.joyToKeyConfig + "' not found");
        }

        if (string.IsNullOrEmpty(dat.windowTitle))
        {
            auditMsgStringBuilder.AppendLine(dat.title + " has no window title set");
        }

        this.launchSettings.Audit(auditMsgStringBuilder);
    }


    enum GameType
    {
        unity, processing, other
    }


    public GameData()
    {
        title = "";
        executable = "";
        designers = "";
        description = "";
    }

    List<string> GetFilesMultipleSearchPattern(string path, string[] searchPatterns)
    {
        List<string> allResults = new List<string>();
        if (Directory.Exists(path))
        {
            foreach (string searchPattern in searchPatterns)
            {
                string[] results = Directory.GetFiles(path, searchPattern);
                foreach (string individResult in results)
                {
                    allResults.Add(individResult);
                }
            }
        }

        return allResults;
    }

    IEnumerator getImage(string texturePath)
    {

        string textureURI = texturePath;


        using (var textureReq = UnityWebRequestTexture.GetTexture(textureURI))
        {
            yield return textureReq.SendWebRequest();

            this.previewImg = DownloadHandlerTexture.GetContent(textureReq);
        }
    }

}
