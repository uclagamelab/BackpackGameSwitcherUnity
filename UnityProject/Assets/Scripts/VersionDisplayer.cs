using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VersionDisplayer : MonoBehaviour
{
    void Start()
    {
        this.GetComponent<Text>().text = "v"+Application.version;   
    }


}
