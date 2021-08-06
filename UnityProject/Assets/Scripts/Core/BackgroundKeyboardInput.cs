using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Debug = UnityEngine.Debug;

public class BackgroundKeyboardInput : MonoBehaviour {

    #region MOUSE STUFF
    /// <summary>
    /// Struct representing a point.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public static implicit operator Vector2(POINT point)
        {
            return new Vector2(point.X, point.Y);
        }
    }

    /// <summary>
    /// Retrieves the cursor's position, in screen coordinates.
    /// </summary>
    /// <see>See MSDN documentation for further information.</see>
    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(out POINT lpPoint);

    public static Vector2 GetCursorPosition()
    {
        POINT lpPoint;
        GetCursorPos(out lpPoint);
        //bool success = User32.GetCursorPos(out lpPoint);
        // if (!success)

        return lpPoint;
    }
    #endregion

    public static class Events
    {
        public static System.Action onBackgroundKeyCombo = () => { };
        public static System.Action onBackgroundMouseClick = () => { };
    }
    


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

    float _timeOfLastInput = -420;// float.PositiveInfinity;
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

    static class Constants
    {
        public const int VK_LBUTTON = 0x01; //L Mouse Click
        public const int VK_RBUTTON = 0x02; //R Mouse Click
    }

    // Use this for initialization
    void Awake ()
    {
        _instance = this;
        _timeOfLastExitComboHit = Time.time;

    }
    public int lastKeyHit = 0;
    bool prevlMouseButtonHeld = false;
    // Update is called once per frame
    float timer = 0;
    const float rate = 1/60f;

    float _forceAttractCountDown = 0;
    void Update ()
    {
        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.A))
        {
            _timeOfLastInput = -1000;
            _forceAttractCountDown = 0.5f;
        }

        if (_forceAttractCountDown > 0)
        {
            _forceAttractCountDown -= Time.deltaTime;
            _forceAttractCountDown = _forceAttractCountDown < 0 ? 0 : _forceAttractCountDown;
        }
        bool forcingAttract = _forceAttractCountDown > 0;


        if (timer < rate)
        {
            timer += Time.deltaTime;
            return;
        }
        timer = 0;



        bool gotMouseInput = Mathf.Abs(Input.GetAxis("MouseDeltaX")) > .1f || Mathf.Abs(Input.GetAxis("MouseDeltaY")) > .1f;
        //OK????
        for (int i = 0; i < 0xFE; i++)
        {

            if (i == 144) //ignore num-lock
            {
                continue;
            }

            

            if ((GetAsyncKeyState(i) != 0 || gotMouseInput) && !forcingAttract)
            {
                //print("gotSOmethning : " + i.ToString("0x00"));
                _timeOfLastInput = Time.time;
                lastKeyHit = i;
            }


        }

       

        bool lMouseButtonHeld = GetAsyncKeyState(Constants.VK_LBUTTON) != 0;
        if (!prevlMouseButtonHeld && lMouseButtonHeld)
        {
            //Debug.Log("BG Mouse click!");
            Events.onBackgroundMouseClick.Invoke();
        }
        prevlMouseButtonHeld = lMouseButtonHeld;

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
                Events.onBackgroundKeyCombo.Invoke();
            }
        }
        else
        {

            _keyComboPressed = false;
        }
    }
}
