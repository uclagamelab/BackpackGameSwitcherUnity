﻿/*
 This class also seems a little heavy...

 */

using System.Collections;
using System.Collections.Generic;
using System.IO;
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

    //public ControllerSettings controllerSettings;
    #endregion ---------------------------------------------------------

    #region --- UNSERIALIZED ------------------------------------------

    [System.NonSerialized]
    public string description;  //not included in JSON (has dedicated separate file)
    [System.NonSerialized]
    public string howToPlay;    //not included in JSON (has dedicated separate file)
    [System.NonSerialized]
    public string notes;    //not included in JSON (has dedicated separate file)


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
    public Texture2D overrideInstructionsImage = null;
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
        setUpVideo(_gameFolder);

        setUpInstructionsOverlay(_gameFolder);

        this.launchSettings.SetUpWithGame(this);
    }
    #endregion ------------------------------------------------------------------------------------------


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
        string rawJson =  !File.Exists(jsonFilePath) ? null : File.ReadAllText(jsonFilePath);
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
    string launchSettingsFilePath;

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


    void setUpInstructionsOverlay(DirectoryInfo gameFolder)
    {
        GameCatalog.Instance.StartCoroutine(setUpInstructionsOverlayRoutine(gameFolder));
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

                this.overrideInstructionsImage = null;
                using (UnityWebRequest instOvlWww = UnityWebRequestTexture.GetTexture(ovlUrl))
                {
                    yield return instOvlWww.SendWebRequest();

                    this.overrideInstructionsImage = DownloadHandlerTexture.GetContent(instOvlWww);
                }
            }



        }

    }

    void setUpVideo(DirectoryInfo gameFolder)
    {

        string videoFolder = gameFolder.FullName + "/video";
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

    }
    //void setUpExe_LEGACY(DirectoryInfo gameFolder)
    //{
    //    string platform = "windows";

    //    string exeFolder = gameFolder.FullName + "/" + platform;

    //    //try to find a .lnk
    //    bool exeFolderExists = Directory.Exists(exeFolder);
    //    if (!exeFolderExists)
    //    {
    //        return;
    //    }

    //    string[] shortcutsInGameDirectory = Directory.GetFiles(exeFolder, "*.lnk");
    //    string[] batchFilesInGameDirectory = Directory.GetFiles(exeFolder, "*.bat");

    //    if (batchFilesInGameDirectory.Length == 1)
    //    {
    //        this.executable = batchFilesInGameDirectory[0];
    //    }
    //    else if (shortcutsInGameDirectory.Length == 1) //use a shortcut
    //    {
    //        //TODO : need to think of something smarter... 
    //        //optionally specify start file???
    //        this.executable = shortcutsInGameDirectory[0];

    //        //verify existence of link...




    //    }
    //    else //try to find an exe...
    //    {
    //        //string[] subdirectories = Directory.GetDirectories(gameFolder.FullName);
    //        string[] exeFolderContents = Directory.GetFiles(exeFolder, "*.exe");

    //        if (exeFolderContents.Length == 0)
    //        {
    //            Debug.Log("couldn't find an exe");
    //            this.executable = "";
    //        }
    //        else if (exeFolderContents.Length > 1)
    //        {
    //            string outString = "Multiple exes in the folder, don't know which one to use!";
    //            foreach (string s in exeFolderContents)
    //            {
    //                outString += '\n' + s;
    //            }
    //            Debug.Log(outString);
    //        }
    //        else //just 1
    //        {
    //            this.executable = exeFolderContents[0];
    //        }
    //    }



    //}

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


    public string directory
    {
        get
        {
            FileInfo fi = new FileInfo(executable);
            return fi.Directory.FullName;
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
