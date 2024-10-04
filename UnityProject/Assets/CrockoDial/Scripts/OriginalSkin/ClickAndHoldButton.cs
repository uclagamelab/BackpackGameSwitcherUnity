using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClickAndHoldButton : MonoBehaviour
{
    public Image clickArea;

    public bool isHovered = false;
    public bool isPressed = false;
    public float direction = 0;
    void Start()
    {
        var et = clickArea.gameObject.AddComponent<EventTrigger>();
        {
        EventTrigger.Entry pEnter = new();
        pEnter.eventID = EventTriggerType.PointerEnter;
        pEnter.callback.AddListener((evt) => isHovered = true);
        et.triggers.Add(pEnter);
        }

        {
        EventTrigger.Entry pExit = new();
        pExit.eventID = EventTriggerType.PointerExit;
            pExit.callback.AddListener((evt) => isHovered = false);
        et.triggers.Add(pExit);
        }

        {
            EventTrigger.Entry e = new();
            e.eventID = EventTriggerType.PointerDown;
            e.callback.AddListener((evt) => isPressed = true);
            et.triggers.Add(e);
        }

        {
            EventTrigger.Entry e = new();
            e.eventID = EventTriggerType.PointerUp;
            e.callback.AddListener((evt) => isPressed = false);
            et.triggers.Add(e);
        }
    }


    void Update()
    {
        var curC = clickArea.color;
        Vector4 cv = new Vector4(curC.r, curC.g, curC.b);
        cv = Vector4.MoveTowards(cv, isHovered || isPressed ? new Vector4(1, 1, 0, 1) : new Vector4(1, 1, 1, 1), Time.deltaTime * 8);
        clickArea.color = new Color(cv.x, cv.y, cv.z);
        this.transform.localScale = Vector3.MoveTowards(this.transform.localScale, isPressed ? Vector3.one * .85f : Vector3.one, Time.deltaTime * 8);
        if (isPressed)
        {
            SpeedyListView.instance.queuedScroll = direction;
        }
    }
}
