using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SizeByText : MonoBehaviour {

    [SerializeField]
    float _speed = 0;

    //21, 300
    [SerializeField]
    ObservedText[] _srcTexts;

    public ObservedText[] srcTexts => _srcTexts;

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
            float newSize= (obt.Length * obt.sizeFac) + obt.padding;
            finalSize = Mathf.Max(newSize, finalSize);
        }
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

    [System.Serializable]
    public class ObservedText
    {
        public Text refText;
        public TMPro.TextMeshProUGUI refText_Tmp;

        public int Length
        {
            get
            {
                return refText != null ? refText.text.Length : refText_Tmp.text.Length;
            }
        }

        public float sizeFac = 40;
        public float padding = 0;


    }
}
