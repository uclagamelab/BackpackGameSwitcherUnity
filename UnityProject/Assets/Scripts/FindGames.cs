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
	public ArrayList gameDataList;	//Holds all GameData objects. Is accessed by Menu.CS when populating the menu.
	public GameData joyToKeyData;
    List<GameData> allGames;
    public int gameIdx = 0;

    GameInfoUI gameInfoUI;

	void Start () 
	{

        populateGameList();
        //populateGameListOrig();
        gameInfoUI = this.GetComponentInChildren<GameInfoUI>();






	}
	
    bool animating= false;
	// Update is called once per frame
	void Update () 
	{

        int selectionChange = 0;
        if (!animating)
        {
            selectionChange = Input.GetKeyDown(KeyCode.W) ? 1 : Input.GetKeyDown(KeyCode.S) ? -1 : 0;
            //print(selectionChange);
        }

        if (selectionChange != 0)
        {
            animating = true;
           
            bool flipped = false;
            this.varyWithT((float rt) => 
            {
                float t = EasingFunctions.Calc(rt, EasingFunctions.QuadEaseInOut);
                
                //gameInfoUI.GetComponent<CanvasGroup>().alpha = 1 - Mathf.PingPong(2 * t, 1);
                gameInfoUI.transform.localScale = new Vector3(
                    1 - Mathf.PingPong(2 * t, 1),
                    1 - Mathf.PingPong(2 * t, 1),
                    1);

                float t2 = rt;//Mathf.PingPong(2*rt, 1);//

                gameInfoUI.transform.eulerAngles = Vector3.forward * 1080 * t2;//;

                if (t >= .5f && !flipped)
                {
                    flipped = true;
                    gameIdx = (gameIdx + selectionChange + allGames.Count) % allGames.Count;
                    //animating = false;
                }

                if (rt == 1)
                {
                    animating = false;
                }

            }, 0.75f);
        }
        updateInfoDisplay();

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            GameData currentGameData = this.allGames[gameIdx];
            //print("________________________" + currentGameData.appFile);
            ProcessRunner.instance.OpenProcess (@currentGameData.directory, currentGameData.appFile, "-popupwindow -screen-width 1920 -screen-height 1080", currentGameData.joyToKeyConfigFile);   
        }
	}

    void startGameExe()
    {
        
    }

    void updateInfoDisplay()
    {
        GameData currentGameData = this.allGames[gameIdx];
      

        //TODO : should move into GameInfoUi
        gameInfoUI.titleText.text = currentGameData.title;
        gameInfoUI.descriptionText.text = 
            "Made By: " + currentGameData.author
            + "\n\n"
            + currentGameData.description;

        gameInfoUI.previewImage.texture = currentGameData.previewImg;
    }

    void populateGameListOrig()
    {
        gameDataList = new ArrayList();
        DirectoryInfo data = Directory.CreateDirectory(Application.dataPath +@"\GamesInfo");    //Get the directory of GamesInfo
        //DirectoryInfo exes = Directory.CreateDirectory (Application.dataPath+@"\GamesExes");

        FileInfo[] fileInfo = data.GetFiles("*.*", SearchOption.TopDirectoryOnly);  // Load files from GamesInfo into fileInfo

        foreach (FileInfo file in fileInfo)
        {
            if (file.Extension == ".txt") 
            {
                parseText (file.FullName); //parses the .txt file, stores the data in gameDataList as a GameData object
            }
        }


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



	void parseText(string fileName) //Parses the formatted text files and populates gameDataList with information
	{
		string line;
		// Create a new StreamReader, tell it which file to read and what encoding the file
		// was saved as
		StreamReader theReader = new StreamReader(fileName, Encoding.Default);

		using (theReader)
		{

			GameData gd = new GameData();
			
			do
			{
				line = theReader.ReadLine();
				
				if (line != null)
				{
					string[] entries = line.Split(new char[] { ':' }, 2); //split each line at first : character.

					if (entries.Length == 2)
					{
						if(entries[0] == "exe")
						{
							gd.executable = entries[1].Trim ();
						}
						if(entries[0] == "title")
						{
							gd.title = entries[1].Trim ();
						}
						if(entries[0] == "author")
						{
							gd.author = entries[1].Trim();
						}
						if(entries[0] == "description")
						{
							gd.description = entries[1].Trim ();
						}
						if(entries[0] == "directory")
						{
							gd.directory = entries[1].Trim();
						}
						if(entries[0] == "image")
						{
							gd.image = entries[1].Trim();
						}
						if(entries[0] == "isUnity" && entries[1].Trim () == "false")
						{
							UnityEngine.Debug.Log("parseText in FindGames.CS detected a non-unity game");
							gd.isUnityApp = false;
						}
					}
				}
			}
			while (line != null);
			if(gd.title != "JoyToKey")
			{
				gameDataList.Add (gd);
			}else{
				joyToKeyData = gd;
			}
			// Done reading, close the reader and return true to broadcast success    
			theReader.Close();
			return;
		}
	}

}




