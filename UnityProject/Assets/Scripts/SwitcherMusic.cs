using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitcherMusic : MonoBehaviour, MenuVisualsGeneric.Listener {

    
    public AudioClip attractMusic;
    public AudioClip selectMusic;
    public AudioClip startSound;
    public AudioClip cycleSound;
    public AudioClip quitGameSound;

    AudioSource source1;
    AudioSource source2;
    AudioSource oneShotter;

    bool hasFocus = true;

    // Use this for initialization
    void Start () {
        source1 = this.gameObject.AddComponent<AudioSource>();
        source2 = this.gameObject.AddComponent<AudioSource>();
        oneShotter = this.gameObject.AddComponent<AudioSource>();
        oneShotter.playOnAwake = false;

        source1.loop = true;
        source1.playOnAwake = false;

        changeToAttractMusic();

        GameObject.FindObjectOfType<MenuVisualsGeneric>().addListener(this);

    }
	
	// Update is called once per frame
	void Update () {
        if (ProcessRunner.instance.gameProcessIsRunning || !hasFocus)
        {
            //print("something is runnig!");
            source1.volume = Mathf.MoveTowards(source1.volume, 0, Time.deltaTime);
            //Could stop the sound???? TODO
        }
        else
        {
            if (!source1.isPlaying)
            {
                source1.Play();
            }
            source1.volume = Mathf.MoveTowards(source1.volume, 1, Time.deltaTime);
        }
	}

    public void changeToAttractMusic()
    {
        source1.volume = 1;
        source1.clip = attractMusic;
        source1.Play();
    }


    public void changeToSelectMusic()
    {
        source1.volume = 1;
        source1.clip = selectMusic;
        source1.Play();
    }

    public void onLeaveAttract()
    {
        //oneShotter.pitch = 4;
        oneShotter.PlayOneShot(quitGameSound);
        changeToSelectMusic();
    }

    public void onEnterAttract()
    {

        changeToAttractMusic();
    }

    public void onCycleGame(int dir)
    {
        if (AttractMode.Instance == null || !AttractMode.Instance.running)
        {
            oneShotter.pitch = 1;
            this.oneShotter.PlayOneShot(cycleSound);
        }
  
    }

    public void onStartGame()
    {
        oneShotter.pitch = 1;
        oneShotter.PlayOneShot(startSound);
    }

    public void onQuitGame()
    {
        oneShotter.pitch = 1;
        oneShotter.PlayOneShot(quitGameSound);
    }

    public void OnApplicationFocus(bool focus)
    {
        this.hasFocus = focus;   
    }
}
