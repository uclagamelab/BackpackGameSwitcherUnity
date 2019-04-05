/*
 This class also seems a little heavy...

 */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class GameData
{
    #region SERIALIZED --------------------------------------------------
    public string title;

    public string designers;
  
    public string windowTitle = null;

    public string joyToKeyConfig_singlePlayer = null;
    
    public string exePath;

    public string description;

    public GameLaunchSettings launchSettings;// = new GameLaunchSettings();
    #endregion ---------------------------------------------------------

    #region --- UNSERIALIZED ------------------------------------------

    [System.NonSerialized]
    FileInfo _gameFolder = null;
    public FileInfo rootFolder => _gameFolder;

    [System.NonSerialized]
    public string image;

    [System.NonSerialized]
    public string videoUrl = null;

    [System.NonSerialized]
    public string executable;

    [System.NonSerialized]
    public Texture2D instructionsOverlay = null;
    [System.NonSerialized]
    public Texture previewImg;
    #endregion -----------------------------------------------



    string[] controlLabels; // convention is '0' is joystick, 1-6 are buttons
    public string joystickLabel
    {
        get { return controlLabels[0]; }
    }

    public string GetJSON(bool prettify = true)
    {
        string rawJson =  !File.Exists(jsonFilePath) ? null : File.ReadAllText(jsonFilePath);
        if (string.IsNullOrEmpty(rawJson))
        {
            rawJson = JsonUtility.ToJson(this, prettify);
            XuFileSystemUtil.WriteStringToFile(rawJson, jsonFilePath);
        }
        //if (prettify)
        //{
        //    rawJson = JSONPrettifier.Prettify(rawJson);
        //}
        return rawJson;
    }

    public void flushChangesToJson()
    {
        string newJson = JsonUtility.ToJson(this, true);
        WriteJSON(newJson);
        /*JSONObject jsonObject = new JSONObject(GetJSON(false));

        jsonObject.SetField("title", this.title);
        jsonObject.SetField("designers", this.author);
        jsonObject.SetField("description",this.description);
        jsonObject.SetField("window title", this.windowTitle);
        jsonObject.SetField("joytokey cfg", this.joyToKeyConfigFile);
        jsonObject.SetField("exe path", this.exePath);
        WriteJSON(jsonObject.ToString(true));
        */

        /*
         
        this.title = jsonObject["title"].str;
        if (!string.IsNullOrEmpty(this.title))
        {
            this.title = title.Replace("\\\"", "\"");//for games with quotes, this json library doesn't seem to properly unescape strings
        }

        this.author = jsonObject["designers"].str;
        if (jsonObject.HasField("command arguments"))
        {
                this.commandLineArguments = jsonObject["command arguments"].str;
        }
        
        this.description = jsonObject["description"].str;
        if (jsonObject.HasField("joytokey cfg"))
        {
            this.joyToKeyConfigFile = jsonObject["joytokey cfg"].str;
        }

        if (jsonObject.HasField("window title"))
        {
            windowTitle = jsonObject["window title"].ToString();
        }

        if (jsonObject.HasField("controls"))
        {

               for (int i = 0; i <= 6; i++)
                {
                    string controlName = i == 0 ? "joystick" : "button_" + i;
                    if (jsonObject["controls"].HasField(controlName))
                    {
                        this.controlLabels[i] = jsonObject["controls"][controlName].str;
                    }
                }
                // Debug.Log(jsonObject["controls"]["button_1"]);
        }
         
         
         */

    }

    public void WriteJSON(string newJson)
    {
        XuFileSystemUtil.WriteStringToFile(newJson, this.jsonFilePath);
    }

    public string getButtonLabel(int buttonIdx)
    {
        return controlLabels[buttonIdx];
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

    public GameData(string gameFolderPath)
    {
        _gameFolder = new FileInfo(gameFolderPath);//IMPORTANT that this gets set immediately

        controlLabels = new string[7];
        for (int i = 0; i < controlLabels.Length; i++)
        {
            controlLabels[i] = null;
        }

        // --- Find the JSON info file ---------

        //bool jsonInfoFound = File.Exists(jsonFilePath);
        //string[] jsonFiles = Directory.GetFiles(_gameFolder.FullName, "*.json");
        string gameDataJson = GetJSON();

        try
        {
            JsonUtility.FromJsonOverwrite(gameDataJson, this);
        }
        catch (System.Exception e)
        {
            Debug.LogError("problem parsing json in " + _gameFolder.FullName);
            return;
        }
            

        if (string.IsNullOrEmpty(this.title))
        {
            this.title = _gameFolder.Name;
            flushChangesToJson();
        }

        // --- Find the exe ------------------------
        setUpExe(_gameFolder);

        // --- Find the preview images ------------------------
        setUpImages(_gameFolder);

        // --- Find the instructions ------------------------

        //--- set up video ---------------
        setUpVideo(_gameFolder);

        setUpInstructionsOverlay(_gameFolder);

        this.launchSettings.SetUpWithGame(this);
    }

    void setUpInstructionsOverlay(FileInfo gameFolder)
    {
        //UGH
        GameObject nob = new GameObject();
        
        GameCatalog.Instance.StartCoroutine(setUpInstructionsOverlayRoutine(gameFolder));
    }

    IEnumerator setUpInstructionsOverlayRoutine(FileInfo gameFolder)
    {
        string instructionsFolder = gameFolder.FullName + "/instructions";
        if (Directory.Exists(instructionsFolder))
        {

            List<string> imgsInDirectory = GetFilesMultipleSearchPattern(instructionsFolder, new string[] { "*.png" });
            foreach (string v in imgsInDirectory)
            {
                //Debug.Log(v + "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            }
            if (imgsInDirectory.Count > 0)
            {
               string ovlUrl = imgsInDirectory[0];

                this.instructionsOverlay = new Texture2D(4, 4, TextureFormat.DXT5, false); //DXT5, assuming image is png
                WWW instOvlWww = new WWW(ovlUrl);
                yield return instOvlWww;

                instOvlWww.LoadImageIntoTexture(this.instructionsOverlay);

                //GameObject sp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                //sp.GetComponent<Renderer>().material.mainTexture = this.instructionsOverlay;
            }



        }

    }

    void setUpVideo(FileInfo gameFolder)
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
    void setUpExe(FileInfo gameFolder)
    {
        string platform = "windows";

        string exeFolder = gameFolder.FullName + "/" + platform;

        //try to find a .lnk
        string[] shortcutsInGameDirectory = Directory.GetFiles(exeFolder, "*.lnk");
        string[] batchFilesInGameDirectory = Directory.GetFiles(exeFolder, "*.bat");
        if (batchFilesInGameDirectory.Length == 1)
        {
            this.executable = batchFilesInGameDirectory[0];
        }
        else if (shortcutsInGameDirectory.Length == 1) //use a shortcut
        {
            //TODO : need to think of something smarter... 
            //optionally specify start file???
            this.executable = shortcutsInGameDirectory[0];

            //verify existence of link...




        }
        else //try to find an exe...
        {
            //string[] subdirectories = Directory.GetDirectories(gameFolder.FullName);
            string[] exeFolderContents = Directory.GetFiles(exeFolder, "*.exe");

            if (exeFolderContents.Length == 0)
            {
                Debug.Log("couldn't find an exe");
                this.executable = "";
            }
            else if (exeFolderContents.Length > 1)
            {
                Debug.Log("Multiple exes in the folder, don't know which one to use!");
            }
            else //just 1
            {
                this.executable = exeFolderContents[0];
            }
        }



    }

    void setUpImages(FileInfo gameFolder)
    {
        List<string> previewImageFolderContents = GetFilesMultipleSearchPattern(gameFolder.FullName + "/image", new string[] { "*.png", "*.jpg", "*.gif" });

        if (previewImageFolderContents.Count > 0)
        {
            string previewImgPath = previewImageFolderContents[0];

            Camera.main.GetComponent<MonoBehaviour>().StartCoroutine(getImage(previewImgPath));
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

    public string appFile
    {
        get
        {
            FileInfo fi = new FileInfo(executable);
            return fi.Name;
        }
    }


    public void Audit(System.Text.StringBuilder auditMsgStringBuilder)
    {
        GameData dat = this;
        if (string.IsNullOrEmpty(dat.exePath))
        {
            auditMsgStringBuilder.AppendLine(dat.title + " has empty exe path");
        }
        else if (!System.IO.File.Exists(Path.Combine(dat.rootFolder.FullName, dat.exePath)))
        {
            auditMsgStringBuilder.AppendLine(dat.title + ", no file found at specified exe path");
        }

        if (string.IsNullOrEmpty(dat.joyToKeyConfig))
        {
            auditMsgStringBuilder.AppendLine(dat.title + " doesn't specify joy to key config");
        }
        else if (!System.IO.File.Exists(Path.Combine(GameCatalog.Instance.joyToKeyData.directory, dat.joyToKeyConfig)))
        {
            auditMsgStringBuilder.AppendLine(dat.title + ", joytokey config: ;" + dat.joyToKeyConfig + "' not found");
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
        foreach (string searchPattern in searchPatterns)
        {
            string[] results = Directory.GetFiles(path, searchPattern);
            foreach (string individResult in results)
            {
                allResults.Add(individResult);
            }
        }

        return allResults;
    }

    IEnumerator getImage(string texturePath)
    {

        string textureURI = texturePath;

        WWW textureReq = new WWW(textureURI);

        yield return textureReq;


        this.previewImg = textureReq.texture;
    }

}
