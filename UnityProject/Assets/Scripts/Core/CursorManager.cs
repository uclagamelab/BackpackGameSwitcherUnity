using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    bool lastAppliedViz = true;
    // Update is called once per frame
    void Update()
    {
        bool shouldBeVisible = SwitcherSettings.AdminMode || ToolsAndSettingsMenu.isOpen;
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
