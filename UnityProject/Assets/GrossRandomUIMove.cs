using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrossRandomUIMove : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        this.GetComponent<RectTransform>().anchoredPosition = 
            Vector3.Scale(
            (new Vector3(this.ezPerlin(Time.time * .2f, 9), this.ezPerlin(Time.time * .2f, 3), 0) - .5f * Vector3.one)
            ,
            new Vector3(200, 100, 0)
            );

        this.transform.eulerAngles += Vector3.forward * Time.deltaTime * 10;
	}

}
