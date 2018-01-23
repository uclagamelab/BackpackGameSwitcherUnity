/*

Responsible for a fullscreen background image/video 

 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class BackgroundDisplay : MonoBehaviour {

    interface Fadeable
    {
        float alpha
        {
            get;
            set;
        }
    }

    public class FadeableVideo : Fadeable
    {
        public VideoPlayer videoPlayer;
        public RawImage img;
        
        public FadeableVideo(VideoPlayer videoPlayer)
        {
            this.videoPlayer = videoPlayer;
            this.img = videoPlayer.GetComponent<RawImage>();
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

                if (alpha == 0)
                {
                    this.videoPlayer.Stop();
                    //this.videoPlayer.url = "";
                    //this.videoPlayer.clip = null;
                }
            }
        }
    }

    public class FadeableVideoV1 : Fadeable
    {
        public VideoPlayer videoPlayer;
        public FadeableVideoV1(VideoPlayer videoPlayer)
        {
            this.videoPlayer = videoPlayer;
        }

        public float alpha
        {
            get
            {
                return videoPlayer.targetCameraAlpha;
            }

            set
            {
                videoPlayer.targetCameraAlpha = value;

                if (alpha == 0)
                {
                    this.videoPlayer.Stop();
                    //this.videoPlayer.url = "";
                    //this.videoPlayer.clip = null;
                }
            }
        }
    }

    public class FadeableRawImage : Fadeable
    {
        public RawImage image;
        public FadeableRawImage(RawImage image)
        {
            this.image = image;
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
	
	// Update is called once per frame
	void Update ()
    {
        Fadeable[] allThings = {videoPlayer1, videoPlayer2, image1, image2};
        /*System.Array.Sort<Fadeable>(allThings, (Fadeable a, Fadeable b) => {
            return a == thingToShow ? 0 : 1;
        });*/
   
            foreach (Fadeable f in allThings)
            {
                if (f == thingToShow)
                {
                    f.alpha = Mathf.MoveTowards(f.alpha, 1, Time.deltaTime * 2);
                    if (f.alpha == 1)
                    {
                        prevThingToShow = null;
                    }
                }
                else if (f != prevThingToShow)
                {
                    f.alpha = 0;
                }

                if (prevThingToShow != null)
                {
                prevThingToShow.alpha = 1;// - thingToShow.alpha;
                }
            }
        
    
        //videoPlayer1.alpha = thingToShow == videoPlayer1 ? 1 : 0;
        //image1.alpha = thingToShow == image1 ? 1 : 0;
	}

    public void setVideo(string videoUrl)
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

 


        targVideoPlayer.videoPlayer.url = videoUrl;
        targVideoPlayer.videoPlayer.Play();
        //targVideoPlayer.videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;

        prevThingToShow = thingToShow;
        thingToShow = targVideoPlayer;

        targVideoPlayer.img.transform.SetAsLastSibling();

        /*if (outgoingVideo != null)
        {
            outgoingVideo.videoPlayer.renderMode = VideoRenderMode.CameraFarPlane;
        }*/
    }

    public void setImage(Texture img)
    {
        FadeableRawImage targImg = thingToShow == image1 ? image2 : image1;
        targImg.image.texture = img;

        prevThingToShow = thingToShow;
        thingToShow = targImg;
        targImg.image.transform.SetAsLastSibling();


    }

    public void hide()
    {
        thingToShow = null;
    }
}
