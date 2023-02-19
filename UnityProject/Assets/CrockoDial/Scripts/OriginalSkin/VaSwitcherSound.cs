using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Audio;

public class VaSwitcherSound : MonoBehaviour {

    [SerializeField]
    SpeedyListView _speedyList;

    float sfxVolume
    {
        get
        {
            return SwitcherSettings.Data._SFXVolume;
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
        menu.OnStartGame += onStartGame;
        ProcessRunner.Events.OnProcessExited += onQuitGame;

        menu.OnOpenCloseInfo+= onOpenCloseInfo;

        menu.InfoMenuCursorMove += OnInfoMenuCursorMove;

        _speedyList.OnPassedItem += OnListPass;
        _speedyList.OnStoppedAtItem += OnListStop;
    }

    private void OnDestroy()
    {
        ProcessRunner.Events.OnProcessExited -= onQuitGame;
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
        PlayOneShot(this.listPlink1, 1, 1.0f + 1.0f * Mathf.Abs(_speedyList.speed));
    }
    void OnListStop()
    {
         PlayOneShot(this.listPlonk, .25f,1f);
    }

    void OnInfoMenuCursorMove(bool open)
    {
        PlayOneShot(infoMenuCursorMove1, .225f, 1);
    }

        void onOpenCloseInfo(bool open)
    {
        if (open)
        {
            PlayOneShot(openInfoSound, .75f, 0.75f);
        }
        else
        {
            PlayOneShot(closeInfoSound, .85f, .75f);
        }
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


    void onQuitGame()
    {
        PlayOneShot(quitGame, 1, 1);
    }

    void onStartGame()
    {
        PlayOneShot(startGame, 1.25f, 1);
    }
}
