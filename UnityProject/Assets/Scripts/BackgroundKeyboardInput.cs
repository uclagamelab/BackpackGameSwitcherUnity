using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;


public class BackgroundKeyboardInput : MonoBehaviour {

    public interface Listener
    {
        Transform transform
        {
            get;
        }
        GameObject gameObject
        {
            get;
        }

        void onBackgroundKeyCombo();
    }

    List<Listener> listeners;

    float _timeOfLastExitComboHit = float.NegativeInfinity;
    public float timeOfLastExitComboHit
    {
        get { return _timeOfLastExitComboHit; }
        protected set { _timeOfLastExitComboHit = value; }
    }


    static BackgroundKeyboardInput _instance;
    public static BackgroundKeyboardInput Instance
    {
        get { return _instance; }
    }

    float _timeOfLastInput = float.PositiveInfinity;
    public float timeOfLastInput
    {
        get
        {
            return _timeOfLastInput;
        }
    }

    // Polls the given Virtual Keycode to check it's state
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vlc);

    bool _keyComboPressed = false;

   

    // Use this for initialization
    void Awake ()
    {
        _instance = this;
        listeners = new List<Listener>();
	}
	
	// Update is called once per frame
	void Update () {

        //OK????
        for (int i = 0; i < 0xFE; i++)
        {
            if (GetAsyncKeyState(i) != 0)
            {
                //print("gotSOmethning : " + i.ToString("0x00"));
                _timeOfLastInput = Time.time;
            }
        }

        bool altShiftBheld = GetAsyncKeyState(0x10) != 0 && GetAsyncKeyState(0x12) != 0 && GetAsyncKeyState(0x42) != 0;
        //bool ctrlCHeld = GetAsyncKeyState(0x11) != 0 && GetAsyncKeyState(0x43) != 0;
        // virtual key codes for "ctrl" and "c"
        //if(GetAsyncKeyState(0x11)!=0 && GetAsyncKeyState(0x43)!=0)
        if (altShiftBheld)
        {

            if (!_keyComboPressed)
            {
                _keyComboPressed = true;
                timeOfLastExitComboHit = Time.time;
                foreach (Listener l in listeners)
                {
                    l.onBackgroundKeyCombo();
                }
            }
        }
        else
        {

            _keyComboPressed = false;
        }
    }

    public void addListener(Listener l)
    {
        this.listeners.Add(l);
    }
}
