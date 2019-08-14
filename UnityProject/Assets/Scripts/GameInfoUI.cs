using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameInfoUI : MonoBehaviour
{
    static XUSingleTown<GameInfoUI> _InstanceHelper = new XUSingleTown<GameInfoUI>();
    public static GameInfoUI Instance => _InstanceHelper.instance;

    public Text titleText;
    public Text creditText;
    public Text descriptionText;

    public GameObject tipsHeaderBar;
    [UnityEngine.Serialization.FormerlySerializedAs("instructionsText")]
    public Text tipsText;
    

    [Space(10)]

    public RawImage _overrideIntructionsImageImage;
    public GameObject _defaultInstructionsContainer;

    [System.Serializable]
    public struct ButtonGUIRefs
    {
        public Text label;
        public Image fill;
    }

    public ButtonGUIRefs[] buttonDisplays;
    public Image joystickFill;
    public Text joystickLabel;

    GameData _cachedCurrentGameData = null;

    private void Start()
    {
        
    }

    private void Update()
    {
        bool gameChanged = _cachedCurrentGameData != MenuVisualsGeneric.Instance.currentlySelectedGame && MenuVisualsGeneric.Instance.currentlySelectedGame != null;
        if (gameChanged)
        {
            _cachedCurrentGameData = MenuVisualsGeneric.Instance.currentlySelectedGame;

            bool hasOverrideInstructionsImage = _cachedCurrentGameData.overrideInstructionsImage != null;
            if (hasOverrideInstructionsImage)
            {
                _overrideIntructionsImageImage.texture = _cachedCurrentGameData.overrideInstructionsImage;
            }
            _overrideIntructionsImageImage.gameObject.SetActive(hasOverrideInstructionsImage);
            _defaultInstructionsContainer.SetActive(!hasOverrideInstructionsImage);


            this.titleText.text = _cachedCurrentGameData.title;
            this.creditText.text = _cachedCurrentGameData.designers;
            this.descriptionText.text = _cachedCurrentGameData.description;

            this.tipsText.text = _cachedCurrentGameData.howToPlay;
            bool tipsTextOn = _cachedCurrentGameData.howToPlay != string.Empty;
            this.tipsText.enabled = tipsTextOn;
            tipsHeaderBar.SetActive(tipsTextOn);


            if (joystickLabel != null)
            {
                this.joystickLabel.text = _cachedCurrentGameData.instructions.joystickInstructions;
                this.joystickFill.enabled = !string.IsNullOrEmpty(_cachedCurrentGameData?.instructions?.joystickInstructions);
            }


            for (int i = 0; i < 6; i++)
            {
                if (buttonDisplays[i].label != null)
                {
                    buttonDisplays[i].label.text = _cachedCurrentGameData.instructions.getButtonLabel(i + 1); //button label 0 used for joystick?
                }

                if (buttonDisplays[i].fill != null)
                {
                    buttonDisplays[i].fill.enabled = !string.IsNullOrEmpty(_cachedCurrentGameData.instructions.getButtonLabel(i + 1));
                }
            }

        }
    }

}
