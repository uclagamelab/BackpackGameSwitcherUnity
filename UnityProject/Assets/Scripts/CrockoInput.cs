﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrockoInput : MonoBehaviour
{
    static bool USE_TRACKBALL_INPUT = false;
    private void Awake()
    {
        USE_TRACKBALL_INPUT = true;// XUCommandLineArguments.Contains("-trackball_input");
    }

    static bool _openGameButtonDownForced = false;
    public static void RequestOpenGameButtonDown()
    {
        _openGameButtonDownForced = true;
    }

    public void requestOpenGameButtonDown()
    {
        RequestOpenGameButtonDown();
    }

    public static bool GetOpenGameButtonDown()
    {
        return 
            !ToolsAndSettingsMenu.isOpen &&
            (CrockoInput.trackBallSubmitDown  //Trackball version
            ||
            Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space)
            ||
            _openGameButtonDownForced);
    }

    public static bool GetListScrollForward(ButtonPhase phase)
    {
        return !ToolsAndSettingsMenu.isOpen && (GetKeyState(KeyCode.UpArrow, phase) || GetKeyState(KeyCode.W, phase));
    }

    public static bool GetListScrollBack(ButtonPhase phase)
    {
        return !ToolsAndSettingsMenu.isOpen && (GetKeyState(KeyCode.DownArrow, phase) || GetKeyState(KeyCode.S, phase));
    }

    public static float GetListScroll()
    {
        float ret = 0;
        if (!ToolsAndSettingsMenu.isOpen)
        {
            if (GetListScrollForward(ButtonPhase.Held))
            {
                ret = 1;
            }
            else if (GetListScrollBack(ButtonPhase.Held))
            {
                ret = -1;
            }

            if (USE_TRACKBALL_INPUT)
            {
                bool clickScrolling = Input.GetMouseButton(0);
                float mouseDelta = !clickScrolling ? 0 : .125f*Input.GetAxis("MouseDeltaY");
                if (Mathf.Abs(mouseDelta) > 1)
                {
                    float clampedMouseDelta = mouseDelta;// Mathf.Clamp(mouseDelta * .125f, -1, 1);
                    ret += clampedMouseDelta;
                }
            }
        }
        return ret;
    }

    public static bool GetAdminMenuKeyComboDown()
    {
        return (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.K);
    }

    public class NoListVersion
    {
        public static bool GetNextGameDown()
        {

            bool ret = false;
            ret |= GetMouseSwipe(1);
            ret |= Input.GetKeyDown(KeyCode.RightArrow);
            ret |= Input.GetKeyDown(KeyCode.D);
            return !ToolsAndSettingsMenu.isOpen && ret;
        }

        public static bool GetPreviousGameDown()
        {
            bool ret = false;
            ret |= GetMouseSwipe(-1);
            ret |= Input.GetKeyDown(KeyCode.LeftArrow);
            ret |= Input.GetKeyDown(KeyCode.A);
            return !ToolsAndSettingsMenu.isOpen && ret;
        }
    }

    static bool GetMouseSwipe(int sign)
    {
        bool ret = false;
        if (USE_TRACKBALL_INPUT && false)
        {
            if (_mouseSwipeCoolDown == 0)
            {
                float mouseDelta = Input.GetAxis("MouseDeltaX");
                if (sign * mouseDelta > 3)
                {
                    _mouseSwipeCoolDown = .65f;//cooldown;
                    ret = true;
                }
            }
        }
        return ret;
    }

    static bool GetKeyState(KeyCode kc, ButtonPhase phase)
    {
        if (phase == ButtonPhase.Down)
        {
            return Input.GetKeyDown(kc);
        }
        else if (phase == ButtonPhase.Held)
        {
            return Input.GetKey(kc);
        }
        else //if (phase == ButtonPhase.Up)
        {
            return Input.GetKeyUp(kc);
        }

    }

    static float _mouseSwipeCoolDown = 0;
    static float _mouseSubmitCoolDown = 0;

    static bool trackBallSubmitDown = false;
    private void Update()
    {
        if (USE_TRACKBALL_INPUT)
        {
            trackBallSubmitDown = false;
            if (_mouseSwipeCoolDown > 0)
            {
                _mouseSwipeCoolDown -= Time.deltaTime;
                if (_mouseSwipeCoolDown < 0)
                {
                    _mouseSwipeCoolDown = 0;
                }
            }

            if (_mouseSubmitCoolDown > 0)
            {
                _mouseSubmitCoolDown -= Time.deltaTime;
                if (_mouseSubmitCoolDown < 0)
                {
                    _mouseSubmitCoolDown = 0;
                }
            }

            if (false && _mouseSubmitCoolDown == 0 && Input.GetMouseButtonDown(0))
            {
                trackBallSubmitDown = true;
                _mouseSubmitCoolDown = .75f;
            }
        }
    }
    private void LateUpdate()
    {
        _openGameButtonDownForced = false;
    }
}
public enum ButtonPhase
{
    Down, Held, Up
}