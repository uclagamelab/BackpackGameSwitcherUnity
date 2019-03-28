using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SizeByText : MonoBehaviour {

    //21, 300
    [SerializeField]
    ObservedText[] _srcTexts;

    RectTransform rt;
	// Use this for initialization
	void Start () {
        rt = this.GetComponent<RectTransform>();

    }
	
	// Update is called once per frame
	void Update ()
    {

        float finalSize = -1;
        foreach (ObservedText obt in _srcTexts)
        {
            float newSize= (obt.refText.text.Length * obt.sizeFac) + obt.padding;
            finalSize = Mathf.Max(newSize, finalSize);
        }
        if (finalSize > 0)
        {
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, finalSize);
        }

    }

    [System.Serializable]
    class ObservedText
    {
        public Text refText;
        public float sizeFac = 40;
        public float padding = 0;
    }
}
