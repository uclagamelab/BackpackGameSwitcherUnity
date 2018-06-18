/*
 
Collects input from gamepads, keyboard etc... and controls the menu
 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitcherApplicationController : MonoBehaviour, BackgroundKeyboardInput.Listener {
    MenuVisualsGeneric gameMenu;

    bool gotAnExitCombo = false; //consumable event

    float attractTimeOut = 60;

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


    // Use this for initialization
    void Awake () {
        gameMenu = this.GetComponent<MenuVisualsGeneric>();
    }

    void Start()
    {
        BackgroundKeyboardInput.Instance.addListener(this);
    }

    // Update is called once per frame
    void Update()
    {

        checkIfIdleAndReturnToAttractUpdate();
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

            gotAnExitCombo = false;
        }

        gameProcessWentNullOrExitedLastUpdate = processWentNullOrExitedThisUpdate;
        //------------------------------------------------------------------



        //Make Switcher Retake focus, if necessary
        if (Time.time > lastFocusSwitchAttemptTime + .5f)
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
                ProcessRunner.instance.BringRunningToForeground(); //this function should be robust to repeated calls
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

    void AttractUpdate()
    {
        if (Input.anyKeyDown && didntQuitRecently && !generatedSimulatedKeypressForFocusSwitchToSwitcherApp)
        {
            gameMenu.setAttractMode(false);
        }

        //consume the event
        generatedSimulatedKeypressForFocusSwitchToSwitcherApp = false;
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
        // quit out of a game, if runnning too long with now input.

        if (Time.time > BackgroundKeyboardInput.Instance.timeOfLastInput + 60)
        {
            onBackgroundKeyCombo();
        }
    }
    
    void checkIfIdleAndReturnToAttractUpdate()
    {


        if (!ProcessRunner.instance.IsGameRunning())
        {
         
            if (Time.time > BackgroundKeyboardInput.Instance.timeOfLastInput + attractTimeOut)
            {
                gameMenu.setAttractMode(true);
            }
        }

    }

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
