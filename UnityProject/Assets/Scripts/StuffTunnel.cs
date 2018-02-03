using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StuffTunnel : MonoBehaviour {

    float lastBlipBack = float.NegativeInfinity;

    Sprite[] baseSprites;

    List<GameObject> allDebris;

	// Use this for initialization
	void Awake () {
        baseSprites = Resources.LoadAll<Sprite>("PixelIcons");

        allDebris = new List<GameObject>();

        int nDebris = baseSprites.Length;
        for (int i = 0; i < nDebris; i++)
        {
            GameObject newDrifty = new GameObject();
            newDrifty.transform.parent = this.transform.parent;
            newDrifty.AddComponent<SpriteRenderer>();
            newDrifty.GetComponent<SpriteRenderer>().sprite = baseSprites[i % baseSprites.Length];
            newDrifty.AddComponent<Drifter>();
            newDrifty.transform.localScale = Vector3.one * 2.54f;
            this.allDebris.Add(newDrifty);
            newDrifty.transform.position = this.transform.position - 2 * this.transform.forward;
            newDrifty.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
        }
    }
	
	// Update is called once per frame
	void Update () {

        this.transform.eulerAngles = new Vector3(
            Mathf.Lerp(-20, 20, Mathf.PerlinNoise(.2003f * Time.time, .2f * 1.01747f * Time.time)), 
            Mathf.Lerp(-20, 20, Mathf.PerlinNoise(.2f * 1.018f * Time.time, .2001f * Time.time)),  
            Mathf.Lerp(-280, 280, Mathf.PerlinNoise(.05f * Time.time, .05f * 1.01037f * Time.time)));

		foreach(GameObject boj in this.allDebris)
        {
            Vector3 dirr = boj.GetComponent<Drifter>().dir;
            boj.transform.position += dirr * Time.deltaTime * 30.0f;
            
            float rspedd= Mathf.PerlinNoise(dirr.x - dirr.z, dirr.y + dirr.z);
            rspedd = Mathf.Lerp(-300, 300, rspedd);
            rspedd = ((int)rspedd) % 2 == 0 ? rspedd : -rspedd;

            boj.transform.Rotate(Vector3.forward,Time.deltaTime * rspedd, Space.Self);

            boj.GetComponent<SpriteRenderer>().color = Color.Lerp(boj.GetComponent<SpriteRenderer>().color, Color.white, 2*Time.deltaTime);
            //boj.transform.LookAt(this.transform.position + this.transform.forward *1);
            if (boj.transform.position.z < this.transform.position.z)
            {

                if (Time.time > lastBlipBack + .1f)
                {
                    lastBlipBack = Time.time;
                    boj.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);


                    Vector3 dir = -this.transform.forward;
                    dir.x += Random.Range(-.2f, .2f);
                    dir.y += Random.Range(-.2f, .2f);


                    Vector3 decelDir = boj.GetComponent<Drifter>().dir;
                    float mag = decelDir.magnitude;
                    mag = Mathf.Lerp(mag, .1f, (1 - (this.transform.position.z - boj.transform.position.z)/2));//Mathf.MoveTowards(mag, .5f, Time.deltaTime);
                    decelDir = decelDir.normalized * mag;
                    //boj.GetComponent<Drifter>().dir = decelDir;

                    boj.GetComponent<Drifter>().dir = dir;
                    boj.transform.up = dir;
                    boj.transform.position = this.transform.position + this.transform.forward * Random.Range(100, 150) + (Vector3.Scale(new Vector3(Screen.width/ Screen.height, 1, 0) ,Quaternion.Euler(0,0,Random.Range(0,360)) * Vector3.right) * Random.Range(15, 60f));

                    boj.transform.LookAt(this.transform);
                    //boj.transform.rotation = Quaternion.RotateTowards(boj.transform.rotation, Random.rotation, 45);

                    Vector3 lookAtPt = Vector3.zero;
                    lookAtPt.z = boj.transform.position.z;
                    
                }
            }
        }
	}
}

public class Drifter : MonoBehaviour
{
    public Vector3 dir;
}
