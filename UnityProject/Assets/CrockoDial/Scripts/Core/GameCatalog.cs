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
        repopulateCatalogFromDisk(cleanPath);
        SwitcherSettings.OnSaveDataUpdated += this.UpdateFilters;
    }

    public void repopulateCatalogFromDisk(string gamesFolderPath)
    {
        if (string.IsNullOrEmpty(gamesFolderPath))
        {
            Debug.LogError("No games path specified");
            return;
        }


        allGames.Clear();
        games.Clear();
        

        //print("================================================================================");


        //string[] files = Directory.GetFiles(Application.streamingAssetsPath + "/games");

        List<string> gameFolders = new();
        if (Directory.Exists(gamesFolderPath))
        {
            var normalDirectories = Directory.GetDirectories(gamesFolderPath);
            gameFolders.AddRange(normalDirectories);
            foreach(var shortcut in Directory.GetFiles(gamesFolderPath, "*.lnk"))
            {
                try
                {

                    string expandedPath = WinOsUtil.GetWindowsShortcutFileTarget(shortcut);
                    if (!string.IsNullOrEmpty(expandedPath) && Directory.Exists(expandedPath))
                    {
                        gameFolders.Add(expandedPath);
                    }
                }
                catch (System.Exception e) { Debug.LogException(e); }
            }
        }
        else
        {
            Debug.LogError("games path '" + gamesFolderPath + "' not found");
            gameFolders.Clear();
        }
        


        foreach (string gameFolderPathString in gameFolders)
        {
            
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
        }

        ApplyFilters();

        Events.OnRepopulated.Invoke();
    }

    public void UpdateFilters()
    {
        ApplyFilters();
        Events.OnRepopulated.Invoke();
    }

    void ApplyFilters()
    {
        games.Clear();
        games.AddRange(allGames);
        for (int i = 0; i < games.Count; i++)
        {
            var game = games[i];
            bool filter = gameIsFiltered(game);

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
            filtered = 
                //!game.playabilityInfo.intersects(SwitcherSettings.Data._shownGameTypes);
                !game.playabilityInfo.IsSupported(SwitcherSettings.Data._controlMode);
        }
        return filtered;
    }

    public class Callbacks
    {
        public System.Action OnRepopulated = ()=>{};
    }
}




