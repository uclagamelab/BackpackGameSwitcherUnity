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


    public float _escapeTimer = 0;


    static BackgroundKeyboardInput _instance;
    public static BackgroundKeyboardInput Instance => _instance;



    float _secondsSinceLastInput = float.MaxValue;
    public float secondsSinceLastInput => _secondsSinceLastInput;

    // Polls the given Virtual Keycode to check it's state
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vlc);

    bool _keyComboPressed = false;


    public int lastKeyHit = 0;
    bool prevlMouseButtonHeld = false;
    // Update is called once per frame

    const float rate = 1/60f;
    private const int MAX_KEY_ID_CHECKED = 0xFE;

    bool[] _pollValuesPrev = new bool[MAX_KEY_ID_CHECKED];
    bool[] _pollValues = new bool[MAX_KEY_ID_CHECKED];
    
    float _forceAttractCountDown = 0;

    void Awake()
    {
        _instance = this;

    }

    void Update ()
    {
        _secondsSinceLastInput += Time.deltaTime;

        bool escapeHeld = GetAsyncKeyState((int) VKeyCode.Escape) != 0;
        if (!escapeHeld)
        {
            _escapeTimer = 0;
        }
        else
        {
            _escapeTimer += Time.deltaTime;
        }

        //Force attrack with Shift-A
        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.A))
        {
            _secondsSinceLastInput = 60 * 60; //1 hour
            _forceAttractCountDown = 0.5f;
        }

        if (_forceAttractCountDown > 0)
        {
            _forceAttractCountDown -= Time.deltaTime;
            _forceAttractCountDown = _forceAttractCountDown < 0 ? 0 : _forceAttractCountDown;
        }
        bool forcingAttract = _forceAttractCountDown > 0;


        bool gotMouseInput = Mathf.Abs(Input.GetAxis("MouseDeltaX")) > .1f || Mathf.Abs(Input.GetAxis("MouseDeltaY")) > .1f;
        //OK????
        for (int i = 0; i < MAX_KEY_ID_CHECKED; i++)
        {

            if (i == 144) //ignore num-lock
            {
                continue;
            }

            short keyState = GetAsyncKeyState(i);
            _pollValuesPrev[i] = _pollValues[i];
            _pollValues[i] = keyState != 0;

            if ((keyState != 0 || gotMouseInput) && !forcingAttract)
            {
                _secondsSinceLastInput = 0;
                lastKeyHit = i;
            }
        }



        bool lMouseButtonHeld = GetAsyncKeyState((int) VKeyCode.LButton) != 0;
        if (!prevlMouseButtonHeld && lMouseButtonHeld)
        {
            //Debug.Log("BG Mouse click!");
            Events.onBackgroundMouseClick.Invoke();
        }
        prevlMouseButtonHeld = lMouseButtonHeld;



        bool altShiftBheld =
            GetKey(VKeyCode.Alt) && GetKey(VKeyCode.Shift) && GetKey(VKeyCode.B);
        //GetAsyncKeyState(0x10) != 0 && GetAsyncKeyState(0x12) != 0 && GetAsyncKeyState(0x42) != 0;
        altShiftBheld |= _escapeTimer >= 2;

        if (altShiftBheld)
        {
            if (!_keyComboPressed)
            {
                
                _keyComboPressed = true;
                Events.onBackgroundKeyCombo.Invoke();
            }
        }
        else
        {

            _keyComboPressed = false;
        }
    }

    public static bool GetKey(VKeyCode kc)
    {
        int i = (int)kc;
        var ar = Instance._pollValues;
        if (i >= 0 && i < ar.Length)
        {
            return ar[i];
        }
        return false;
    }

    public static bool GetKeyDown(VKeyCode kc)
    {
        int i = (int)kc;
        var arPrev = Instance._pollValuesPrev;
        var ar = Instance._pollValues;
        if (i >= 0 && i < ar.Length)
        {
            return !arPrev[i] && ar[i];
        }
        return false;
    }

    public static bool GetKeyUp(VKeyCode kc)
    {
        int i = (int)kc;
        var arPrev = Instance._pollValuesPrev;
        var ar = Instance._pollValues;
        if (i >= 0 && i < ar.Length)
        {
            return arPrev[i] && !ar[i];
        }
        return false;
    }
}

//Modified: System.Windows.Forms.Keys
public enum VKeyCode
{
    Modifiers = -65536,
    None = 0,
    LButton = 1,
    RButton = 2,
    Cancel = 3,
    MButton = 4,
    XButton1 = 5,
    XButton2 = 6,
    Back = 8,
    Tab = 9,
    LineFeed = 10,
    Clear = 12,
    Return = 13,
    Enter = 13,
    Shift = 16,
    ControlKey = 17,
    Alt = 18,
    Pause = 19,
    CapsLock = 20,
    Capital = 20,
    KanaMode = 21,
    HanguelMode = 21,
    HangulMode = 21,
    JunjaMode = 23,
    FinalMode = 24,
    KanjiMode = 25,
    HanjaMode = 25,
    Escape = 27,
    IMEConvert = 28,
    IMENonconvert = 29,
    IMEAceept = 30,
    IMEAccept = 30,
    IMEModeChange = 31,
    Space = 32,
    PageUp = 33,
    Prior = 33,
    PageDown = 34,
    Next = 34,
    End = 35,
    Home = 36,
    Left = 37,
    Up = 38,
    Right = 39,
    Down = 40,
    Select = 41,
    Print = 42,
    Execute = 43,
    PrintScreen = 44,
    Snapshot = 44,
    Insert = 45,
    Delete = 46,
    Help = 47,
    D0 = 48,
    D1 = 49,
    D2 = 50,
    D3 = 51,
    D4 = 52,
    D5 = 53,
    D6 = 54,
    D7 = 55,
    D8 = 56,
    D9 = 57,
    A = 65,
    B = 66,
    C = 67,
    D = 68,
    E = 69,
    F = 70,
    G = 71,
    H = 72,
    I = 73,
    J = 74,
    K = 75,
    L = 76,
    M = 77,
    N = 78,
    O = 79,
    P = 80,
    Q = 81,
    R = 82,
    S = 83,
    T = 84,
    U = 85,
    V = 86,
    W = 87,
    X = 88,
    Y = 89,
    Z = 90,
    LWin = 91,
    RWin = 92,
    Apps = 93,
    Sleep = 95,
    NumPad0 = 96,
    NumPad1 = 97,
    NumPad2 = 98,
    NumPad3 = 99,
    NumPad4 = 100,
    NumPad5 = 101,
    NumPad6 = 102,
    NumPad7 = 103,
    NumPad8 = 104,
    NumPad9 = 105,
    Multiply = 106,
    Add = 107,
    Separator = 108,
    Subtract = 109,
    Decimal = 110,
    Divide = 111,
    F1 = 112,
    F2 = 113,
    F3 = 114,
    F4 = 115,
    F5 = 116,
    F6 = 117,
    F7 = 118,
    F8 = 119,
    F9 = 120,
    F10 = 121,
    F11 = 122,
    F12 = 123,
    F13 = 124,
    F14 = 125,
    F15 = 126,
    F16 = 127,
    F17 = 128,
    F18 = 129,
    F19 = 130,
    F20 = 131,
    F21 = 132,
    F22 = 133,
    F23 = 134,
    F24 = 135,
    NumLock = 144,
    Scroll = 145,
    LShiftKey = 160,
    RShiftKey = 161,
    LControlKey = 162,
    RControlKey = 163,
    LMenu = 164,
    RMenu = 165,
}