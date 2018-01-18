using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RawTexAspectFixer : MonoBehaviour {
    RectTransform rt;
    RawImage ri;
	// Use this for initialization
	void Start () {
        rt = this.GetComponent<RectTransform>();
        ri = this.GetComponent<RawImage>();
	}
	
	// Update is called once per frame
	void LateUpdate () {
		if (ri.texture != null)
        {
            //rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, )

            //Vector2 sizeDelta = rt.sizeDelta;
            //sizeDelta.x = rt.sizeDelta.y * (ri.texture.width / ri.texture.height);
            //rt.sizeDelta = sizeDelta;

            float y = RectTransformUtility.PixelAdjustRect(rt, this.GetComponentInParent<Canvas>()).height;
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Abs(y * ri.texture.width / ri.texture.height));

        }
	}
}
