using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameInfoUI : MonoBehaviour {


    public Text titleText;
    public Text creditText;
    public Text descriptionText;

    public Text[] buttonLabels;
    public Text joystickLabel;

    GameData _cachedCurrentGameData = null;

    private void Start()
    {
        
    }

    private void Update()
    {
        if (_cachedCurrentGameData != MenuVisualsGeneric.Instance.currentlySelectedGame && MenuVisualsGeneric.Instance.currentlySelectedGame != null)
        {
            _cachedCurrentGameData = MenuVisualsGeneric.Instance.currentlySelectedGame;

            this.titleText.text = _cachedCurrentGameData.title;
            this.creditText.text = _cachedCurrentGameData.author;
            this.descriptionText.text = _cachedCurrentGameData.description;

            if (joystickLabel != null)
            {
                this.joystickLabel.text = _cachedCurrentGameData.joystickLabel;
            }
          
            for(int i = 0; i < 6; i++)
            {
                if (buttonLabels[i] != null)
                {
                    buttonLabels[i].text = _cachedCurrentGameData.getButtonLabel(i+1); //button label 0 used for joystick?
                }
            }

        }
    }

}
