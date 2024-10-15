using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{

    bool visibleWhileChoosingAGame = false;
    bool lastAppliedViz = true;
    private void Start()
    {
        visibleWhileChoosingAGame = SwitcherSettings.Data._controlMode == CrockoInputMode.mouseAndKeyboard;
        Screen.fullScreen = true;
    }

    void Update()
    {
        bool shouldBeVisible = visibleWhileChoosingAGame || SwitcherSettings.AdminMode || ToolsAndSettingsMenu.isOpen;
        #if UNITY_EDITOR
        shouldBeVisible = true;
        #endif
        if (shouldBeVisible != lastAppliedViz)
        {
            Cursor.visible = shouldBeVisible;
            lastAppliedViz = shouldBeVisible;
        }

            bool keyCommandEntered = 
            (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) 
            && 
            (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return));

        keyCommandEntered |= (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.F);

        if (keyCommandEntered 
            && 
            (!SwitcherSettings.Data._SecurityMode || ToolsAndSettingsMenu.isOpen || !Screen.fullScreen))
        {
            Screen.fullScreen = !Screen.fullScreen;
        }

    }
}
