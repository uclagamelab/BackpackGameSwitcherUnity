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

    public GameObject loadingScreen;
    int gameIdx = 0;

    GameInfoUI gameInfoUI;
    bool animating = false;

    GameCatalog model;

    enum State { CHOOSING, PLAYING_GAME}
    State state = State.CHOOSING;



    float attractTimeOut = 60;
    float timeOfLastInput = float.NegativeInfinity;


	// Use this for initialization
	void Start () {
        gameInfoUI = this.GetComponentInChildren<GameInfoUI>();
        setAttractMode(true);
    }
	
    void setAttractMode(bool attract)
    {
        AttractMode.Instance.running = attract;
        this.gameInfoUI.gameObject.SetActive(!attract);
        BackgroundDisplay.Instance.gameObject.SetActive(!attract);
    }

	// Update is called once per frame
	void Update () {

        bool playingGame = ProcessRunner.instance.gameProcessIsRunning;

        if (Input.anyKeyDown && !playingGame)
        {
            timeOfLastInput = Time.unscaledTime;
        }

        if (AttractMode.Instance.running)
        {
            if (Input.anyKeyDown)
            {
                setAttractMode(false);
            }
            return;
        }

        if (Time.unscaledTime > timeOfLastInput + attractTimeOut)
        {
            setAttractMode(true);
        }


        if (playingGame)
        {
            state = State.PLAYING_GAME;
        }
        else
        {
            state = State.CHOOSING;
        }

		/*if (state == State.PLAYING_GAME)
        {
            if (!this.loadingScreen.activeSelf)
            {
                this.loadingScreen.SetActive(true);
                this.gameInfoUI.gameObject.SetActive(false);
            }
        }
        else
        {
            if (this.loadingScreen.activeSelf)
            {
                this.loadingScreen.SetActive(false);
                this.gameInfoUI.gameObject.SetActive(true);
            }
        }*/
	}

    public GameData currentlySelectedGame
    {
        get
        {
            return GameCatalog.Instance.games[gameIdx];
        }
    }

    public void cycleToNextGame(int selectionDirection)
    {
        if (!animating)
        {
                animating = true;
            
            this.varyWithT((float t) => 
            {
                this.gameInfoUI.GetComponent<CanvasGroup>().alpha = 1 - t;
                if (t == 1)
                {
                    this.gameIdx = (gameIdx + selectionDirection + GameCatalog.Instance.gameCount) % GameCatalog.Instance.gameCount;
                    this.updateInfoDisplay(GameCatalog.Instance.games[this.gameIdx]);
                    this.varyWithT((float t2) =>
                    {
                        this.gameInfoUI.GetComponent<CanvasGroup>().alpha = t2;
                        if (t2 == 1)
                        {
                            animating = false;
                        }
                    }, .45f);
                }
            }, .25f);

        }
      
    }

    public void cycleToNextGameOld(int selectionDirection)
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
                    this.gameIdx = (gameIdx + selectionDirection + GameCatalog.Instance.gameCount) % GameCatalog.Instance.gameCount;
                    //animating = false;

                    updateInfoDisplay(currentlySelectedGame);
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
        gameInfoUI.descriptionText.text = ""


            // + currentGameData.description
            + ""
            + "<i>by " + currentGameData.author + "</i>";

        //gameInfoUI.previewImage.texture = currentGameData.previewImg;
        if (currentGameData.videoUrl != null)
        {
            BackgroundDisplay.Instance.setVideo(currentGameData.videoUrl);
        }
        else
        {
            BackgroundDisplay.Instance.setImage(currentGameData.previewImg);
        }
    }

 
    public bool acceptingInput()
    {
        return !animating;
    }

    public void onStartGameButtonPress()
    {
        //this.state = State.PLAYING_GAME;
        this.varyWithT((float t) => {
            float cale = 1 - .1f * (Mathf.PingPong(2 * t, 1));
            if (t == 1)
            {
                cale = 1;
            }
            this.gameInfoUI.transform.localScale = Vector3.one * cale;
            }, .1f);
    }

    public void onQuitGame()
    {
        //  this.state = State.CHOOSING;
        setAttractMode(true);
    }



}
