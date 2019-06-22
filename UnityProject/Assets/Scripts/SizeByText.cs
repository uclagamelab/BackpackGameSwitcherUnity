﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SizeByText : MonoBehaviour {

    public bool manualOnly = false;
    [SerializeField]
    float _speed = 0;

    //21, 300
    [SerializeField]
    ObservedText[] _srcTexts;

    public ObservedText[] srcTexts => _srcTexts;

    RectTransform rt;
    public float xPadding = 0;
    // Use this for initialization
    void Awake() {
        rt = this.GetComponent<RectTransform>();
        if (manualOnly)
        {
            this.enabled = false;
        }

    }

    //public void ForceUpdate()
    //{
    //    LateUpdate();
    //}
    // Update is called once per frame
    public void ForceUpdate ()
    {
        foreach (ObservedText txt in _srcTexts)
        {
            txt.refText_Tmp.ForceMeshUpdate();
        }

                float finalSize = -1;
        //foreach (ObservedText obt in _srcTexts)
        //{
        //    float newSize= (obt.Length * obt.sizeFac) + obt.padding;
        //    finalSize = Mathf.Max(newSize, finalSize);
        //}
        finalSize = getRenderedSize().x + xPadding;
        if (finalSize > 0)
        {

            if (_speed == 0)
            {
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, finalSize);
            }
            else
            {
                
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Lerp(rt.sizeDelta.x, finalSize, _speed * Time.deltaTime));
            }
        }

    }

    public Vector2 getRenderedSize()
    {
        Vector2 ret = Vector2.zero;
        foreach (ObservedText txt in _srcTexts)
        {
            if (txt.refText_Tmp != null)
            {
                Vector2 renVal = txt.refText_Tmp.GetRenderedValues();
                ret.x = Mathf.Max(ret.x, Mathf.Abs(renVal.x));
                ret.y = Mathf.Max(ret.y, Mathf.Abs(renVal.y));
            }
        }
        return ret;
    }


    [System.Serializable]
    public class ObservedText
    {
        public Text refText;
        public TMPro.TextMeshProUGUI refText_Tmp;

        public int Length
        {
            get
            {
                return refText != null ? refText.text.Length : refText_Tmp != null ? refText_Tmp.text.Length : 0;
            }
        }


        public float sizeFac = 40;
        public float padding = 0;


    }
}
