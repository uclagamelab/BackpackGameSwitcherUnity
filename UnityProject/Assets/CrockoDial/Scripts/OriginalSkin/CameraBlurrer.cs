using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing; 

public class CameraBlurrer : MonoBehaviour
{
    float _blurAmount = 0;
    //public UnityEngine.Rendering.PostProcessing.PostProcessVolume vol;
    PostProcessProfile _ppp;
    DepthOfField _dof;
    private void Start()
    {
        _ppp = ScriptableObject.CreateInstance<PostProcessProfile>();
        _ppp.AddSettings<DepthOfField>();
        _dof = _ppp.GetSetting<DepthOfField>();

        _dof.focusDistance.overrideState = true;
        _dof.focalLength.overrideState = true;
        _dof.aperture.overrideState = true;
        _dof.kernelSize.overrideState = true;

        _dof.focusDistance.value = .1f;
        _dof.aperture.value = 10;
        _dof.kernelSize.value = KernelSize.Medium;
        this.GetComponent<PostProcessVolume>().profile = _ppp;
    }
    public float blurAmount
    {
        get => _blurAmount;
        set
        {
            _blurAmount = value;
            updatePostEffects();
        }
    }

    void updatePostEffects()
    {

        _dof.active = _blurAmount > 0;
        _dof.focalLength.value = Mathf.Lerp(1f, 30f, _blurAmount);
    }
}
