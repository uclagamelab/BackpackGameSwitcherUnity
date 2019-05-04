using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GentleJitterAnim : MonoBehaviour {

    Vector3 localPos;
    float nextUpdateTime = 0;
    public float jitterScale = 5;
    public bool on = true;
	// Use this for initialization
	void Start () {
        localPos = this.transform.localPosition;

    }
	
	// Update is called once per frame
	void Update ()
    {
        if (on)
        {
            nextUpdateTime -= Time.deltaTime;
            if (nextUpdateTime < 0)
            {
                nextUpdateTime = Random.Range(.15f, .25f);
                this.transform.localPosition = localPos + Random.onUnitSphere * jitterScale;
            }
        }
	}
}
