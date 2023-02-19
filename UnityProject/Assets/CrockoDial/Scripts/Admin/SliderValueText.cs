using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderValueText : MonoBehaviour {
    public Slider slider;
    Text text;

	void Start () {
        text = this.GetComponent<Text>();
        slider.onValueChanged.AddListener(ValChan);

        ValChan(slider.value);
    }

    void ValChan(float val)
    {
        text.text = "" + Mathf.RoundToInt(100 * Mathf.InverseLerp(slider.minValue, slider.maxValue, val));
    }
}
