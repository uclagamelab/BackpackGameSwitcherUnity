using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class VaSwitcherSound : MonoBehaviour, MenuVisualsGeneric.Listener {

    float volumeScale = 1.25f;

    public MenuVisualsGeneric menu;
    public AudioClip cycleSound;
    public AudioClip cycleSound2;
    public AudioClip cycleFinishSound;
    public AudioClip openInfoSound;
    public AudioClip closeInfoSound;
    public AudioClip startGame;
    public AudioClip quitGame;
    AudioClip backGroundMusic;
    AudioLowPassFilter musicFilter;

    public AudioClip infoMenuCursorMove1;

    public AudioSource bgMusicSource;

    AudioSource[] oneShotPool;


    bool hasFocus = true;
    float MaxMusicVolume = .45f;

    void Start () {
        //bgMusicSource = this.gameObject.AddComponent<AudioSource>();
        oneShotPool = new AudioSource[3];
        for (int i = 0; i < oneShotPool.Length; i++)
        {
            oneShotPool[i] = this.gameObject.AddComponent<AudioSource>();
            oneShotPool[i].playOnAwake = false;
        }
        menu.addListener(this);

        bgMusicSource.clip = backGroundMusic;
        bgMusicSource.loop = true;
        bgMusicSource.volume = 0;

        StartCoroutine(LoadCustomMusic());

        menu.OnOpenCloseInfo+= onOpenCloseInfo;

        menu.InfoMenuCursorMove += OnInfoMenuCursorMove;

        musicFilter = bgMusicSource.GetComponent<AudioLowPassFilter>();
    }

    void OnInfoMenuCursorMove(bool open)
    {
        PlayOneShot(infoMenuCursorMove1, .1f, 1);
    }

        void onOpenCloseInfo(bool open)
    {
        if (open)
        {
            PlayOneShot(openInfoSound, .20f, 1);
        }
        else
        {
            PlayOneShot(closeInfoSound, .25f, 1);
        }
    }

    IEnumerator LoadCustomMusic()
    {
        string musicLocation = Application.streamingAssetsPath + "/BGMusic";
        FileInfo fileToUse = null;
        foreach (string path in Directory.GetFiles(musicLocation))
        {
            string[] okExtensions = {"ogg", "wav"};
            FileInfo maybeFile = new FileInfo(path);
            foreach (string ext in okExtensions)
            {
                if (maybeFile.Extension.ToLower().EndsWith(ext))
                {
                    Debug.Log("" + maybeFile);
                    fileToUse = maybeFile;
                }
            }
        }

        if (fileToUse != null)
        {
            WWW audRequest = new WWW(fileToUse.FullName);
            yield return audRequest;
            this.backGroundMusic = audRequest.GetAudioClip();// false, false);
            this.bgMusicSource.clip = this.backGroundMusic;
        }
    }
	
	void Update () {
        float targMusicVolume = 0;
        if (menu.state != MenuVisualsGeneric.MenuState.LaunchGame && hasFocus)
        {
            targMusicVolume = 1;
        }

        this.bgMusicSource.volume = Mathf.MoveTowards(this.bgMusicSource.volume, targMusicVolume * volumeScale * MaxMusicVolume, .5f * Time.deltaTime);
        bool shouldPlay = bgMusicSource.volume != 0;
        if (bgMusicSource.isPlaying != shouldPlay)
        {
            if (shouldPlay)
            {
                bgMusicSource.Play();
            }
            else
            {
                bgMusicSource.Pause();
            }
        }

        float targetCutoff = 22000;
        if (PreLaunchGameInfo.Instance.open || (PreLaunchGameInfo.Instance.animating && !PreLaunchGameInfo.Instance.open))
        {
            targetCutoff = 850;
        }

        float maxDiff = 22000 - 850;
        this.musicFilter.cutoffFrequency = Mathf.Lerp(this.musicFilter.cutoffFrequency, targetCutoff, 8 * Time.deltaTime);// 2*maxDiff * Time.deltaTime);
        this.musicFilter.cutoffFrequency = Mathf.MoveTowards(this.musicFilter.cutoffFrequency, targetCutoff, maxDiff * Time.deltaTime);

    }

    void OnApplicationFocus(bool hasFocus)
    {
        this.hasFocus = hasFocus;
    }

    void MenuVisualsGeneric.Listener.onCycleGame(int direction)
    {
        float pitch = .85f;// direction > 0 ? 0.85f : -0.85f;
        AudioClip clip = direction > 0 ? cycleSound : cycleSound2;
        this.delayedFunction(() =>
       {

           PlayOneShot(clip, .9f, pitch);
       }, .15f);

        this.delayedFunction(() =>
        {
            PlayOneShot(cycleFinishSound, .08f, 1.75f);
        }, .5f);
    }

    void PlayOneShot(AudioClip ac, float volume, float pitch)
    {
        foreach (AudioSource aso in this.oneShotPool)
        {
            if (!aso.isPlaying)
            {
                aso.clip = ac;
                aso.pitch = pitch;
                aso.volume = volumeScale * volume;
                aso.Play();
            }
        }
    }

    void MenuVisualsGeneric.Listener.onEnterAttract()
    {

    }

    void MenuVisualsGeneric.Listener.onLeaveAttract()
    {

    }

    void MenuVisualsGeneric.Listener.onQuitGame()
    {
        PlayOneShot(quitGame, .25f, 1);
    }

    void MenuVisualsGeneric.Listener.onStartGame()
    {
        PlayOneShot(startGame, .2f, 1);
    }
}
