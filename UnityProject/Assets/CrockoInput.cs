using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrockoInput : MonoBehaviour
{
    static readonly bool USE_TRACKBALL_INPUT = false;
    public static bool GetOpenGameButtonDown()
    {
        return 
            !ToolsAndSettingsMenu.isOpen &&
            (USE_TRACKBALL_INPUT && Input.GetMouseButtonDown(0)) || //Trackball version
            Input.GetKeyDown(KeyCode.UpArrow);
    }

    public static bool GetListScrollForward(ButtonPhase phase)
    {
        return !ToolsAndSettingsMenu.isOpen && GetKeyState(KeyCode.LeftArrow, phase);
    }

    public static bool GetListScrollBack(ButtonPhase phase)
    {
        return !ToolsAndSettingsMenu.isOpen && GetKeyState(KeyCode.RightArrow, phase);
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
                float mouseDelta = Input.GetAxis("MouseDelta");
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
        return (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.K);
    }

    public class NoListVersion
    {
        public static bool GetNextGameDown()
        {
          
                return !ToolsAndSettingsMenu.isOpen && Input.GetKeyDown(KeyCode.W);
        }

        public static bool GetPreviousGameDown()
        {
            return !ToolsAndSettingsMenu.isOpen && Input.GetKeyDown(KeyCode.S);
        }
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
}
public enum ButtonPhase
{
    Down, Held, Up
}