using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class RawImageFitter : MonoBehaviour {

    public Vector2 _offset;
    public Vector2 offset
    {
        get => _offset;
        set 
        { 
            _offset = value;
            forceUpdate();
        }
    }

    public Vector2 sz = Vector2.zero;

    public enum Mode
    {
        MatchHeight,
        MatchWidth,
        Cover,
        FitInside
    }

    public Mode mode = Mode.MatchHeight;

    RectTransform rt;
    RectTransform parentRt;
    RawImage rawImg;
    Canvas canvas;
    //RawImage ri;
    Texture tex
    {
    get 
        {
            return rawImg?.texture;
        }
    }

	// Use this for initialization
	void Start ()
    {
        rawImg = this.GetComponent<RawImage>();
        rt = this.GetComponent<RectTransform>();
        parentRt = this.transform.parent.GetComponentInParent<RectTransform>();
        canvas = this.transform.parent.GetComponent<Canvas>();
        //ri = this.GetComponent<RawImage>();
        //v.texture.wi

        //sz = this.rt.sizeDelta;
    }
	
	// Update is called once per frame
	void LateUpdate ()
    {

    }
	void forceUpdate()
    {
		if (tex != null)
        {
            //rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, )

            //Vector2 sizeDelta = rt.sizeDelta;
            //sizeDelta.x = rt.sizeDelta.y * (ri.texture.width / ri.texture.height);
            //rt.sizeDelta = sizeDelta;

            Vector2 containerSize = sz;//new Vector2(Screen.width, Screen.height);
            if (canvas == null)
            {
                containerSize = RectTransformUtility.PixelAdjustRect(parentRt, canvas).size;//
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
            rt.anchoredPosition += offset;
        }
	}
}
