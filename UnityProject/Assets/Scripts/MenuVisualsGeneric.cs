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
using UnityEngine.Events;
using UnityEngine.UI;

public class MenuVisualsGeneric : MonoBehaviour
{

    [SerializeField]
    CanvasGroup _slideshowTitleTabCanvasGroup;

    public static MenuVisualsGeneric Instance
    {
        get;
        private set;
    }

    public Text _titleText;
        public Text _creditText;

    public delegate void OnCloseInfoCB(bool open);
    public OnCloseInfoCB OnOpenCloseInfo = (open) => { };

    public OnCloseInfoCB InfoMenuCursorMove = (dirIsRight) => { };

    public enum MenuState { ChooseGame, GameInfo, LaunchGame };
    public MenuState state
    {
        get;
        private set;
    }
    #region FIELDS
    List<Listener> listners;
    public int gameIdx
    {
        get;
        private set;
    }

    GameInfoUI gameInfoUI;
    PreLaunchGameInfo gameInfoV2;

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


    public void selectRandomGame()
    {
        gameIdx = 0;// Random.Range(0, GameCatalog.Instance.games.Count);
        this.updateInfoDisplay(this.currentlySelectedGame, 0);
    }


    public void onCycleButtonPressed(int selectionDirection, bool fromUser = true)
    {
        if (state != MenuState.LaunchGame)
        {
            if (gameInfoV2.open)
            {
                bool infoInputAccepted = gameInfoV2.TakeDirectionInput(selectionDirection);
                if (infoInputAccepted && fromUser)
                {
                    InfoMenuCursorMove.Invoke(selectionDirection > 0);
                }
            }
            else
            {
                //cycleToNextGame(-selectionDirection, false, fromUser);
            }
        }
    }

        private void Awake()
    {
        Instance = this;
        state = MenuState.ChooseGame;
        this.listners = new List<Listener>();
        gameInfoUI = this.GetComponentInChildren<GameInfoUI>();
    }



    // Use this for initialization
    void Start ()
    {
        //updateInfoDisplay(currentlySelectedGame, 0);
        gameInfoV2 = this.GetComponentInChildren<PreLaunchGameInfo>(true);

        selectRandomGame();
    }
	
    public void setAttractMode(bool attract)
    {
        if (attract)
        {
            this.state = MenuState.ChooseGame;
        }
        /*this.showLoadingScreen(false);
        AttractMode.Instance.running = attract;
        this.gameInfoUI.gameObject.SetActive(!attract);
        

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
        }*/
    
    }


    public void cycleToNextGame(int selectionDirection, bool forceDuringAttractNoAnimation =false, bool fromUser = true)
    {


        if (BackgroundDisplay.Instance.animating
            ||
            animating
            ||
            PreLaunchGameInfo.Instance.animating
            || 
            GameCatalog.Instance.gameCount == 0)
        {
            return;
        }


            foreach (Listener l in this.listners)
            {
                l.onCycleGame(selectionDirection, fromUser);
            }

            if (!forceDuringAttractNoAnimation)
            {
            animating = true;

            this.varyWithT((float t) =>
            {
                _slideshowTitleTabCanvasGroup.alpha = 1 - t;
                if (t == 1)
                {
                    this.gameIdx = (gameIdx + selectionDirection + GameCatalog.Instance.gameCount) % GameCatalog.Instance.gameCount;
                    this.updateInfoDisplay(GameCatalog.Instance.games[this.gameIdx], selectionDirection);
                    this.varyWithT((float t2) =>
                    {
                        _slideshowTitleTabCanvasGroup.alpha = t2;
                        if (t2 == 1)
                        {
                            animating = false;
                        }
                    }, .45f);
                }
            }, .25f);
        }
            else
        {
            this.gameIdx = (gameIdx + selectionDirection + GameCatalog.Instance.gameCount) % GameCatalog.Instance.gameCount;
            this.updateInfoDisplay(GameCatalog.Instance.games[this.gameIdx], selectionDirection);
        }

           

        
      
    }

    void updateInfoDisplay(GameData currentGameData, int updateDirection)
    {
        if (currentGameData == null)
        {
            return;
        }

        BackgroundDisplay.Instance.visible = true;

        //TODO : should move into GameInfoUi
        _titleText.text = currentGameData.title.Replace('\n', ' '); ;
        //gameInfoUI.descriptionText.text = currentGameData.description;

        _creditText.text = "by " + currentGameData.designers + "";

        //gameInfoUI.previewImage.texture = currentGameData.previewImg;
        if (currentGameData.videoUrl != null)
        {
            BackgroundDisplay.Instance.setVideo(currentGameData.videoUrl, updateDirection);
        }
        else
        {
            Debug.Log("only an image");
            BackgroundDisplay.Instance.setImage(currentGameData.previewImg, updateDirection);
        }

    }



    //returns if accepted press
    public bool onStartGameButtonPress()
    {
        if (PreLaunchGameInfo.Instance.animating)
        {
            return false;
        }

        if (!this.gameInfoV2.open)
        {
            state = MenuState.GameInfo;
            this.gameInfoV2.AnimateOpen(true);

            OnOpenCloseInfo.Invoke(true);
            return false;
        }
        else if (this.gameInfoV2.backButtonHighighted)
        {
                state = MenuState.ChooseGame;
                this.gameInfoV2.AnimateOpen(false);
            OnOpenCloseInfo.Invoke(false);
            return false;
        }



        this.gameInfoV2.AnimateOpen(false);

        state = MenuState.LaunchGame;


        bool gameHasExe = currentlySelectedGame.executable != "";

        if (inProcessOfLaunching || !gameHasExe)// || AttractMode.Instance.running)
        {
            return false;
        }

        attemptedLaunchTime = Time.time;

        foreach (Listener l in this.listners)
        {
            l.onStartGame();
        }

        this.delayedFunction(() => 
        {
            GameData currentGameData = currentlySelectedGame;
            ProcessRunner.instance.StartGame(currentGameData);
            BackgroundDisplay.Instance.stopAllVideos();
        }, .1f);

        return true;
    }

    public void showLoadingScreen(bool show)
    {
        //BackgroundDisplay.Instance.gameObject.SetActive(!show);
        this.gameInfoUI.gameObject.SetActive(!show);
        //LoadingScreen.instance.on = show;
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
        //If coming out of a game, auto switch 1 game 
        //so you don't see video of the game you were just playing
        //and so you see the video scroll, if you're just walking up
        
        if (state == MenuState.LaunchGame)
        {
            //this.cycleToNextGame(1);
            if (Screen.width != 1920)
            {
                Screen.SetResolution(1920, 1080, true);
            }

            foreach (Listener l in this.listners)
            {
                l.onQuitGame();
            }
        }

        this.state = MenuState.ChooseGame;

        

        this.showLoadingScreen(false);
       

   
    }

    public interface Listener
    {
        void onLeaveAttract();
        void onEnterAttract();
        void onCycleGame(int direction, bool userInitiated);
        void onStartGame();
        void onQuitGame();
    }
    public void addListener(Listener l)
    {
        this.listners.Add(l);
    }

}
