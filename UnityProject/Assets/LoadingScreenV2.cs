using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreenV2 : MonoBehaviour {

    CanvasGroup canvasGroup;
	// Use this for initialization
	void Start () {
        canvasGroup = this.GetComponent<CanvasGroup>();

    }
	
	// Update is called once per frame
	void Update () {

        float targetOpacity = MenuVisualsGeneric.Instance.state == MenuVisualsGeneric.MenuState.LaunchGame ?1 : 0;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetOpacity, 3 * Time.deltaTime);
    }
}
