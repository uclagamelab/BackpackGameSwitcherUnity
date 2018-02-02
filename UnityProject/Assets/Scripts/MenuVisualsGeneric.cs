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

    float attemptedLaunchTime = float.NegativeInfinity;
    bool inProcessOfLaunching
    {
        get { return Time.time < attemptedLaunchTime + 3; }
    }

    

    List<Listener> listners;
    public GameObject loadingScreen;
    int gameIdx = 0;

    GameInfoUI gameInfoUI;
    bool animating = false;

    public Text errorText;

    //bool ignoreInputTemporarily = false;




    private void Awake()
    {
        this.listners = new List<Listener>();
    }

    public void addListener(Listener l)
    {
        this.listners.Add(l);
    }

    // Use this for initialization
    void Start () {
        gameInfoUI = this.GetComponentInChildren<GameInfoUI>();
        setAttractMode(true);
        updateInfoDisplay(currentlySelectedGame);
    }
	
    public void setAttractMode(bool attract)
    {
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

    // Update is called once per frame

    //TODO need to separate concerns...
    //Reverting to state, if no input for one.
    //Actually, input should probably go into controller??






    public GameData currentlySelectedGame
    {
        get
        {
            return GameCatalog.Instance.games[gameIdx];
        }
    }

    public void cycleToNextGame(int selectionDirection)
    {
        if (AttractMode.Instance.running || animating)
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



    public void onStartGameButtonPress()
    {
        bool gameHasExe = currentlySelectedGame.executable != "";

        if (inProcessOfLaunching || !gameHasExe)// || AttractMode.Instance.running)
        {

            if (!gameHasExe)
            {
                this.showErrorText(currentlySelectedGame.title + " is video only.");
            }

            return;
        }

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
        



        //bool playingGame = ProcessRunner.instance.gameProcessIsRunning;


        foreach (Listener l in this.listners)
            {
                l.onQuitGame();
            }
        

        //  this.state = State.CHOOSING;
        //setAttractMode(true);
     
    }

    public interface Listener
    {
        void onLeaveAttract();
        void onEnterAttract();
        void onCycleGame();
        void onStartGame();
        void onQuitGame();


    }


}
