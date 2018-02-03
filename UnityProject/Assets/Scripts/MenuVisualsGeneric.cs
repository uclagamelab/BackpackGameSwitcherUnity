/*
 Model{
    list of games,
    current selected games,
    state {Choosing, Loading, PlayingGame}
 }
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuVisualsGeneric : MonoBehaviour
{


    #region FIELDS
    List<Listener> listners;
    int gameIdx = 0;

    GameInfoUI gameInfoUI;
    bool animating = false;

    public Text errorText;
    float attemptedLaunchTime = float.NegativeInfinity;
    #endregion

    #region PROPERTIES
    bool inProcessOfLaunching
    {
        get { return Time.time < attemptedLaunchTime + 3; }
    }

    public GameData currentlySelectedGame
    {
        get
        {
            if (gameIdx >= GameCatalog.Instance.gameCount)
            {
                return null;
            }
            return GameCatalog.Instance.games[gameIdx];
        }
    }

    #endregion





    private void Awake()
    {
        this.listners = new List<Listener>();
    }



    // Use this for initialization
    void Start () {
        gameInfoUI = this.GetComponentInChildren<GameInfoUI>();
        setAttractMode(true);
        updateInfoDisplay(currentlySelectedGame);
        
    }
	
    public void setAttractMode(bool attract)
    {
        this.showLoadingScreen(false);
        AttractMode.Instance.running = attract;
        this.gameInfoUI.gameObject.SetActive(!attract);
        BackgroundDisplay.Instance.gameObject.SetActive(!attract);

        if (attract)
        {
            foreach (Listener l in this.listners)
            {
                l.onEnterAttract();
            }
        }
        else
        {
            foreach (Listener l in this.listners)
            {
                l.onLeaveAttract();
            }
        }
    
    }


    public void cycleToNextGame(int selectionDirection)
    {
        

        if (AttractMode.Instance.running || animating || GameCatalog.Instance.gameCount == 0)
        {
            return;
        }


            foreach (Listener l in this.listners)
            {
                l.onCycleGame();
            }
            animating = true;
            
            this.varyWithT((float t) => 
            {
                this.gameInfoUI.GetComponent<CanvasGroup>().alpha = 1 - t;
                if (t == 1)
                {
                    this.gameIdx = (gameIdx + selectionDirection + GameCatalog.Instance.gameCount) % GameCatalog.Instance.gameCount;
                    this.updateInfoDisplay(GameCatalog.Instance.games[this.gameIdx]);
                    this.varyWithT((float t2) =>
                    {
                        this.gameInfoUI.GetComponent<CanvasGroup>().alpha = t2;
                        if (t2 == 1)
                        {
                            animating = false;
                        }
                    }, .45f);
                }
            }, .25f);

        
      
    }

    void updateInfoDisplay(GameData currentGameData)
    {
        if (currentGameData == null)
        {
            return;
        }

        //TODO : should move into GameInfoUi
        gameInfoUI.titleText.text = currentGameData.title;
        gameInfoUI.descriptionText.text = currentGameData.description;
            
            gameInfoUI.creditText.text = "by " + currentGameData.author + "";

        //gameInfoUI.previewImage.texture = currentGameData.previewImg;
        if (currentGameData.videoUrl != null)
        {
            BackgroundDisplay.Instance.setVideo(currentGameData.videoUrl);
        }
        else
        {
            BackgroundDisplay.Instance.setImage(currentGameData.previewImg);
        }

        //update the displayed controls

        if (currentGameData.joystickLabel == null)
        {
            gameInfoUI.joystickLabel.SetActive(false);
        }
        else
        {
            gameInfoUI.joystickLabel.SetActive(true);
            gameInfoUI.joystickLabel.GetComponentInChildren<Text>().text = currentGameData.joystickLabel;
        }

        
        for (int i = 1; i <= 6; i++)
        {
            GameObject buttonLabel = gameInfoUI.buttonLabels[i - 1];

            if (currentGameData.getButtonLabel(i) == null)
            {
                buttonLabel.SetActive(false);
            }
            else
            {
                buttonLabel.SetActive(true);
                buttonLabel.GetComponentInChildren<Text>().text = currentGameData.getButtonLabel(i);
            }
        }

    }

    public void showLoadingScreen(bool show)
    {
        BackgroundDisplay.Instance.gameObject.SetActive(!show);
        this.gameInfoUI.gameObject.SetActive(!show);
        LoadingScreen.instance.on = show;
    }

    //returns if accepted press
    public bool onStartGameButtonPress()
    {
        if (currentlySelectedGame == null)
        {
            return false;
        }


        


        bool gameHasExe = currentlySelectedGame.executable != "";

        if (inProcessOfLaunching || !gameHasExe)// || AttractMode.Instance.running)
        {

            if (!gameHasExe)
            {
                this.showErrorText(currentlySelectedGame.title + " is video only.");
            }

            return false;
        }

        showLoadingScreen(true);
        attemptedLaunchTime = Time.time;

        foreach (Listener l in this.listners)
        {
            l.onStartGame();
        }

        //this.state = State.PLAYING_GAME;
        this.varyWithT((float t) => {
            float cale = 1 - .1f * (Mathf.PingPong(2 * t, 1));
            if (t == 1)
            {
                cale = 1;
            }
            this.gameInfoUI.transform.localScale = Vector3.one * cale;

            if (t == 1)
            {
                GameData currentGameData = currentlySelectedGame;//this.allGames[gameIdx];
                                                                 //print("________________________" + currentGameData.appFile);
                ProcessRunner.instance.OpenProcess(@currentGameData.directory, currentGameData.appFile, currentGameData.commandLineArguments, currentGameData.joyToKeyConfigFile);
                BackgroundDisplay.Instance.stopAllVideos();
            }

            }, .1f);

        return true;
    }

    void showErrorText(string error)
    {
        this.varyWithT((float t) => {

            errorText.text = error;

            errorText.enabled = true;// (t * 8) % 1 > .3f;
            if (t == 1)
            {
                errorText.enabled = false;
            }
        }, 3);
    }

    public void onQuitGame()
    {

        this.showLoadingScreen(false);

        foreach (Listener l in this.listners)
            {
                l.onQuitGame();
            }     
    }

    public interface Listener
    {
        void onLeaveAttract();
        void onEnterAttract();
        void onCycleGame();
        void onStartGame();
        void onQuitGame();
    }
    public void addListener(Listener l)
    {
        this.listners.Add(l);
    }

}
