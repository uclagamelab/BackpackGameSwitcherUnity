/*
 
TODO: a lot of these are relative skin specific (the list, the prelaunch buttons)
 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CrockoInputMode { joystick = 0, trackball=1, mouseAndKeyboard=2 }
public class CrockoInput : MonoBehaviour
{
    public static CrockoInputMode InputMode => SwitcherSettings.Data._controlMode;

    private void Awake()
    {
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

    static float _regularMouseScrollDeltaAccumulator = 0;
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

            if (InputMode == CrockoInputMode.mouseAndKeyboard)
            {

                if (Input.GetMouseButton(0))
                {
                    float mouseDelta = Input.GetAxis("MouseDeltaY");
                    _regularMouseScrollDeltaAccumulator += 10 * mouseDelta / Screen.height;
                }
                else
                {
                    _regularMouseScrollDeltaAccumulator = 0;
                }

                _regularMouseScrollDeltaAccumulator = Mathf.Clamp(_regularMouseScrollDeltaAccumulator, -1, 1);

                float sign = Mathf.Sign(_regularMouseScrollDeltaAccumulator);
                var deadzonedAccumulator = sign * Mathf.InverseLerp(.05f, 1, Mathf.Abs(_regularMouseScrollDeltaAccumulator));

                ret += deadzonedAccumulator;// SwitcherSettings.Data.mouseScrollSpeed * .035f * sign * Mathf.Pow(Mathf.Max(0, Mathf.Abs(_regularMouseScrollDeltaAccumulator) - .05f), 1);

            }
            else if (InputMode == CrockoInputMode.trackball)// || InputMode == CrockoInputMode.regularMouse)
            {
                float mouseDelta = 2*Input.GetAxis("MouseDeltaY") / Screen.height;

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
        if (InputMode == CrockoInputMode.trackball)
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

        if (InputMode == CrockoInputMode.trackball)
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