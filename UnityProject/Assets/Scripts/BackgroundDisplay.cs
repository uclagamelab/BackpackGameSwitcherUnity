/*

Responsible for a fullscreen background image/video 

 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityStandardAssets.ImageEffects;

public class BackgroundDisplay : MonoBehaviour {

    [SerializeField]
    CameraBlurrer bgBlurrer;

 

    interface Fadeable
    {
        GameObject gameObject
        {
            get;
        }
        float alpha
        {
            get;
            set;
        }

        Transform transform
        {
            get;
        }

    }

    public void stopAllVideos()
    {
        //this.vid1.Stop();
        //this.vid2.Stop();
    }



    public class FadeableVideo : Fadeable
    {
        public VideoPlayer videoPlayer;
        public RawImage img;

        public bool isSeeking = false;

        public float time
        {
            get
            {
                return (float) videoPlayer.time;
            }

            set
            {
                isSeeking = true;
                videoPlayer.time = value;
            }
        }

        public GameObject gameObject
        {
            get
            {
                return img.gameObject;
            }
        }


        public FadeableVideo(VideoPlayer videoPlayer)
        {
            this.videoPlayer = videoPlayer;
            this.img = videoPlayer.GetComponent<RawImage>();
            videoPlayer.seekCompleted += seekFinished;
            //Debug.Log("SSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSS");

        }

        void seekFinished(VideoPlayer vp)
        {
           // Debug.Log("SSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSS");
            isSeeking = false;
        }

        public Transform transform
        {
            get { return img.transform; }
        }

        public float alpha
        {
            get
            {
                return img.color.a;
            }

            set
            {
                Color newColor = img.color;
                newColor.a = value;
                img.color = newColor;
            }
        }


    }


    public class FadeableRawImage : Fadeable
    {
        public GameObject gameObject
            {
            get
            {
            return image.gameObject;
            }
        }
        public RawImage image;
        public FadeableRawImage(RawImage image)
        {
            this.image = image;
        }

        public Transform transform
        {
            get { return image.transform; }
        }


        public float alpha
        {
            get
            {
                return image.color.a;
            }

            set
            {
                Color c = image.color;
                c.a = value;
                image.color = c;
            }
        }
    }



    FadeableVideo videoPlayer1;
    FadeableVideo videoPlayer2;

    FadeableRawImage image1;
    FadeableRawImage image2;

    static BackgroundDisplay _instance;


    Fadeable prevThingToShow = null;
    Fadeable thingToShow = null;

    [SerializeField]
    RawImage img1;
    [SerializeField]
    RawImage img2;

    [SerializeField]
    VideoPlayer vid1;

    [SerializeField]
    VideoPlayer vid2;



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

        //Two of each for cross fading

        //VideoPlayer[] vids = this.GetComponentsInChildren<VideoPlayer>();
        //RawImage[] imgs = this.GetComponentsInChildren<RawImage>();



        this.image1 = new FadeableRawImage(img1);
        this.image2 = new FadeableRawImage(img2);

        this.videoPlayer1 = new FadeableVideo(vid1);
        this.videoPlayer2 = new FadeableVideo(vid2);
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
        Fadeable[] allThings = {videoPlayer1, videoPlayer2, image1, image2};
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
            FadeableVideo vidToShow = ((FadeableVideo)thingToShow);

            float targetAlpha = 0;
            if (vidToShow.videoPlayer.isPrepared)//&& !vidToShow.isSeeking)
            {
                targetAlpha = 1;
                /*if (vidToShow.alpha == 0)
                {
                    vidToShow.videoPlayer.time = (double)Random.Range(0, 100);
                    
                }*/
            }
            vidToShow.alpha = Mathf.MoveTowards(vidToShow.alpha, targetAlpha, Time.deltaTime / .65f);


                if (!vidToShow.videoPlayer.isPlaying && vidToShow.videoPlayer.isPrepared)// && vidToShow.transform.GetSiblingIndex() != vidToShow.transform.parent.childCount -1)
                {
                //vidToShow.alpha = 0;
                
                vidToShow.videoPlayer.transform.SetAsLastSibling();


                    vidToShow.videoPlayer.Play();

                    
                }
    
            
       
            

                //
        }

        foreach (Fadeable f in allThings)
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

    public bool visible
    {
        get
        {
            return this.image1.image.enabled ||  this.image2.image.enabled  || this.videoPlayer1.img.enabled || this.videoPlayer2.img.enabled;
        }

        set
        {
            this.image1.image.enabled = value;
            this.image2.image.enabled = value;
            this.videoPlayer1.img.enabled = value;
            this.videoPlayer2.img.enabled = value;

        }
    }

    public void setImage(Texture img, int direction)
    {

        Fadeable outgoingThing = null;
        if (thingToShow == videoPlayer1)
        {
            outgoingThing = videoPlayer1;
        }
        else if (thingToShow == videoPlayer2)
        {
            outgoingThing = videoPlayer2;
        }
        else if (thingToShow == image1)
        {
            outgoingThing = image1;
        }
        else if (thingToShow == image2)
        {
            outgoingThing = image2;
        }

        FadeableRawImage targImg = thingToShow == image1 ? image2 : image1;
        targImg.image.texture = img;


        targImg.image.transform.SetAsFirstSibling();

       

        prevThingToShow = thingToShow;
        thingToShow = targImg;

        animateChangedObject(direction);
    }


    bool needToSeek = false;
    public void setVideo(string videoUrl, int direction = 0)
    {

        FadeableVideo targVideoPlayer = thingToShow == videoPlayer1 ? videoPlayer2 : videoPlayer1;

        FadeableVideo outgoingVideo = null;
        if (thingToShow == videoPlayer1)
        {
            outgoingVideo = videoPlayer1;
        }
        else if (thingToShow == videoPlayer2)
        {
            outgoingVideo = videoPlayer2;
        }

        targVideoPlayer.alpha = 0;
        if (targVideoPlayer.videoPlayer.url != videoUrl)
        {
            targVideoPlayer.videoPlayer.Stop();
            
            targVideoPlayer.videoPlayer.url = videoUrl;
            //





        }



        //
        //targVideoPlayer.videoPlayer.time = (double)Random.Range(0, 100);
        targVideoPlayer.videoPlayer.
            //Play();//
            Prepare();
        //needToSeek = true;



        //targVideoPlayer.videoPlayer.Pause();

        RenderTexture savedRt = RenderTexture.active;
        RenderTexture.active = targVideoPlayer.videoPlayer.targetTexture;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = savedRt;


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
