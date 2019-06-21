using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

using Debug = UnityEngine.Debug;

/*
 * This script finds all the text files in GamesInfo and parses them into GameData objects (also defined here)
 * 
*/

public class GameCatalog : MonoBehaviour
{
	public GameData joyToKeyData;
    List<GameData> allGames;

    public IList<GameData> games
    {
        get { return allGames; }
    }

    public int gameCount
    {
        get { return games.Count; }
    }

    public static Callbacks Events = new Callbacks();

    static GameCatalog _instance;
    public static GameCatalog Instance
    {
        get
        {
            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
        string cleanPath = SwitcherSettings.Data.GamesFolder;
        cleanPath = cleanPath.Replace('\\', '/');
        repopulateCatalog(cleanPath);
    }

    void Start () 
	{
        
	}
    
    
    public void repopulateCatalog(string gamesFolderPath)
    {
        this.joyToKeyData = new GameData();

        // directory = (Application.streamingAssetsPath + "\\JoyToKey"),
        this.joyToKeyData.executable = SwitcherSettings.Data.JoyToKeyFolder + "/JoyToKey.exe";//Application.streamingAssetsPath + "\\~Special" + "\\JoyToKey\\JoyToKey.exe";
        //this.joyToKeyData.commandLineArguments = ""; //does it actually need some???

        if (allGames == null)
        {
            allGames = new List<GameData>();
        }
        else
        {
            allGames.Clear();
        }

        //print("================================================================================");


        //string[] files = Directory.GetFiles(Application.streamingAssetsPath + "/games");

        string[] gameFolders;
        if (Directory.Exists(gamesFolderPath))
        {
            gameFolders = Directory.GetDirectories(gamesFolderPath);
        }
        else
        {
            Debug.LogError("games path '" + gamesFolderPath + "' not found");
            gameFolders = new string[] { };
        }
        


        foreach (string gameFolderPathString in gameFolders)
        {
            

            /*try
            {*/
                bool shouldSkip = new FileInfo(gameFolderPathString).Name.StartsWith("~");
                if (shouldSkip)
                {
                    //Debug.Log("Starts with '~', so IGNORING : " + gameFolderPathString);
                    continue;
                }

                GameData gameData = new GameData(gameFolderPathString.Replace('\\', '/'));
                if (gameData.valid)
                {
                    allGames.Add(gameData);
                }

            /*}
            catch (System.Exception e)
            {
                Debug.LogError("Problem loading game at : '" + gameFolderPathString + "'\n\t reason: " + e);
            }*/
        }

        Events.OnRepopulated.Invoke();
    }

    public class Callbacks
    {
        public System.Action OnRepopulated = ()=>{};
    }
}




