using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttractMode : MonoBehaviour {

    public static AttractMode Instance;
    float timeOfLastInput = float.PositiveInfinity;
    float attractTimeOut = 60;
    GameObject container;
	// Use this for initialization
	void Awake () {
        Instance = this;
        container = this.transform.GetChild(0).gameObject;
    }

    public bool running
    {
        get
        {
            return container.activeSelf;
        }

        set
        {
            if (container.activeSelf != value)
            {
                container.SetActive(value);
            }
        }

    }
	
	// Update is called once per frame
	void Update () {

	}
}
