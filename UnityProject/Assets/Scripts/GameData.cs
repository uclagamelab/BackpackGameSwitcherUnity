using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public class GameData
{
    public string title;
    public string executable;
    public string author;
    public string description;

    public string windowTitle = null;

    string _joyToKeyConfigFile = null;


    public GameData(FileInfo gameFolder)
{

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
        this.commandLineArguments = jsonObject["command arguments"].str;
        this.description = jsonObject["description"].str;
        if (jsonObject.HasField("joytokey cfg"))
        {
            this.joyToKeyConfigFile = jsonObject["joytokey cfg"].str;
        }

        if (jsonObject.HasField("window title"))
        {
            windowTitle = jsonObject["window title"].ToString();
        }
        //Debug.Log(this.title + ", " + this.author + ", " + this.commandLineArguments);
    }

        // --- Find the exe ------------------------
        setUpExe(gameFolder);

        // --- Find the preview images ------------------------
        setUpImages(gameFolder);

    // --- Find the instructions ------------------------
    //TODO
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

        }
        else //try to find an exe...
        {
            //string[] subdirectories = Directory.GetDirectories(gameFolder.FullName);
            string[] exeFolderContents = Directory.GetFiles(exeFolder, "*.exe");

            if (exeFolderContents.Length == 0)
            {
                Debug.Log("couldn't find an exe");
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
            //this.previewImgPath = previewImageFolderContents[0];

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
        set
        {

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
        directory = "";
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
        //HttpUtility hp;
        textureURI = textureURI.Replace("\\", "/");
        if (textureURI.Contains(" "))
        {
            Debug.Log(textureURI);
            //textureURI = WWW.EscapeURL(texturePath);
            textureURI = textureURI.Replace(" ", "^ ");
            Debug.Log(textureURI);

            //Debug.Break();
        }

        WWW textureReq = new WWW(textureURI);

        yield return textureReq;


        this.previewImg = textureReq.texture;
        /*
        WWW imgLoader = new WWW(@"file:///" + @directory +"/"+ @image);
yield return imgLoader;
imgTexture = new Texture2D(1024,1024);
imgTexture = imgLoader.texture;
*/
    }


}
