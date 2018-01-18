using UnityEngine;
using System.Collections;

public class BGImage : MonoBehaviour {
	Vector3 startPos;

	// Use this for initialization
	void Start () {
		startPos = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = new Vector3(startPos.x + Random.Range (-2,2), startPos.y + Random.Range(-2,2), startPos.z);
	}
}
