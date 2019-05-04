using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderValueText : MonoBehaviour {
    public Slider slider;
    Text text;
	// Use this for initialization
	void Start () {
        text = this.GetComponent<Text>();
        slider.onValueChanged.AddListener(ValChan);
    }

    void ValChan(float val)
    {
        text.text = "" + Mathf.RoundToInt(100 * Mathf.InverseLerp(slider.minValue, slider.maxValue, val));
    }
}
