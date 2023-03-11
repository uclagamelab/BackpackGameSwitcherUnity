/*

Responsible for a fullscreen background image/video 

 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class BackgroundDisplay : MonoBehaviour {

    [SerializeField]
    CameraBlurrer bgBlurrer;

    [SerializeField] Texture _placeHolderImg;

    string _placeHolderVideoUrl;
    string placeHolderVideoUrl
    {
        get
        {
            if (_placeHolderVideoUrl == null)
            {
                _placeHolderVideoUrl = System.IO.Path.Combine(Application.streamingAssetsPath, "placeholder_video.mp4");
                _placeHolderVideoUrl = System.IO.File.Exists(_placeHolderVideoUrl) ? _placeHolderVideoUrl : "";
            }
            return _placeHolderVideoUrl;
        }
    }
 


    GamePreviewDisplayTexture videoPlayer1;
    GamePreviewDisplayTexture videoPlayer2;


    static BackgroundDisplay _instance;


    GamePreviewDisplayTexture prevThingToShow = null;
    GamePreviewDisplayTexture thingToShow = null;

    [SerializeField]
    RawImage previewTex1;


    [SerializeField]
    RawImage previewTex2;


    GamePreviewDisplayTexture[] _allFadeables;

    //Showable video;
    //Showable image;

    public static BackgroundDisplay Instance
    {
        get
        {
            return _instance;
        }
    }
    float blurAmt = 0;
    // Use this for initialization
    void Awake ()
    {
        _instance = this;


        this.videoPlayer1 = new GamePreviewDisplayTexture(previewTex1);
        this.videoPlayer2 = new GamePreviewDisplayTexture(previewTex2);


        _allFadeables = new GamePreviewDisplayTexture[] { videoPlayer1, videoPlayer2};

        //zero out all fadeables
        //so an irrelevant one doesn't block an active one
        foreach (GamePreviewDisplayTexture f in _allFadeables)
        {
            f.alpha = 0;
        }
    }

    void Start()
    {
        setDisplayedGame(MenuVisualsGeneric.Instance.currentlySelectedGame, 1);
    }

    void blurUpdate()
    {
        if (bgBlurrer == null)
        {
            return;
        }

        float targetBlurAmt = MenuVisualsGeneric.Instance.state == MenuVisualsGeneric.MenuState.ChooseGame ? 0 : 1;

        if (targetBlurAmt > 0 && !bgBlurrer.enabled)
        {
            bgBlurrer.enabled = true;
        }
        else if (bgBlurrer.blurAmount == 0 && bgBlurrer.enabled)
        {
            bgBlurrer.enabled = false;
        }

        blurAmt = Mathf.MoveTowards(blurAmt, targetBlurAmt, 3 * Time.deltaTime);
        bgBlurrer.blurAmount = blurAmt;
    }

	// Update is called once per frame
	void Update ()
    {
        blurUpdate();


        bool thingToShowIsAVideo = !string.IsNullOrEmpty(thingToShow._videoPlayer.url);

        float targetAlpha = 0;
        if (!thingToShowIsAVideo)
        {
            if (thingToShow != null)
            {
                targetAlpha = 1;
            }
        }
        else //thing to show is a video
        {
            
            //wait for the video player to be ready?
            GamePreviewDisplayTexture vidToShow = ((GamePreviewDisplayTexture)thingToShow);

            if (vidToShow._videoPlayer.isPrepared)//&& !vidToShow.isSeeking)
            {
                targetAlpha = 1;
                /*if (vidToShow.alpha == 0)
                {
                    vidToShow.videoPlayer.time = (double)Random.Range(0, 100);
                    
                }*/
            }
      


                if (!vidToShow._videoPlayer.isPlaying && vidToShow._videoPlayer.isPrepared)// && vidToShow.transform.GetSiblingIndex() != vidToShow.transform.parent.childCount -1)
                {
                    vidToShow._videoPlayer.transform.SetAsLastSibling();
                }
    
            
       
            

                //
        }

        thingToShow.alpha = Mathf.MoveTowards(thingToShow.alpha, targetAlpha, Time.deltaTime / .65f);

        foreach (GamePreviewDisplayTexture f in _allFadeables)
            {

                if (f == thingToShow)
                {
                //f.alpha = 1;
                }
                else if (f != prevThingToShow)
                {
                    //f.alpha = 0;
                } else if (f == prevThingToShow )
                {
                   // f.alpha = 1;// - thingToShow.alpha;
                }


        }
        
    
        //videoPlayer1.alpha = thingToShow == videoPlayer1 ? 1 : 0;
        //image1.alpha = thingToShow == image1 ? 1 : 0;
	}


    public void setDisplayedGame(GameData game, int direction)
    {
        GamePreviewDisplayTexture targVideoPlayer = thingToShow == videoPlayer1 ? videoPlayer2 : videoPlayer1;

        var outgoingVideo = thingToShow;

        targVideoPlayer.alpha = 0;

        targVideoPlayer.setVideo(game, placeHolderVideoUrl);

        prevThingToShow = thingToShow;
        thingToShow = targVideoPlayer;

        animateChangedObject(direction);
    }
    public bool animating = false;

    void animateChangedObject(int direction)
    {
        if (animating)
        {
            return;
        }

        animating = true;
        this.varyWithT((rawT) =>
        {
            float t = EasingFunctions.Calc(rawT, EasingFunctions.BackEaseOut);
            var h = (thingToShow.transform as RectTransform).sizeDelta.y;
            if (prevThingToShow != null)
            {
                prevThingToShow.gameObject.GetComponent<RawImageFitter>().offset = 
                new Vector2(0, direction * Mathf.Clamp01(t) * h);
               
            }

            thingToShow.gameObject.GetComponent<RawImageFitter>().offset = 
                //new Vector2(direction * (t - 1) * Screen.width, 0);
                new Vector2(0, direction * (t - 1) * h);
            
            if (t == 1)
            {
                animating = false;

                //this.varyWithT((j) => { thingToShow.alpha = j; }, .2f);
                if (prevThingToShow != null)
                {
                    prevThingToShow.alpha = 0;
            }
            }
        }, 0.25f);
    }
}
