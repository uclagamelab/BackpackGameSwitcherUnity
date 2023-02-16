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
    const float FOCUS_RETAKE_DELAY = 3;
    bool gotAnExitCombo = false; //consumable event

    //float attractTimeOut = 120;

    bool gameProcessWentNullOrExitedLastUpdate = true;

    float lastFocusSwitchAttemptTimer = float.NegativeInfinity;

    bool _defaultIsFullScreen = true;
    Resolution _defaultResolution;



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
        Application.targetFrameRate = 60;
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
        if (lastFocusSwitchAttemptTimer > 0)
        {
            lastFocusSwitchAttemptTimer -= Time.deltaTime;
        }
        else
        {
            lastFocusSwitchAttemptTimer = FOCUS_RETAKE_DELAY;

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



        if (aGameIsRunning)
        {
            if (BackgroundKeyboardInput.Instance.secondsSinceLastInput > idleTimeout)
            {
                onRequestQuit();
                events.OnEnterIdle.Invoke();
            }
        }
    }

  

    public static bool isIdle
    {
        get
        {
            bool ret = BackgroundKeyboardInput.Instance.secondsSinceLastInput > idleTimeout;
            return ret;
        }
    }

    public void onRequestQuit()
    {
        gotAnExitCombo = true;
        
        ProcessRunner.instance.quitCurrentGame();
    }

}
