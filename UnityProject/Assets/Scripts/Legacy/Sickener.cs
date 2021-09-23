using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class Sickener : MonoBehaviour {

    Twirl[] twirls;
    float robustTime = 0;
    private void OnEnable()
    {
        robustTime = 0;
    }


    // Use this for initialization
    void Start () {
        twirls = this.GetComponents<Twirl>();//new Twirl[5];
        for (int i = 0; i < twirls.Length; i++)
        {
            //twirls[i] = this.gameObject.AddComponent<Twirl>();
        }

    }
	
	// Update is called once per frame
	void Update () {
        //Twirl twirl1 = this.GetComponent<Twirl>();
        float t = robustTime * .3f;
        for (int i = 0; i < twirls.Length; i++)
        {
            Twirl twirl = twirls[i];
           

            twirl.center = new Vector2(this.ezPerlin(t, (2 * i)), this.ezPerlin(t, (2 * i) + 1));

            twirl.angle = Mathf.Lerp(-35, 35, this.ezPerlin(t, i + 100));
            twirl.radius =  Vector2.one * Mathf.Lerp(.2f, 0.8f, this.ezPerlin(t, i + 50));
        }
        robustTime += Time.deltaTime;
        robustTime = robustTime % (24 * 60 * 60);
    }


}
