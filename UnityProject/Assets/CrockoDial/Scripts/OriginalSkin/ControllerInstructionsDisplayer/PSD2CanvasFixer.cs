using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class PSD2CanvasFixer : MonoBehaviour
{
#if UNITY_EDITOR
    [ContextMenu("Fix")]
    void tryFix()
    {
        Undo.RegisterFullObjectHierarchyUndo(this.gameObject, "Fix");
        for (int i = 0; i < this.transform.childCount; i++)
        {
            var g = this.transform.GetChild(i).gameObject;
            var sr = g.GetComponent<SpriteRenderer>();
            var sprite = sr.sprite;
            var img = g.AddComponent<Image>();
            img.sprite = sprite;
       
            g.name.Replace(" #1", "");
            DestroyImmediate(sr);
            
            //t.gameObject.AddComponent<RectTransform>();
        }
    }
#endif
}
