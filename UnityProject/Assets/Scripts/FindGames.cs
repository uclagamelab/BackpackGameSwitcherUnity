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

public class FindGames : MonoBehaviour
{
	public GameData joyToKeyData;
    List<GameData> allGames;

    public IList<GameData> games
    {
        get { return allGames.AsReadOnly(); }
    }

    public int gameCount
    {
        get { return games.Count; }
    }
    

    static FindGames _instance;
    public static FindGames Instance
    {
        get
        {
            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    void Start () 
	{
        populateGameList();
        //populateGameListOrig(); 
	}
	
   
	// Update is called once per frame
	void Update ()
    {         

   
	}

    void startGameExe()
    {
        
    }


    void populateGameList()
    {
        allGames = new List<GameData>();
        //print("================================================================================");


        //string[] files = Directory.GetFiles(Application.streamingAssetsPath + "/games");

        string[] gameFolders = Directory.GetDirectories(Application.streamingAssetsPath + "/~Special" + "/games");


        foreach (string gameFolderPathString in gameFolders)
        {
            FileInfo gameFolder = new FileInfo(gameFolderPathString);
            GameData gameData = new GameData(gameFolder);

            allGames.Add(gameData);

            //print("!!! - " + gameFolder.Name);



            //StartCoroutine(getImage(gameData.previewImgPath, gameInfoUI));


        }

        this.joyToKeyData = new GameData();

        // directory = (Application.streamingAssetsPath + "\\JoyToKey"),
        this.joyToKeyData.executable = Application.streamingAssetsPath + "\\~Special" + "\\JoyToKey\\JoyToKey.exe";
        this.joyToKeyData.commandLineArguments = ""; //does it actually need some???
        
        //this.joyToKeyData.directory = (Application.streamingAssetsPath + "\\JoyToKey");


    }
}




