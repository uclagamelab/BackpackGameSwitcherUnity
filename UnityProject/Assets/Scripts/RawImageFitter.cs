using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class RawImageFitter : MonoBehaviour {




    public enum Mode
    {
        MatchHeight,
        MatchWidth,
        Cover,
        FitInside
    }

    public Mode mode = Mode.MatchHeight;

    RectTransform rt;
    //RawImage ri;
    Texture tex
    {
    get 
        {
            return this.GetComponent<RawImage>().texture;
        }
    }

	// Use this for initialization
	void Start () {
        rt = this.GetComponent<RectTransform>();
        //ri = this.GetComponent<RawImage>();
        //v.texture.wi

	}
	
	// Update is called once per frame
	void LateUpdate () {
		if (tex != null)
        {
            //rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, )

            //Vector2 sizeDelta = rt.sizeDelta;
            //sizeDelta.x = rt.sizeDelta.y * (ri.texture.width / ri.texture.height);
            //rt.sizeDelta = sizeDelta;

            Vector2 containerSize = new Vector2(Screen.width, Screen.height);
            if (this.transform.parent.GetComponent<Canvas>() == null)
            {
                containerSize = RectTransformUtility.PixelAdjustRect(this.transform.parent.GetComponentInParent<RectTransform>(), this.GetComponentInParent<Canvas>()).size;//
            }
            bool matchHeight = false;

            //horiz = (Screen.height / Screen.width) > (ri.texture.height / ri.texture.width);

            float matchWidthHeight = tex.height * (1.0f / tex.width) * containerSize.x;
            float matchHeightWidth = tex.width * (1.0f / tex.height) * containerSize.y;
            if (mode == Mode.Cover || mode == Mode.FitInside)
            {
                matchHeight = matchWidthHeight <= containerSize.y;
                if (mode == Mode.FitInside)
                {
                    matchHeight = !matchHeight;
                }
            }
            else if (mode == Mode.MatchHeight)
            {
                matchHeight = true;
            }
            else
            {
                matchHeight = false;
            }

            if (matchHeight)
            {
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(matchHeightWidth, containerSize.y);
            }
            else
            {
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(containerSize.x, matchWidthHeight);
            }


            /*if (horiz)//!fitHorizontally)
            { 
                float y = RectTransformUtility.PixelAdjustRect(rt, this.GetComponentInParent<Canvas>()).height;
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Abs(y * ri.texture.width / ri.texture.height));
            }
            else
            {
                float x = RectTransformUtility.PixelAdjustRect(rt, this.GetComponentInParent<Canvas>()).width;
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Abs(x * ri.texture.height / ri.texture.width));
            }*/
        }
	}
}
