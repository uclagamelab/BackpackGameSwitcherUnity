using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttractMode : MonoBehaviour {


    float _changeTime = -1;
    public float changeTime
    {
        get { return _changeTime; }
        protected set { _changeTime = value; }
    }

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
            return container.activeSelf;// && Time.time > activationTime + .5f;
        }

        set
        {
            if (container.activeSelf != value)
            {
                changeTime = Time.time;

                container.SetActive(value);

            }
        }

    }
	
	// Update is called once per frame
	void Update () {
        
	}
}
