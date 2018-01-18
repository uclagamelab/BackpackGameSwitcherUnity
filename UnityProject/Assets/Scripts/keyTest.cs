using UnityEngine;
using System.Collections;

public class keyTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		print(Input.GetKey(KeyCode.A));
	}
}
