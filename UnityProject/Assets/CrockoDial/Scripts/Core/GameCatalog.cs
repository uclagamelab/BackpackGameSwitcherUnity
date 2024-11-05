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

    /// <summary>
    /// A list of all game projects loaded from the games folder, including those filtered out
    /// by the current control scheme, (eventually playlist etc...)
    /// </summary>
    public List<GameData> allGames
    {
        get; private set;
    } = new List<GameData>();

    /// <summary>
    /// A list of the active, available games
    /// </summary>
    public List<GameData> games
    {
        get;
        private set;
    } = new List<GameData>();
    public int gameCount => games.CountNullRobust();
    

    public static Callbacks Events = new Callbacks();

    public static GameCatalog Instance
    {
        get; private set;
    }

    private void Awake()
    {
        Instance = this;
        string cleanPath = SwitcherSettings.Data.GamesFolder;
        cleanPath = cleanPath.Replace('\\', '/');
        repopulateCatalog(cleanPath);
    }

    public void repopulateCatalog(string gamesFolderPath)
    {
        if (string.IsNullOrEmpty(gamesFolderPath))
        {
            Debug.LogError("No games path specified");
            return;
        }

        if (games == null)
        {
            games = new List<GameData>();
        }
        else
        {
            games.Clear();
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
                    games.Add(gameData);
                }

            /*}
            catch (System.Exception e)
            {
                Debug.LogError("Problem loading game at : '" + gameFolderPathString + "'\n\t reason: " + e);
            }*/
        }

        allGames.Clear();
        allGames.AddRange(games);

        ApplyFilters();

        Events.OnRepopulated.Invoke();
    }

    public void UpdateFilters()
    {
        games.Clear();
        games.AddRange(allGames);
        ApplyFilters();
        Events.OnRepopulated.Invoke();
    }
    void ApplyFilters()
    {
        for (int i = 0; i < games.Count; i++)
        {
            var game = games[i];
            bool filter = gameIsFiltered(game);
            game.setFiltered(filter);
            if (filter)
            {
                games.RemoveAt(i);
                i--;
            }
        }
    }

    bool gameIsFiltered(GameData game)
    {
        bool filtered = false;
        if (SwitcherSettings.Data._filterGamesBySupportedControls)
        {
            filtered = !game.playabilityInfo.intersects(SwitcherSettings.Data._shownGameTypes);
        }
        return filtered;
    }

    public class Callbacks
    {
        public System.Action OnRepopulated = ()=>{};
    }
}




