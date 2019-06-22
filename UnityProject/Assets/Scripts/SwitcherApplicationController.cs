/*
 
Collects input from gamepads, keyboard etc... and controls the menu

BAD ADDITIONS : 
- fixes resolution
 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SwitcherApplicationController : MonoBehaviour
{
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


    static float idleTimeout = 6 * 60;
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
        BackgroundKeyboardInput.Events.onBackgroundKeyCombo += onBackgroundKeyCombo;
    }

    //TODO: Move this to process runner
    public void ResetToDefaultResolutionIfDifferent()
    {
        #if !UNITY_EDITOR
        bool resolutionDifferent = Screen.width != _defaultResolution.width || Screen.height != _defaultResolution.height;
        if (!SwitcherSettings.AdminMode && resolutionDifferent)
        {
            Screen.SetResolution(_defaultResolution.width, _defaultResolution.height, _defaultIsFullScreen);
        }
        #endif
    }

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
                //print("game exited with combo!");
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
        if (Time.time > lastFocusSwitchAttemptTime + 3)
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

        //--- LEGACY INPUT STYLE, JUST SLIDING THE BACKGROUND -----------------------
        int selectionDirection = 0;
        selectionDirection = CrockoInput.NoListVersion.GetNextGameDown() ? 1 : CrockoInput.NoListVersion.GetPreviousGameDown() ? -1 : 0;
        if (selectionDirection != 0)
        {
            gameMenu.onCycleButtonPressed(selectionDirection);
        }
        //----------------------------------------------------------------------------


        if (CrockoInput.GetOpenGameButtonDown())
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

    public static System.Action OnAttractCycleNextGame = () => { };

    public static bool isIdle
    {
        get
        {
            bool ret = Time.time > BackgroundKeyboardInput.Instance.timeOfLastInput + idleTimeout;
            return ret;
        }
    }

    void autoCycleGamesIfNoInput()
    {

        if (!ProcessRunner.instance.IsGameRunning())
        {

            if (isIdle)
            {
                //Debug.Log("zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz");
                gameMenu.setAttractMode(true);
                if (Time.time > timeOfNextAttractAutoCyle)
                {
                    //gameMenu.onCycleButtonPressed(1, false);
                    OnAttractCycleNextGame.Invoke();
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
