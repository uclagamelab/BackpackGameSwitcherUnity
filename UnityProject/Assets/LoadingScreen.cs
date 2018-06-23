using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreen : MonoBehaviour, MenuVisualsGeneric.Listener {

    public static LoadingScreen instance;
	// Use this for initialization
	void Awake () {
        instance = this;	
	}

    void Start()
    {
        GameObject.FindObjectOfType<MenuVisualsGeneric>().addListener(this);
    }

    public void onLeaveAttract()
    {
        //this.on = false;
    }

    public void onEnterAttract()
    {
       // this.on = false;
    }

    public void onCycleGame(int dir)
    {

    }

    public void onStartGame()
    {
        //this.on = t;
    }

    public void onQuitGame()
    {
       // this.on = false;
    }

    public bool on
    {
        get
        {
            return this.transform.GetChild(0).gameObject.activeSelf;
        }

        set
        {
            this.transform.GetChild(0).gameObject.SetActive(value);
        }
    }

}
