using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleTab : MonoBehaviour {

    CanvasGroup canvasGroup;
    private void Awake()
    {
        canvasGroup = this.GetComponent<CanvasGroup>();
    }

    // Update is called once per frame
    void Update () {
        float targetOpacity = MenuVisualsGeneric.Instance.state == MenuVisualsGeneric.MenuState.ChooseGame ? 1 : 0;

        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetOpacity, Time.deltaTime * 3);

    }
}
