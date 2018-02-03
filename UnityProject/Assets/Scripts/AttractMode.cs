using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttractMode : MonoBehaviour {


    enum AttractState
    {
        IconBlizzard,
        RandomVideo
    }

    float attractStateChangeTime = float.PositiveInfinity;

    AttractState state = AttractState.IconBlizzard;
    [SerializeField]
    UnityEngine.UI.Image blotter;

    float _changeTime = -1;
    public float changeTime
    {
        get { return _changeTime; }
        protected set { _changeTime = value; }
    }

    public static AttractMode Instance;

    GameObject container;
    [SerializeField]
    GameObject iconBlizzardContainer;


	// Use this for initialization
	void Awake () {
        Instance = this;
        container = this.transform.GetChild(0).gameObject;
    }

    private void Start()
    {
        handleStateChange();
    }

    public bool running
    {
        get
        {
            return container.activeSelf;// && Time.time > activationTime + .5f;
        }

        set
        {
            if (value)
            {
                //schedule a switch
                state = AttractState.IconBlizzard;
                scheduleAttractTypeTransition();
                handleStateChange();
            }

            if (container.activeSelf != value)
            {
                changeTime = Time.time;

                container.SetActive(value);

            }
        }

    }
	
    void scheduleAttractTypeTransition()
    {
        float duration = state == AttractState.IconBlizzard ? Random.Range(8, 30) : Random.Range(8, 30.0f);
        attractStateChangeTime = Time.time + duration;
    }

    void handleStateChange()
    {
        bool blizzardOn =  state == AttractState.IconBlizzard;
        if (blizzardOn)
        {
            this.iconBlizzardContainer.SetActive(true);
            BackgroundDisplay.Instance.visible = false;
        }
        else //if (!blizzardOn && this.iconBlizzardContainer.activeSelf)
        {
            BackgroundDisplay.Instance.visible = true;
            this.iconBlizzardContainer.SetActive(false);         
        }
       
        


    }

	// Update is called once per frame
	void Update () {
        if (running)
        {
            if(Time.time > attractStateChangeTime)
            {

                state =
                    (state == AttractState.IconBlizzard) ?
                        AttractState.RandomVideo
                        :
                        AttractState.IconBlizzard;

                if (state == AttractState.RandomVideo)
                {
                    GameObject.FindObjectOfType<MenuVisualsGeneric>().cycleToNextGame(-1, true);
                }
                

                handleStateChange();
                scheduleAttractTypeTransition();
            }

          

        }
	}
}
