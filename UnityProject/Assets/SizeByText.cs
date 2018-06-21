using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SizeByText : MonoBehaviour {

    public Text refText;
    public float sizeFac = 40;
    public float padding = 0;
    RectTransform rt;
	// Use this for initialization
	void Start () {
        rt = this.GetComponent<RectTransform>();

    }
	
	// Update is called once per frame
	void Update () {
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (refText.text.Length * sizeFac) + padding);

    }
}
