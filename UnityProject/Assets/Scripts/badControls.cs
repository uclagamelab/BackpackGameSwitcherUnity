using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class badControls : MonoBehaviour {

    public string upKey;
    public string downKey;
    public string leftKey;
    public string rightKey;
    public string button1Key;
    public string button2Key;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float speed = 2;
        if (Input.GetKey(upKey))
        {
            this.transform.position += Vector3.up * Time.deltaTime * speed;
        }

        if (Input.GetKey(downKey))
        {
            this.transform.position -= Vector3.up * Time.deltaTime * speed;
        }

        if (Input.GetKey(rightKey))
        {
            this.transform.position += Vector3.right * Time.deltaTime * speed;
        }

        if (Input.GetKey(leftKey))
        {
            this.transform.position -= Vector3.right * Time.deltaTime * speed;
        }

        if (Input.GetKeyDown(button1Key))
        {
            this.GetComponent<Renderer>().material.color = Color.red;
            this.delayedFunction(() => { this.GetComponent<Renderer>().material.color = Color.white; }, .25f);
            //this.varyWithT()
        }

        if (Input.GetKeyDown(button2Key))
        {
            this.GetComponent<Renderer>().material.color = Color.blue;
            this.delayedFunction(() => { this.GetComponent<Renderer>().material.color = Color.white; }, .25f);
            //this.varyWithT()
        }
    }
}
