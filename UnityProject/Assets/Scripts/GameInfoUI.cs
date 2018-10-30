using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameInfoUI : MonoBehaviour {


    public Text titleText;
    public Text creditText;
    public Text descriptionText;

    public GameObject[] buttonLabels;
    public GameObject joystickLabel;

    GameData _cachedCurrentGameData = null;

    private void Start()
    {
        
    }

    private void Update()
    {
        if (_cachedCurrentGameData != MenuVisualsGeneric.Instance.currentlySelectedGame)
        {
            _cachedCurrentGameData = MenuVisualsGeneric.Instance.currentlySelectedGame;

            this.titleText.text = _cachedCurrentGameData.title;
            this.creditText.text = _cachedCurrentGameData.author;
            this.descriptionText.text = _cachedCurrentGameData.description;
        }
    }

}
