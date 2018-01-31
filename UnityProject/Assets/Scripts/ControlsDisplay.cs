using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class ControlsDisplay : MonoBehaviour {
    [SerializeField]
    Image coloredBg;
    [SerializeField]
    RectTransform controlGfx;
    bool animating = false;
    public bool appeared = false;
    public Text joystickText;
    public Text[] buttonTexts;

    

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float targetBgAlpha = 1;
        Vector3 targetPos = Vector3.zero;
		if (!appeared)
        {
            targetBgAlpha = 0;
            targetPos = new Vector3(0,-300, 0);
        }

        Color c = coloredBg.color;
        c.a = Mathf.Lerp(c.a, targetBgAlpha, Time.deltaTime * 2);
        coloredBg.color = c;
        controlGfx.anchoredPosition = Vector3.Lerp(controlGfx.anchoredPosition, targetPos, Time.deltaTime * 5);

    }

    public void appear()
    {

    }
}
