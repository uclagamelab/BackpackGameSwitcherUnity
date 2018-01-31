using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitcherApplicationController : MonoBehaviour, BackgroundKeyboardInput.Listener {
    MenuVisualsGeneric gameMenu;

    float attractTimeOut = 60;

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
	void Update ()
    {

        checkIfIdleAndReturnToAttractUpdate();

        bool aGameIsRunning = ProcessRunner.instance.IsGameRunning();

        if (!aGameIsRunning)
        {
            if (AttractMode.Instance.running)
            {
                AttractUpdate();
            }
            else
            {
                selectingGameUpdate();
            }

        }
        else //A Game is running...
        {
            gameRunningUpdate();
        }
    }

    void AttractUpdate()
    {
        if (Input.anyKeyDown && didntQuitRecently)
        {
            gameMenu.setAttractMode(false);
        }
    }

    void selectingGameUpdate()
    {
        bool changedAttractTimeTooRecently = Time.time < AttractMode.Instance.changeTime + .5f;

        if (changedAttractTimeTooRecently)
        {
            return;
        }

        int selectionDirection = 0;
        selectionDirection = Input.GetKeyDown(KeyCode.W) ? 1 : Input.GetKeyDown(KeyCode.S) ? -1 : 0;
        if (selectionDirection != 0)
        {
            gameMenu.cycleToNextGame(selectionDirection);
        }


        if (Input.GetKeyDown(KeyCode.UpArrow))
        {

            this.gameMenu.onStartGameButtonPress();


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

    public void onBackgroundKeyCombo()
    {
        timeOfLastQuit = Time.time;
        Debug.Log("!!!!!!!!!!!!");
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
