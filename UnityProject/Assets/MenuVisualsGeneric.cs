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

public class MenuVisualsGeneric : MonoBehaviour
{

    public int gameIdx = 0;

    GameInfoUI gameInfoUI;
    bool animating = false;

    FindGames model;

	// Use this for initialization
	void Start () {
        gameInfoUI = this.GetComponentInChildren<GameInfoUI>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public GameData currentlySelectedGame
    {
        get
        {
            return FindGames.Instance.games[gameIdx];
        }
    }

    public void cycleToNextGame(int selectionDirection)
    {
        if (!animating)
        {
                animating = true;

                bool flipped = false;
                this.varyWithT((float rt) =>
                {
                    float t = EasingFunctions.Calc(rt, EasingFunctions.QuadEaseInOut);

                    //gameInfoUI.GetComponent<CanvasGroup>().alpha = 1 - Mathf.PingPong(2 * t, 1);
                    gameInfoUI.transform.localScale = new Vector3(
                        1 - Mathf.PingPong(2 * t, 1),
                        1 - Mathf.PingPong(2 * t, 1),
                        1);

                    float t2 = rt;//Mathf.PingPong(2*rt, 1);//

                    gameInfoUI.transform.eulerAngles = Vector3.forward * 1080 * t2;//;

                    if (t >= .5f && !flipped)
                    {


                        flipped = true;
                        updateInfoDisplay(currentlySelectedGame);
                        this.gameIdx = (gameIdx + selectionDirection + FindGames.Instance.gameCount) % FindGames.Instance.gameCount;
                        //animating = false;
                    }

                    if (rt == 1)
                    {
                        animating = false;
                    }

                }, 0.75f);
            
            
        }
      
    }

    void updateInfoDisplay(GameData currentGameData)
    {

        //TODO : should move into GameInfoUi
        gameInfoUI.titleText.text = currentGameData.title;
        gameInfoUI.descriptionText.text =
            "Made By: " + currentGameData.author
            + "\n\n"
            + currentGameData.description;

        gameInfoUI.previewImage.texture = currentGameData.previewImg;
    }

 
    public bool acceptingInput()
    {
        return !animating;
    }

}
