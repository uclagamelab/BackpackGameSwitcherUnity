using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingAnimation : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float dots = (Time.time % 1);
        dots = Mathf.Floor(dots * 4);

        this.GetComponent<Text>().text = "Loading";
        for (int i = 0; i < dots; i++)
        {
            this.GetComponent<Text>().text += ".";
        }
	}
}
