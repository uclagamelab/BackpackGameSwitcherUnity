using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class GameInfoUI : MonoBehaviour
{
    static XUSingleTown<GameInfoUI> _InstanceHelper = new XUSingleTown<GameInfoUI>();
    public static GameInfoUI Instance => _InstanceHelper.instance;

    [SerializeField]
    Text titleText;
    [SerializeField]
    Text creditText;

    [SerializeField]
    TitleTabFancy _titleTab;

    [SerializeField]
    Text descriptionText;

    [SerializeField]
    RectTransform descriptionColumn;
    [SerializeField]
    Rect descriptionColumnNoControls = default;
    Rect descriptionColumnDefault = default;

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
        descriptionColumnDefault.position = descriptionColumn.offsetMin;
        descriptionColumnDefault.size = descriptionColumn.offsetMax;
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
           
            _titleTab.UpdateWithGame(_cachedCurrentGameData);

            this.titleText.text = _cachedCurrentGameData.title;
            this.creditText.text = _cachedCurrentGameData.designers;
            if (!string.IsNullOrEmpty(_cachedCurrentGameData.year))
            {
                this.creditText.text = this.creditText.text + " (" + _cachedCurrentGameData.year + ")";
            }
            this.descriptionText.text = _cachedCurrentGameData.description;

            this.tipsText.text = _cachedCurrentGameData.howToPlay;
            bool tipsTextOn = _cachedCurrentGameData.howToPlay != string.Empty;
            this.tipsText.enabled = tipsTextOn;
            tipsHeaderBar.SetActive(tipsTextOn);


            bool hasAnyValidControlText = false;
            
            if (joystickLabel != null)
            {
                var instructionsText = _cachedCurrentGameData.instructions.joystickInstructions;
                bool hasJoystickStruction = !string.IsNullOrEmpty(_cachedCurrentGameData?.instructions?.joystickInstructions);
                
                this.joystickLabel.text = instructionsText;
                
                this.joystickFill.gameObject.SetActive (hasJoystickStruction);
                hasAnyValidControlText |= hasJoystickStruction;
            }
    
            for (int i = 0; i < 6; i++)
            {
                var instructionsText = _cachedCurrentGameData.instructions.getButtonLabel(i + 1);
                hasAnyValidControlText |= !string.IsNullOrEmpty(instructionsText);

                if (buttonDisplays[i].label != null)
                {
                    buttonDisplays[i].label.text = instructionsText; //button label 0 used for joystick?
                }

                if (buttonDisplays[i].fill != null)
                {
                    buttonDisplays[i].fill.gameObject.SetActive(!string.IsNullOrEmpty(instructionsText));   
                }
            }

            bool hasControls = hasOverrideInstructionsImage || hasAnyValidControlText;
            this.positionInstructionsColumn(hasControls);
        }
    }

    //Centers the instruction column and disables the joystick display if the game doesn't have any instructions.
    void positionInstructionsColumn(bool hasControLabels)
    {
        var r = !hasControLabels ? descriptionColumnNoControls : descriptionColumnDefault;
        descriptionColumn.offsetMin = r.position;
        descriptionColumn.offsetMax = r.size;
        _overrideIntructionsImageImage.transform.parent.gameObject.SetActive(!hasControLabels);
    }


    [System.Serializable]
    class TitleTabFancy
    {
        StringBuilder _stringBuilder = new StringBuilder(256);
        [SerializeField]
        TextMeshProUGUI _titleText;
        [SerializeField]
        TextMeshProUGUI _designerText;
        [SerializeField]
        SizeByText _sizer;

        internal void UpdateWithGame(GameData gd)
        {
            _titleText.text = gd.title;

            _stringBuilder.Clear();
            _stringBuilder.Append(gd.designers);
            if (!string.IsNullOrEmpty(gd.year))
            {
                if (_stringBuilder.Length > 0)
                {
                    _stringBuilder.Append(' ');
                }
                _stringBuilder.Append('(');
                _stringBuilder.Append(gd.year);
                _stringBuilder.Append(')');
            }
     
            _designerText.SetText(_stringBuilder);
            _designerText.gameObject.SetActive(_stringBuilder.Length != 0);
            _sizer.ForceUpdate();
        }

  
    }
}
