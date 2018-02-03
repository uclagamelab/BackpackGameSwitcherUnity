/*
 This class also seems a little heavy...
 */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;



public class GameData
{

    //[DllImport("shell32.dll")]

    public string title;
    public string executable;
    public string author;
    public string description;

    public string windowTitle = null;

    string _joyToKeyConfigFile = null;

    public string videoUrl = null;

    // Dictionary<string, string> controllerLabels;
    string[] controlLabels; // convention is '0' is joystick, 1-6 are buttons
    public string joystickLabel
    {
        get { return controlLabels[0]; }
    }

    public string getButtonLabel(int buttonIdx)
    {
        return controlLabels[buttonIdx];
    }


    public GameData(string gameFolderPath)
    {

        FileInfo gameFolder = new FileInfo(gameFolderPath);

        controlLabels = new string[7];
        for (int i = 0; i < controlLabels.Length; i++)
        {
            controlLabels[i] = null;
        }

    // --- Find the JSON info file ---------
        string[] jsonFiles = Directory.GetFiles(gameFolder.FullName, "*.json");

    if (jsonFiles.Length == 0)
    {
        Debug.LogError("Couldn't find Json file in '" + gameFolder.FullName + "'");
    }
    else
    {
        string jsonFilePath = jsonFiles[0];
        //assuming just 1 JSON file...
        string jsonString = File.ReadAllText(jsonFilePath);
        JSONObject jsonObject = new JSONObject(jsonString);

        this.title = jsonObject["title"].str;
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

        //Debug.Log(this.title + ", " + this.author + ", " + this.commandLineArguments);
    }

        // --- Find the exe ------------------------
        setUpExe(gameFolder);

        // --- Find the preview images ------------------------
        setUpImages(gameFolder);

        // --- Find the instructions ------------------------

        //--- set up video ---------------
        setUpVideo(gameFolder);
    //TODO
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
        if (shortcutsInGameDirectory.Length == 1) //use a shortcut
        {
            //TODO : need to think of something smarter... 
            //optionally specify start file???
            this.executable = shortcutsInGameDirectory[0];

            //verify existence of link...
            checkShortcutValidity(this.executable);




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

    public string joyToKeyConfigFile
    {
        get
        {
            if (_joyToKeyConfigFile == null || _joyToKeyConfigFile == "")
            {
                return "default.cfg";
            }
            else
            {
                return _joyToKeyConfigFile;
            }
        }

        set
        {
            _joyToKeyConfigFile = value;
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


    public string image;
    public bool isUnityApp;

    //public string previewImgPath;

    public string commandLineArguments;

    public Texture previewImg;

    enum GameType
    {
        unity, processing, other
    }


    public GameData()
    {
        title = "";
        executable = "";
        author = "";
        description = "";
        isUnityApp = true;

    }



    void customStopAction()
    {
        //string  stop= "call sendKeys.bat \"Gmaeish\" \"%{f4}\"";
    }

    public void stopGame(System.IntPtr processId)
    {

    }

    void stopViaAlt4(string windowTitle)
    {
        string sendkeyscall = "call " + Application.streamingAssetsPath + "\\~Special\\sendKeys.bat \"Gmaeish\" \"%{ f4}\"";
        /**/
    }

    void recursiveKillProcess(System.IntPtr processId)
    {
        //TerminateProcessTree(_runningProcess.Handle, (uint) _runningProcess.Id, 0);
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

    void checkShortcutValidity(string shortcutPath)
    {
        //UnityEngine.Debug.Log(LnkToFile(shortcutPath));
        //nityEngine.Debug.Break();

    }

    private static string LnkToFile(string fileLink)
    {
        string link = File.ReadAllText(fileLink);
        int i1 = link.IndexOf("DATA\0");
        if (i1 < 0)
            return null;
        i1 += 5;
        int i2 = link.IndexOf("\0", i1);
        if (i2 < 0)
            return link.Substring(i1);
        else
            return link.Substring(i1, i2 - i1);
    }

}
