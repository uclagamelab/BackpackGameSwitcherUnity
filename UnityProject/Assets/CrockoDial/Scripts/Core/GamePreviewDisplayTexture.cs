using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class GamePreviewDisplayTexture 
{
    public VideoPlayer _videoPlayer;
    public RawImage _rawImgOuput;
    RenderTexture _videoRenderTexture;
    public AudioSource _audioOuput;
    GameData _viewedGameData;
 


    public bool isSeeking = false;

    public float time
    {
        get
        {
            return (float)_videoPlayer.time;
        }

        set
        {
            isSeeking = true;
            _videoPlayer.time = value;
        }
    }

    public GameObject gameObject
    {
        get
        {
            return _rawImgOuput.gameObject;
        }
    }

    public GamePreviewDisplayTexture(RawImage rawImgDisplayer)
    {
        RenderTextureDescriptor rtd = new RenderTextureDescriptor(1920, 1080);
        rtd.depthBufferBits = 0;
        //rtd.autoGenerateMips = false;
        rtd.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.B8G8R8A8_UNorm;
        rtd.colorFormat = RenderTextureFormat.BGRA32;
        _videoRenderTexture = new RenderTexture(rtd);

        _rawImgOuput = rawImgDisplayer;
        _videoPlayer = rawImgDisplayer.gameObject.AddComponent<VideoPlayer>();
        _videoPlayer.source = VideoSource.Url;
        _videoPlayer.playOnAwake = false;
        _videoPlayer.waitForFirstFrame = true;
        _videoPlayer.isLooping = true;
        _videoPlayer.skipOnDrop = true;
        _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        _videoPlayer.aspectRatio = VideoAspectRatio.FitOutside;

        _videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        _audioOuput = BgMusicPlayer.instance.gameObject.AddComponent<AudioSource>(); //<- a little hacky, but so the low pass filter effect gets applied as it is for the bg music.
        _audioOuput.volume = 0;
        _audioOuput.outputAudioMixerGroup = BgMusicPlayer.instance.bgmGroup;
        _videoPlayer.SetTargetAudioSource(0, _audioOuput);

        this._videoPlayer.targetTexture = _videoRenderTexture;
        this._rawImgOuput.texture = _videoRenderTexture;
        _videoPlayer.seekCompleted += seekFinished;
        _videoPlayer.prepareCompleted += prepareFinished;
        BgMusicPlayer.instance.AddBGMVolumeOverrider(bgMusicForcer);
    }

    private void bgMusicForcer(ref BgMusicPlayer.ForceMode mode, ref int priority)
    {
        if (_viewedGameData != null && _viewedGameData.previewVideoHasAudio && this.alpha > .5f && SwitcherSettings.Data._PreviewVideoVolume > 0)
        {
            mode = BgMusicPlayer.ForceMode.WantOff;
            priority = 100;
        }
    }

    void seekFinished(VideoPlayer vp)
    {
        isSeeking = false;
    }

    void prepareFinished(VideoPlayer vp)
    {
    }

    public Transform transform
    {
        get { return _rawImgOuput.transform; }
    }

    public float alpha
    {
        get
        {
            return _rawImgOuput.color.a;
        }

        set
        {
            Color newColor = _rawImgOuput.color;
            newColor.a = value;
            _rawImgOuput.color = newColor;
            _audioOuput.volume = 
                _viewedGameData == null || !_viewedGameData.previewVideoHasAudio ?  0 : 
                value * SwitcherSettings.Data._PreviewVideoVolume;
        }
    }

    public void setVideo(GameData game, string fallbackVideoUrl = null, Texture fallbackPreviewTexture = null)
    {
        _viewedGameData = game;
        if (!string.IsNullOrEmpty(game?.videoUrl))
        {
            setVideo(game.videoUrl);
        }
        else if (game?.previewImg != null)
        {
            setVideo(game.previewImg);
        }
        else if (!string.IsNullOrEmpty(fallbackVideoUrl))
        {
            setVideo(fallbackVideoUrl);
        }
        else
        {
            setVideo(fallbackPreviewTexture);
        }
    }

    public void setVideo(Texture staticImg)
    {
        _rawImgOuput.texture = staticImg;
    }


    public  void setVideo(string videoUrl)
    {
        if (_rawImgOuput.texture != _videoRenderTexture)
        {
            _rawImgOuput.texture = _videoRenderTexture;
        }

        if (_videoPlayer.url != videoUrl)
        {
            _videoPlayer.Stop();

            _videoPlayer.url = videoUrl;
        }

        //
        //targVideoPlayer.videoPlayer.time = (double)Random.Range(0, 100);
        _videoPlayer.
            //Play();//
            Prepare();
        //needToSeek = true;



        //targVideoPlayer.videoPlayer.Pause();

        RenderTexture savedRt = RenderTexture.active;
        RenderTexture.active = _videoPlayer.targetTexture;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = savedRt;

        _videoPlayer.Play();
    }
}