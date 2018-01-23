using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitcherApplicationController : MonoBehaviour, BackgroundKeyboardInput.Listener {
    MenuVisualsGeneric gameMenu;

    

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


        int selectionDirection = 0;
        selectionDirection = Input.GetKeyDown(KeyCode.W) ? 1 : Input.GetKeyDown(KeyCode.S) ? -1 : 0;
        if (selectionDirection != 0)
        {
            gameMenu.cycleToNextGame(selectionDirection);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if(!ProcessRunner.instance.IsGameRunning())
            {
                this.gameMenu.onStartGameButtonPress();
                this.delayedFunction(() =>
                {
                    GameData currentGameData = gameMenu.currentlySelectedGame;//this.allGames[gameIdx];
                                                                              //print("________________________" + currentGameData.appFile);
                    ProcessRunner.instance.OpenProcess(@currentGameData.directory, currentGameData.appFile, "-popupwindow -screen-width 1920 -screen-height 1080", currentGameData.joyToKeyConfigFile);
                }, .2f);
            }
        }
    }

    public void onBackgroundKeyCombo()
    {
        Debug.Log("!!!!!!!!!!!!");
        ProcessRunner.instance.quitCurrentGame();
        this.gameMenu.onQuitGame();
    }
}
