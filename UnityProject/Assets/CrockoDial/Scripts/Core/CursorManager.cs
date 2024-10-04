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

    }
}
