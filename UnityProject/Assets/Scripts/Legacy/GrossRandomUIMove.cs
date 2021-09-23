using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrossRandomUIMove : MonoBehaviour {

    float robustTime = 0;
	void Start () {
		
	}

    private void OnEnable()
    {
        robustTime = 0;
    }


    void Update () {
        this.GetComponent<RectTransform>().anchoredPosition = 
            Vector3.Scale(
            (new Vector3(this.ezPerlin(robustTime * .2f, 9), this.ezPerlin(robustTime * .2f, 3), 0) - .5f * Vector3.one)
            ,
            new Vector3(200, 100, 0)
            );

        this.transform.eulerAngles += Vector3.forward * Time.deltaTime * 10;
        robustTime += Time.deltaTime;
        robustTime = robustTime % (24 * 60 * 60);
    }

}
