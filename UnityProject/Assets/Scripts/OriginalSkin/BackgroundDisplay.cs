/*

Responsible for a fullscreen background image/video 

 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityStandardAssets.ImageEffects;

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

        /*
        
                    _rawImg.material.SetFloat("_BlurAmt", Mathf.InverseLerp(1, 0.9f, t));
            _rawImg.material.SetFloat("_MipsBias", 6*Mathf.InverseLerp(1, 0, t));
            _rawImg.material.SetFloat("_BlurDist", 12 * (1-t));
         
         */
        blurAmt = Mathf.MoveTowards(blurAmt, targetBlurAmt, 6 * Time.deltaTime);
        bgBlurrer.blurAmount = Mathf.InverseLerp(0, .1f, blurAmt);
        bgBlurrer.blurSize = blurAmt * 4;

    }

	// Update is called once per frame
	void Update ()
    {

        /*System.Array.Sort<Fadeable>(allThings, (Fadeable a, Fadeable b) => {
            return a == thingToShow ? 0 : 1;
        });*/

        blurUpdate();


        bool thingToShowIsAVideo = thingToShow == videoPlayer1 || thingToShow == videoPlayer2;

        if (!thingToShowIsAVideo)
        {
            if (thingToShow != null)
            {
                thingToShow.alpha = Mathf.MoveTowards(thingToShow.alpha, 1, Time.deltaTime / .65f);
            }
        }
        else //thing to show is a video
        {
            
            //wait for the video player to be ready?
            GamePreviewDisplayTexture vidToShow = ((GamePreviewDisplayTexture)thingToShow);

            float targetAlpha = 0;
            if (vidToShow._videoPlayer.isPrepared)//&& !vidToShow.isSeeking)
            {
                targetAlpha = 1;
                /*if (vidToShow.alpha == 0)
                {
                    vidToShow.videoPlayer.time = (double)Random.Range(0, 100);
                    
                }*/
            }
            vidToShow.alpha = Mathf.MoveTowards(vidToShow.alpha, targetAlpha, Time.deltaTime / .65f);


                if (!vidToShow._videoPlayer.isPlaying && vidToShow._videoPlayer.isPrepared)// && vidToShow.transform.GetSiblingIndex() != vidToShow.transform.parent.childCount -1)
                {
                    vidToShow._videoPlayer.transform.SetAsLastSibling();
                }
    
            
       
            

                //
        }

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

        if (!string.IsNullOrEmpty(game?.videoUrl) || game.previewImg != null)
        {
            setVideo(game?.videoUrl, null, direction);
        }
        else if (game?.previewImg != null)
        {
            setVideo(null, game.previewImg, direction);
        }
        else
        {
            setPlaceholder(direction);
        }
    }

    void setPlaceholder(int direction)
    {
        if (!string.IsNullOrEmpty(placeHolderVideoUrl))
        {
            setVideo(placeHolderVideoUrl, null, direction);
        }
        else
        {
            setVideo(null, _placeHolderImg, direction);
        }
    }

    void setVideo(string videoUrl, Texture texture, int direction = 0)
    {

        GamePreviewDisplayTexture targVideoPlayer = thingToShow == videoPlayer1 ? videoPlayer2 : videoPlayer1;

        var outgoingVideo = thingToShow;


        targVideoPlayer.alpha = 0;
        if (!string.IsNullOrEmpty(videoUrl))
        {
            targVideoPlayer.setVideo(videoUrl);
        }
        else
        {
            targVideoPlayer.setVideo(texture);
        }


        //Debug.Log(targVideoPlayer.videoPlayer.isPrepared);

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
            float t = EasingFunctions.Calc(rawT, EasingFunctions.QuadEaseIn);
            if (prevThingToShow != null)
            {
                prevThingToShow.gameObject.GetComponent<RawImageFitter>().offset = new Vector2(direction * t * Screen.width, 0);
               
            }

            //thingToShow.alpha = 0;// Mathf.InverseLerp(.25f, 1, t);
            //thingToShow.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, thingToShow.alpha);//.withY(1);
            thingToShow.gameObject.GetComponent<RawImageFitter>().offset = new Vector2(direction * (t - 1) * Screen.width, 0);
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

    public void hide()
    {
        thingToShow = null;
    }
}
