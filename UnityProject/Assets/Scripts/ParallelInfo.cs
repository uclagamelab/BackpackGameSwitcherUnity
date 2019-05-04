using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallelInfo : MonoBehaviour {

    [SerializeField]
    Texture testInfoOverlay;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public Texture GetGameInfoOverlay()
    {
        return testInfoOverlay;
    }
}
