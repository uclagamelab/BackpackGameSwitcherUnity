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
    static XUSingleTown<MenuVisualsGeneric> _InstanceHelper = new XUSingleTown<MenuVisualsGeneric>();
    public static MenuVisualsGeneric Instance => _InstanceHelper.instance;
    
    public System.Action<bool> OnOpenCloseInfo = (open) => { };
    public System.Action<bool> InfoMenuCursorMove = (dirIsRight) => { };
    public System.Action OnStartGame = () => { };

    public enum MenuState { ChooseGame, GameInfo, LaunchGame };
    public MenuState state
    {
        get;
        private set;
    }
    #region FIELDS



    public int gameIdx
    {
        get;
        private set;
    }

    PreLaunchGameInfo gameInfoV2 => PreLaunchGameInfo.Instance;


    const float IN_PROCESS_OF_LAUNCHING_MAX_TIME = 3;
    float attemptedLaunchTimer = 0;
    #endregion

    #region PROPERTIES

    public GameData currentlySelectedGame
    {
        get
        {
            return SpeedyListView.instance.currentGame;
        }
    }

    #endregion

    private void Awake()
    {
        state = MenuState.ChooseGame;
    }

    // Use this for initialization
    void Start()
    {
        //updateInfoDisplay(currentlySelectedGame, 0);


        selectRandomGame();
        SwitcherApplicationController.events.OnAppQuittedOnItsOwn += onQuitGame;
        SwitcherApplicationController.events.OnEnterIdle += () => setAttractMode(true);
    }

    private void Update()
    {
        UpdateBackground();

        bool aGameIsRunning = ProcessRunner.instance.IsGameRunning();
        if (aGameIsRunning)
        {

        }
        else
        {
            //--- LEGACY INPUT STYLE, JUST SLIDING THE BACKGROUND -----------------------
            int selectionDirection = 0;
            selectionDirection = CrockoInput.PrelaunchMenuInput.OnSelectRight() ? 1 : CrockoInput.PrelaunchMenuInput.OnSelectLeft() ? -1 : 0;
            if (selectionDirection != 0)
            {
                onCycleButtonPressed(selectionDirection);
            }
            //----------------------------------------------------------------------------

            if (CrockoInput.GetOpenGameButtonDown())
            {
                onStartGameButtonPress();
            }

            autoCycleGamesIfNoInput();
        }

        if (attemptedLaunchTimer >= 0)
        {
            attemptedLaunchTimer -= Time.deltaTime;
        }
    }

    public void selectRandomGame()
    {
        gameIdx = Random.Range(0, GameCatalog.Instance.games.Count);
    }


    public void onCycleButtonPressed(int selectionDirection, bool fromUser = true)
    {
        if (state != MenuState.LaunchGame && gameInfoV2.open)
        {
            bool infoInputAccepted = gameInfoV2.TakeDirectionInput(selectionDirection);
            if (infoInputAccepted && fromUser)
            {
                InfoMenuCursorMove.Invoke(selectionDirection > 0);
            }
        }
    }


	
    public void setAttractMode(bool attract)
    {
        if (attract)
        {
            this.state = MenuState.ChooseGame;
        }
    }

    GameData _lastAppliedGameData = null;
    int _lastVidMoveDirection = 1;
    void UpdateBackground()
    {
        if (!SpeedyListView.instance.keyHeld && currentlySelectedGame != _lastAppliedGameData && !BackgroundDisplay.Instance.animating)
        {
            _lastAppliedGameData = currentlySelectedGame;
            BackgroundDisplay.Instance.setDisplayedGame(currentlySelectedGame, _lastVidMoveDirection);
        }
    }

    const float ATTRACT_AUTO_CYCLE_DURATION = 60;
    float nextAttractAutoCyleTimer = ATTRACT_AUTO_CYCLE_DURATION;
    void autoCycleGamesIfNoInput()
    {
            if (SwitcherApplicationController.isIdle)
            {
                //Debug.Log("zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz");
                setAttractMode(true);
                if (nextAttractAutoCyleTimer > 0)
                {
                    nextAttractAutoCyleTimer -= Time.deltaTime;
                }
                else
                {
                    //gameMenu.onCycleButtonPressed(1, false);
                    OnAttractCycleNextGame.Invoke();
                    nextAttractAutoCyleTimer = ATTRACT_AUTO_CYCLE_DURATION;
                }

            }
    }

    public static System.Action OnAttractCycleNextGame = () => { };

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

        if (attemptedLaunchTimer > 0 || !gameHasExe)// || AttractMode.Instance.running)
        {
            return false;
        }

        attemptedLaunchTimer = IN_PROCESS_OF_LAUNCHING_MAX_TIME;

        OnStartGame.Invoke();
        

        this.delayedFunction(() => 
        {
            GameData currentGameData = currentlySelectedGame;
            ProcessRunner.instance.StartGame(currentGameData);
        }, .1f);

        return true;
    }

    public void onQuitGame()
    {
        //If coming out of a game, auto switch 1 game 
        //so you don't see video of the game you were just playing
        //and so you see the video scroll, if you're just walking up

        this.state = MenuState.ChooseGame;
    }



}
