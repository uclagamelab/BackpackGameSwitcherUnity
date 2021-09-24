using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{

    public bool whileChoosingGame = true;
    bool lastAppliedViz = true;

    void Update()
    {
        bool shouldBeVisible = whileChoosingGame || SwitcherSettings.AdminMode || ToolsAndSettingsMenu.isOpen;
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
