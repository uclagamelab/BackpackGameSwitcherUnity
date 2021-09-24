using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SizeByText : MonoBehaviour {

    enum CombineMode
    {
        max, sum
    }

    public bool manualOnly = false;
    [SerializeField]
    float _speed = 0;

    [SerializeField]
    bool resizeX = true;
    [SerializeField]
    CombineMode xCombine = CombineMode.max;

    [SerializeField]
    bool resizeY = false;

    [SerializeField]
    CombineMode yCombine = CombineMode.sum;

    //21, 300
    [SerializeField]
    ObservedText[] _srcTexts;

    public ObservedText[] srcTexts => _srcTexts;

    [SerializeField]
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
    [ContextMenu("Update Size")]
    public void ForceUpdate ()
    {
        foreach (ObservedText txt in _srcTexts)
        {
            txt.refText_Tmp.ForceMeshUpdate();
        }
        Vector2 renderedSize = getRenderedSize();
        if (resizeX)
        {
            float finalSizeX = -1;
            //foreach (ObservedText obt in _srcTexts)
            //{
            //    float newSize= (obt.Length * obt.sizeFac) + obt.padding;
            //    finalSize = Mathf.Max(newSize, finalSize);
            //}
            finalSizeX = Mathf.Max(200, renderedSize.x) + xPadding;
            if (finalSizeX > 0)
            {

                if (_speed == 0)
                {
                    rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, finalSizeX);
                }
                else
                {

                    rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Lerp(rt.sizeDelta.x, finalSizeX, _speed * Time.deltaTime));
                }
            }
        }


        if (resizeY)
        {
            float finalSizeY = -1;
            //foreach (ObservedText obt in _srcTexts)
            //{
            //    float newSize= (obt.Length * obt.sizeFac) + obt.padding;
            //    finalSize = Mathf.Max(newSize, finalSize);
            //}
            finalSizeY = Mathf.Max(75, renderedSize.y) + xPadding;
        
            if (finalSizeY > 0)
            {

                if (_speed == 0)
                {
                    rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, finalSizeY);
                }
                else
                {

                    rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Lerp(rt.sizeDelta.y, finalSizeY, _speed * Time.deltaTime));
                }
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
                Vector2 renVal = txt.refText_Tmp.GetRenderedValues(true);
                if (renVal.x < 0)
                {
                    renVal.x = 0;
                }

                if (renVal.y < 0)
                {
                    renVal.y = 0;
                }

                if (xCombine == CombineMode.sum)
                {
                    ret.x += renVal.x;
                }
                else
                {
                    ret.x = Mathf.Max(ret.x, renVal.x);
                }

                if (yCombine == CombineMode.sum)
                {
                    ret.y += renVal.y;
                }
                else
                {
                    ret.y = Mathf.Max(ret.y, renVal.y);
                }
                
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
    }
}
