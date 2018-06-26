using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class VaSwitcherSound : MonoBehaviour, MenuVisualsGeneric.Listener {

    float volumeScale = 3f;

    bool forceSkip = false;

    public MenuVisualsGeneric menu;
    public AudioClip cycleSound;
    public AudioClip cycleSound2;
    public AudioClip cycleFinishSound;
    public AudioClip openInfoSound;
    public AudioClip closeInfoSound;
    public AudioClip startGame;
    public AudioClip quitGame;
    AudioLowPassFilter musicFilter;

    public AudioClip infoMenuCursorMove1;

    List<FileInfo> bgMusicList = new List<FileInfo>();
    int currentBgMusicIdx = 0;
    public AudioSource bgMusicSource;

    AudioSource[] oneShotPool;


    bool hasFocus = true;
    float MaxMusicVolume = 1f;

    bool multipleBGM
    {
        get
        {
            return bgMusicList.Count > 1;
        }
    }


    void Start () {
        //bgMusicSource = this.gameObject.AddComponent<AudioSource>();
        oneShotPool = new AudioSource[5];
        for (int i = 0; i < oneShotPool.Length; i++)
        {
            oneShotPool[i] = this.gameObject.AddComponent<AudioSource>();
            oneShotPool[i].playOnAwake = false;
        }
        menu.addListener(this);

        
        bgMusicSource.volume = 0;

        LoadCustomMusic();
        bgMusicSource.loop = !multipleBGM;

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
            PlayOneShot(openInfoSound, .20f, 0.75f);
        }
        else
        {
            PlayOneShot(closeInfoSound, .25f, .75f);
        }
    }

    void LoadCustomMusic()
    {
        string musicLocation = Application.streamingAssetsPath + "/BGMusic";

        foreach (string path in Directory.GetFiles(musicLocation))
        {
            string[] okExtensions = {"ogg", "wav"};
            FileInfo maybeFile = new FileInfo(path);
            foreach (string ext in okExtensions)
            {
                if (maybeFile.Extension.ToLower().EndsWith(ext))
                {
                    Debug.Log("!!!!!!!!!!!!!!!" + maybeFile);
                   // fileToUse = maybeFile;
                    bgMusicList.Add(maybeFile);
                }
            }
        }



        if (bgMusicList.Count > 0)
        {
            LoadNewSong(Random.Range(0, bgMusicList.Count));
        }
    }

    void LoadNewSong(int bgmIdx)
    {
        currentBgMusicIdx = bgmIdx;
        FileInfo fileToUse = bgMusicList[currentBgMusicIdx];
        loadingNextSongRoutine = StartCoroutine(GetAudioClipFromDisk(fileToUse));
    }

    Coroutine loadingNextSongRoutine = null;

    void OnNewMusicClipLoaded(AudioClip newClip)
    {
        this.bgMusicSource.clip = newClip;
        if (hasFocus)
        {
            bgMusicSource.Play();
        }
    }

    IEnumerator GetAudioClipFromDisk(FileInfo fileToUse)
    {
        WWW audRequest = new WWW(fileToUse.FullName);
        yield return audRequest;
        AudioClip newClip = audRequest.GetAudioClip();// false, false);
        OnNewMusicClipLoaded(newClip);
        loadingNextSongRoutine = null;
    }
	
	void Update () {
        float targMusicVolume = 0;
        if (menu.state != MenuVisualsGeneric.MenuState.LaunchGame && hasFocus)
        {
            targMusicVolume = 1;
        }


        bool alreadyInProcessOfLoadingNextSong = loadingNextSongRoutine != null;
        bool closeToEndOfSong = bgMusicSource.clip != null && !bgMusicSource.isPlaying;//.time == bgMusicSource.clip.length;
        bool shouldLoadNextSong = multipleBGM && closeToEndOfSong && targMusicVolume == 1;

        shouldLoadNextSong |= Input.GetKeyDown(KeyCode.Y);

        shouldLoadNextSong &= !alreadyInProcessOfLoadingNextSong;

        if (shouldLoadNextSong)
        {
            
            if (bgMusicSource.clip != null)
            {
                bgMusicSource.clip.UnloadAudioData();
                DestroyImmediate(bgMusicSource.clip);
            }
            
            bgMusicSource.clip = null;
               
               currentBgMusicIdx = (currentBgMusicIdx + 1) % bgMusicList.Count;
            LoadNewSong(currentBgMusicIdx);
            Debug.Log(currentBgMusicIdx + " / " + (bgMusicList.Count - 1));
        }


        this.bgMusicSource.volume = Mathf.MoveTowards(this.bgMusicSource.volume, targMusicVolume * MaxMusicVolume, .5f * Time.deltaTime);
        bool shouldPlay = bgMusicSource.volume != 0 && bgMusicSource.clip != null;
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

    void MenuVisualsGeneric.Listener.onCycleGame(int direction, bool userInitiated)
    {
        if (!userInitiated)
        {
            return;
        }

        float pitch = .85f;// direction > 0 ? 0.85f : -0.85f;
        AudioClip clip = direction > 0 ? cycleSound : cycleSound2;
        this.delayedFunction(() =>
       {

           PlayOneShot(clip, .9f, pitch);
       }, .05f);

        this.delayedFunction(() =>
        {
            PlayOneShot(cycleFinishSound, .08f, 1.75f);
        }, .4f);
    }

    void PlayOneShot(AudioClip ac, float volume, float pitch)
    {
        
        foreach (AudioSource aso in this.oneShotPool)
        {
            if (!aso.isPlaying)
            {
                aso.clip = ac;
                aso.pitch = pitch;
                aso.volume = Mathf.Clamp01(volumeScale * volume);
                aso.Play();

                //Debug.Log("playing " + ac + " --- " + aso.volume);
                break;
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
