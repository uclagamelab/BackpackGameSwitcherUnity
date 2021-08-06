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
    bool gotAnExitCombo = false; //consumable event

    //float attractTimeOut = 120;

    bool gameProcessWentNullOrExitedLastUpdate = true;

    float lastFocusSwitchAttemptTime = float.NegativeInfinity;

    bool _defaultIsFullScreen = true;
    Resolution _defaultResolution;

    float timeOfLastQuit = float.NegativeInfinity;
    bool didntQuitRecently
    {
        get
        {
            return Time.time > timeOfLastQuit + .5f;
        }
    }


    static float idleTimeout = 6 * 60;



    public class Events
    {
        public System.Action OnAppQuittedOnItsOwn = () => { };
        public System.Action OnEnterIdle = () => { };
    }
    public static Events events = new Events();


    void Awake () {
        _defaultResolution = Screen.currentResolution;
        _defaultIsFullScreen = Screen.fullScreen;
    }

    void Start()
    {
        BackgroundKeyboardInput.Events.onBackgroundKeyCombo += onRequestQuit;
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

        bool aGameIsRunning = ProcessRunner.instance.IsGameRunning();
        //--------------------------------------------------------------------
        bool processWentNullOrExitedThisUpdate = !aGameIsRunning;
        if (processWentNullOrExitedThisUpdate && !gameProcessWentNullOrExitedLastUpdate)
        {
            if (gotAnExitCombo)
            {
                #if UNITY_EDITOR
                print("game exited with combo!");
                #endif
            }
            else
            {
                #if UNITY_EDITOR
                print("game exited some other way");
                #endif

                //if the program quit for some reason other than the key-combo, it might have extra
                //companion processes that still need to be cleaned up
                //probably ok to call redunantly, even in key combo case
                ProcessRunner.instance.KillAllNonSafeProcesses();
                  
            }

            events.OnAppQuittedOnItsOwn.Invoke();
            this.delayedFunction(ResetToDefaultResolutionIfDifferent, .25f);
            gotAnExitCombo = false;
        }

        gameProcessWentNullOrExitedLastUpdate = processWentNullOrExitedThisUpdate;

        //Make Switcher Retake focus, if necessary
        if (Time.time > lastFocusSwitchAttemptTime + 3)
        {
            lastFocusSwitchAttemptTime = Time.time;

            //BAD QUIRK: only take back control once the quit button is pressed
            //Mostly ok, is hacky fix to issue that checking whether a process exists/hasn't exited
            //is not a good way to determine if a game is running, or starting to run.
            //TODO: More robustly detect if a game is running or not (detect a non-key-combo quit)
        
            if (ProcessRunner.instance.IsGameRunning()) // have the game try to retake focus, if one is running.
            {
                ProcessRunner.instance.BringRunningToForeground(); //this function should be robust to repeated calls   
            }
            else//if (_lastActionWasQuit) //make the switcher retake focus.. if the user is trying to quit.
            {
                //print("bring switcher to the frongt!");
                ProcessRunner.instance.BringThisToForeground();

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

  


        if (CrockoInput.GetOpenGameButtonDown())
        {
            _lastActionWasQuit = false;
        }
    }

    
    void gameRunningUpdate()
    {
        // quit out of a game, if runnning too long with no input.


        if (Time.time > BackgroundKeyboardInput.Instance.timeOfLastInput + idleTimeout)
        {
            onRequestQuit();
            events.OnEnterIdle.Invoke();
        }
    }




    public static bool isIdle
    {
        get
        {
            bool ret = Time.time > BackgroundKeyboardInput.Instance.timeOfLastInput + idleTimeout;
            return ret;
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


    public void onRequestQuit()
    {
        _lastActionWasQuit = true;
        timeOfLastQuit = Time.time;
        gotAnExitCombo = true;
        
        ProcessRunner.instance.quitCurrentGame();
    }

}
