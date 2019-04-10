/*
 
Collects input from gamepads, keyboard etc... and controls the menu
 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SwitcherApplicationController : MonoBehaviour, BackgroundKeyboardInput.Listener {
    MenuVisualsGeneric gameMenu;

    bool gotAnExitCombo = false; //consumable event

    //float attractTimeOut = 120;

    bool gameProcessWentNullOrExitedLastUpdate = true;

    float lastFocusSwitchAttemptTime = float.NegativeInfinity;

    bool generatedSimulatedKeypressForFocusSwitchToSwitcherApp = false;

   

    float timeOfLastQuit = float.NegativeInfinity;
    bool didntQuitRecently
    {
        get
        {
            return Time.time > timeOfLastQuit + .5f;
        }
    }


    float idleTimeout = 6 * 60;
    float attractAutoCycleDuration = 2;

    bool _defaultIsFullScreen = true;
    Resolution _defaultResolution;

    // Use this for initialization
    void Awake () {
        gameMenu = this.GetComponent<MenuVisualsGeneric>();
        _defaultResolution = Screen.currentResolution;
        _defaultIsFullScreen = Screen.fullScreen;
    }

    void Start()
    {
        BackgroundKeyboardInput.Instance.addListener(this);
        GenericSettings.OnSettingsUpdated += OnSettingsUpdated;
        OnSettingsUpdated();
    }

    void OnSettingsUpdated()
    {
        GenericSettings.TryGetValue("idle_timeout", out idleTimeout, 6 * 60);
        GenericSettings.TryGetValue("idle_cycle_duration", out attractAutoCycleDuration, 90);
    }

    public void ResetToDefaultResolutionIfDifferent()
    {
        #if !UNITY_EDITOR
        if (Screen.width != _defaultResolution.width || Screen.height != _defaultResolution.height)
        {
            Screen.SetResolution(_defaultResolution.width, _defaultResolution.height, _defaultIsFullScreen);
        }
        #endif
    }


    // Update is called once per frame
    void Update()
    {



        //Don't focus steal while menu is open
        if (ToolsAndSettingsMenu.isOpen)
        {
            return;
        }
        autoCycleGamesIfNoInput();
        bool aGameIsRunning = ProcessRunner.instance.IsGameRunning();
        //--------------------------------------------------------------------
        bool processWentNullOrExitedThisUpdate = !aGameIsRunning;
        if (processWentNullOrExitedThisUpdate && !gameProcessWentNullOrExitedLastUpdate)
        {
            if (gotAnExitCombo)
            {
                print("game exited with combo!");
            }
            else
            {
                print("game exited some other way");
                this.gameMenu.onQuitGame();

            }
            ResetToDefaultResolutionIfDifferent();

            gotAnExitCombo = false;
        }

        gameProcessWentNullOrExitedLastUpdate = processWentNullOrExitedThisUpdate;

        //Make Switcher Retake focus, if necessary
        if (Time.time > lastFocusSwitchAttemptTime + 1)
        {
            lastFocusSwitchAttemptTime = Time.time;
            if (_lastActionWasQuit) //make the switcher retake focus.. if the user is trying to quit.
            {
                //print("bring switcher to the frongt!");
                ProcessRunner.instance.BringThisToForeground();

            }
            else if (ProcessRunner.instance.gameProcessIsRunning) // have the game try to retake focus, if one is running.
            {
                //print("give it a try");
                generatedSimulatedKeypressForFocusSwitchToSwitcherApp = true;
                if (!string.IsNullOrEmpty(gameMenu.currentlySelectedGame.windowTitle))
                {
                    ProcessRunner.instance.BringRunningToForeground(gameMenu.currentlySelectedGame); //this function should be robust to repeated calls
                }
                
            }
        }



        if (!aGameIsRunning)
        {

            selectingGameUpdate();

        }
        else //A Game is running...
        {
            gameRunningUpdate();
        }
    }

    void selectingGameUpdate()
    {
        bool changedAttractTimeTooRecently = false;// Time.time < AttractMode.Instance.changeTime + .5f;

        if (changedAttractTimeTooRecently)
        {
            return;
        }

        int selectionDirection = 0;
        selectionDirection = Input.GetKeyDown(KeyCode.W) ? 1 : Input.GetKeyDown(KeyCode.S) ? -1 : 0;
        if (selectionDirection != 0)
        {
            gameMenu.onCycleButtonPressed(selectionDirection);
        }


        if (Input.GetKeyDown(KeyCode.UpArrow))
        {

            bool accepted = this.gameMenu.onStartGameButtonPress();
            //if (accepted)
            //{
                _lastActionWasQuit = false;
            //}


        }
    }

    
    void gameRunningUpdate()
    {
        // quit out of a game, if runnning too long with no input.


        if (Time.time > BackgroundKeyboardInput.Instance.timeOfLastInput + idleTimeout)
        {
            onBackgroundKeyCombo();
        }
    }

    float timeOfNextAttractAutoCyle = 0;

    void autoCycleGamesIfNoInput()
    {


        if (!ProcessRunner.instance.IsGameRunning())
        {

            if (Time.time > BackgroundKeyboardInput.Instance.timeOfLastInput + idleTimeout)
            {
                //Debug.Log("zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz");
                gameMenu.setAttractMode(true);
                if (Time.time > timeOfNextAttractAutoCyle)
                {
                    gameMenu.onCycleButtonPressed(1, false);
                    timeOfNextAttractAutoCyle = Time.time + attractAutoCycleDuration;
                }

            }

            /*if (Time.time > BackgroundKeyboardInput.Instance.timeOfLastInput + idleTimeout + 10)
            {
                Debug.Log("Refresh");
               UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }*/



            
        }

    }

    /*void OnGUI()
    {
        AlexUtil.DrawText(Vector2.one * 75, BackgroundKeyboardInput.Instance.lastKeyHit + ", " + BackgroundKeyboardInput.Instance.timeOfLastInput, 24, Color.magenta, null);
        if (Time.time > BackgroundKeyboardInput.Instance.timeOfLastInput + idleTimeout)
        {
            AlexUtil.DrawText(Vector2.one * 10, "" + (timeOfNextAttractAutoCyle - Time.time), 24, Color.magenta, null);
        }
    }*/

    bool _lastActionWasQuit = true;
    bool lastActionWasQuit()
    {
        return _lastActionWasQuit;
    }

    public void onBackgroundKeyCombo()
    {
        _lastActionWasQuit = true;
        timeOfLastQuit = Time.time;
        gotAnExitCombo = true;
        
        ProcessRunner.instance.quitCurrentGame();
        this.gameMenu.onQuitGame();

        //if (!AttractMode.Instance.running)
        //{
            gameMenu.setAttractMode(true);
        //}
    }

    public void onApplicationFocus(bool focus)
    {

    }
}
