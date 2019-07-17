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
        bool shouldBeVisible = true;// SwitcherSettings.AdminMode || ToolsAndSettingsMenu.isOpen;
        if (shouldBeVisible != lastAppliedViz)
        {
            Cursor.visible = shouldBeVisible;
            lastAppliedViz = shouldBeVisible;
        }

    }
}
