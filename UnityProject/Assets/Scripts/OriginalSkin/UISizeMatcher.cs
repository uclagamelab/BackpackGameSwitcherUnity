using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class UISizeMatcher : MonoBehaviour
{
    public RectTransform _toMatch;
    public RectTransform _rt;
    public Vector2 _anchoredPositionOffset = Vector2.zero;
    public Vector2 _sizeDeltaExtra = Vector2.zero;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_toMatch != null && _rt != null)
        {

            _rt.anchoredPosition = _toMatch.anchoredPosition + _anchoredPositionOffset;
            //_rt.anchorMin = _toMatch.anchorMin;
            //_rt.anchorMax = _toMatch.anchorMax;
            Vector2 oldSizeDelta = _rt.sizeDelta;
            Vector2 finalSizeDelta = _toMatch.sizeDelta + _sizeDeltaExtra;
            finalSizeDelta.x = oldSizeDelta.x;
            _rt.sizeDelta = finalSizeDelta;
        }
    }
}
