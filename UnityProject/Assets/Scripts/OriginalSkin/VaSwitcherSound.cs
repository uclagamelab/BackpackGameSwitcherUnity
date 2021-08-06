using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Audio;

public class VaSwitcherSound : MonoBehaviour, MenuVisualsGeneric.Listener {

    [SerializeField]
    SpeedyListView _speedyList;

    float sfxVolume
    {
        get
        {
            return 3.5f;// * _sfxVolFromeSettings;
        }
    }

    [SerializeField]
    AudioMixerGroup _sfxAudioMixerGroup;

    MenuVisualsGeneric menu => MenuVisualsGeneric.Instance;
    public AudioClip cycleSound;
    public AudioClip cycleSound2;
    public AudioClip cycleFinishSound;
    public AudioClip openInfoSound;
    public AudioClip closeInfoSound;
    public AudioClip startGame;
    public AudioClip quitGame;

    public AudioClip listPlink1;
    public AudioClip listPlonk;

    public AudioClip infoMenuCursorMove1;

    AudioSource[] oneShotPool;

    [SerializeField] AudioLowPassFilter musicFilter;


    void Start () {
        //bgMusicSource = this.gameObject.AddComponent<AudioSource>();
        oneShotPool = new AudioSource[10];
        for (int i = 0; i < oneShotPool.Length; i++)
        {
            oneShotPool[i] = this.gameObject.AddComponent<AudioSource>();
            oneShotPool[i].playOnAwake = false;
            oneShotPool[i].outputAudioMixerGroup = _sfxAudioMixerGroup;
        }
        menu.addListener(this);

        menu.OnOpenCloseInfo+= onOpenCloseInfo;

        menu.InfoMenuCursorMove += OnInfoMenuCursorMove;

        _speedyList.OnPassedItem += OnListPass;
        _speedyList.OnStoppedAtItem += OnListStop;
    }

    void Update()
    {
        float targMusicVolume = 0;
        if (menu.state != MenuVisualsGeneric.MenuState.LaunchGame)
        {
            targMusicVolume = 1;
        }
        BgMusicPlayer.instance.targMusicVolume = targMusicVolume;

        float targetCutoff = 22000;
        if (PreLaunchGameInfo.Instance.open || (PreLaunchGameInfo.Instance.animating && !PreLaunchGameInfo.Instance.open))
        {
            targetCutoff = 850;
        }

        float maxDiff = 22000 - 850;
        this.musicFilter.cutoffFrequency = Mathf.Lerp(this.musicFilter.cutoffFrequency, targetCutoff, 8 * Time.deltaTime);// 2*maxDiff * Time.deltaTime);
        this.musicFilter.cutoffFrequency = Mathf.MoveTowards(this.musicFilter.cutoffFrequency, targetCutoff, maxDiff * Time.deltaTime);
    }

    void OnListPass()
    {

        PlayOneShot(this.listPlink1, .25f, 1.5f + 2.1f * Mathf.Abs(_speedyList.speed));
    }
    void OnListStop()
    {
        //xxx
        
        
            
        //Mathf.Round(_speedyList.fuzzyIdx) == Mathf.Round(_speedyList.stopIndex) && 

         PlayOneShot(this.listPlonk, .35f, 1.25f);
        
    }

    void OnInfoMenuCursorMove(bool open)
    {
        PlayOneShot(infoMenuCursorMove1, .075f, 1);
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
                aso.volume = Mathf.Clamp01(sfxVolume * volume);
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
        PlayOneShot(startGame, .5f, 1);
    }
}
