using System.Collections;
using System.Collections.Generic;
using System.Web;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class XboxControllerInstructionsDesc
{
    public string stick_l, stick_r, d_pad, a, b, x, y, start, select, shoulder_l, shoulder_r, trigger_l, trigger_r;
}

public class XboxControllerInstructionDisplayer : MonoBehaviour
{
    public XboxButton dbKey = XboxButton.stick_l;

    public string txt;
    [ContextMenu("try")]
    void test()
    {
        SetInstruction(dbKey, txt);
    }

    public enum XboxButton
    {
         stick_l, stick_r, d_pad, a, b, x, y, start, select, shoulder_l, shoulder_r, trigger_l, trigger_r   
    }

    [System.Serializable]
    class InstructionPart 
    {
        public Image img;
        public TMPro.TextMeshProUGUI actionText;
        public GameObject pointer;
        //MaterialPropertyBlock mpb;
        Material dynamicMat;
        public bool isHighlighted = false;

        public void Clear()
        {
            setHighlight(false);
        }

        public void SetInstructions(string instructions)
        {
            if (dynamicMat == null)
            {
                dynamicMat = new Material(img.material);
                dynamicMat.CopyMatchingPropertiesFromMaterial(img.material);
                img.material = dynamicMat;
            }

            bool on = !string.IsNullOrEmpty(instructions);
            setHighlight(on);
            if (actionText != null)
            {
                actionText.text = on ? instructions : "";
            }
            if (pointer != null) 
            {
                pointer.SetActive(on);
            }
        }


        void setHighlight (bool highlight)
        {
            isHighlighted = highlight;
            dynamicMat.SetFloat("_Hue", isHighlighted ? 1 : 0);
        }
    }

    [SerializeField] InstructionPart[] parts = { };

    
    void Start()
    {
        
    }

    public void SetInstruction(XboxButton btn, string instructions)
    {
        parts[(int)btn].SetInstructions(instructions);
    }

    [ContextMenu("populate")]
    void populate()
    {
        var allE = Enumz.AllValues<XboxButton>();
        parts = new InstructionPart[allE.Length];
        int i = -1;
        foreach (var e in allE)
        {
            i++;
            parts[i] = new();
            parts[i].img = this.transform.Find("" + ((XboxButton) i))?.GetComponent<Image>();
        }
    }
}

public class UGLMaterialModifier : MonoBehaviour, IMaterialModifier
{
    List<FloatProp> floatProps = new List<FloatProp>();
    public class FloatProp
    {
        int propId;
        float value;

        public FloatProp(string id, float defaultValue = 0)
        {
            propId = Shader.PropertyToID(id);
            value = defaultValue;
        }

        public void Apply(Material mat)
        {
            mat.SetFloat(propId, value);
        }
    }
    
    Material IMaterialModifier.GetModifiedMaterial(Material baseMaterial)
    {
        foreach(var prop in floatProps)
        {

        }
        return baseMaterial;
    }
}