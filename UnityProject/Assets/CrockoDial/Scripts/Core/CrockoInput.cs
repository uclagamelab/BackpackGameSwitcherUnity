/*
 
TODO: a lot of these are relative skin specific (the list, the prelaunch buttons)
 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CrockoInputMode { joystick = 0, trackball=1, regularMouse=2 }
public class CrockoInput : MonoBehaviour
{
    static bool USE_TRACKBALL_INPUT = false;

    private void Awake()
    {
        USE_TRACKBALL_INPUT = SwitcherSettings.Data._controlMode == CrockoInputMode.trackball;
        BackgroundKeyboardInput.Events.onBackgroundKeyCombo += onBackgroundKeyCombo;
    }

    static float _postQuitInputSuppressTimer = 0;
    static bool suppressInputTemporarilyPostQuit => _postQuitInputSuppressTimer > 0;
    void onBackgroundKeyCombo()
    {
        if (ProcessRunner.instance.IsGameRunning())
        {
            _postQuitInputSuppressTimer = .3f;
        }
    }
    
    public static bool GetOpenGameButtonDown()
    {
        return 
            !ToolsAndSettingsMenu.isOpen
            && !suppressInputTemporarilyPostQuit
            && (
                CrockoInput.trackBallSubmitDown || //Trackball version
                Input.GetKeyDown(KeyCode.Return)
                || Input.GetKeyDown(KeyCode.KeypadEnter)
                || Input.GetKeyDown(KeyCode.Space)
            )
            ;
    }

    public static bool GetListScrollForward(ButtonPhase phase)
    {
        return !ToolsAndSettingsMenu.isOpen && !suppressInputTemporarilyPostQuit && GetKeyState(KeyCode.UpArrow, phase);
    }

    public static bool GetListScrollBack(ButtonPhase phase)
    {
        return !ToolsAndSettingsMenu.isOpen && !suppressInputTemporarilyPostQuit && GetKeyState(KeyCode.DownArrow, phase);
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
                float mouseDelta = Input.GetAxis("MouseDeltaY");
                if (Mathf.Abs(mouseDelta) > 1)
                {
                    float clampedMouseDelta = Mathf.Clamp(mouseDelta * .125f, -1, 1);
                    ret += clampedMouseDelta;
                }
            }
        }
        return ret;
    }

    public static bool GetAdminMenuKeyComboDown()
    {
        bool ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        #if UNITY_EDITOR
        ctrlHeld &= Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt); //Unity has its own ctrl-k now.... 
        #endif
        return ctrlHeld && Input.GetKeyDown(KeyCode.K);
    }

    public class PrelaunchMenuInput
    {
        public static bool OnSelectRight()
        {
            bool ret = false;
            ret |= GetXMouseSwipe(1);
            ret |= Input.GetKeyDown(KeyCode.RightArrow);
            ret &= !suppressInputTemporarilyPostQuit;
            return !ToolsAndSettingsMenu.isOpen && ret;
        }

        public static bool OnSelectLeft()
        {
            bool ret = false;
            ret |= GetXMouseSwipe(-1);
            ret |= Input.GetKeyDown(KeyCode.LeftArrow);
            ret &= !suppressInputTemporarilyPostQuit;
            return !ToolsAndSettingsMenu.isOpen && ret;
        }
    }

    static bool GetXMouseSwipe(int sign)
    {
        bool ret = false;
        if (USE_TRACKBALL_INPUT)
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
        if (_postQuitInputSuppressTimer >= 0)
        {
            _postQuitInputSuppressTimer -= Time.deltaTime;
        }

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

            if (_mouseSubmitCoolDown == 0 && Input.GetMouseButtonDown(0))
            {
                trackBallSubmitDown = true;
                _mouseSubmitCoolDown = .75f;
            }
        }
    }
}
public enum ButtonPhase
{
    Down, Held, Up
}