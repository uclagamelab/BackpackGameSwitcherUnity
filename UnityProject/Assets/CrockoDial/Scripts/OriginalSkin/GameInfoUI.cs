﻿using System.Collections;
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

    IInstructionsDisplayer[] _instructionsDisplayers = null;

    [System.Serializable]
    public struct ButtonGUIRefs
    {
        public Text label;
        public Image fill;
    }



    GameData _cachedCurrentGameData = null;

    private void Start()
    {
        descriptionColumnDefault.position = descriptionColumn.offsetMin;
        descriptionColumnDefault.size = descriptionColumn.offsetMax;
        _instructionsDisplayers = this.GetComponentsInChildren<IInstructionsDisplayer>(true);
    }

    bool _gameNeedsRefresh = false;
    public void SetGame(GameData game)
    {
        _gameNeedsRefresh = true;
         //_cachedCurrentGameData != MenuVisualsGeneric.Instance.currentlySelectedGame && MenuVisualsGeneric.Instance.currentlySelectedGame != null;
        
        if (_gameNeedsRefresh)
        {
            _cachedCurrentGameData = MenuVisualsGeneric.Instance.currentlySelectedGame;
            
     
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

            IInstructionsDisplayer bestHandler  = null;
            int bestHandlerPriority = -1;
            foreach (var handler in _instructionsDisplayers)
            {
                int priority = handler.IsHandlerFor(_cachedCurrentGameData);
                if (priority > 0 && priority > bestHandlerPriority)
                {
                    bestHandler = handler;
                    bestHandlerPriority = priority;
                }
            }

            foreach (var handler in _instructionsDisplayers)
            {
                handler.gameObject.SetActive(handler == bestHandler);
            }

            bool hasControls = false;
            if (bestHandler != null)
            {
                hasControls = bestHandler.ShowGame(_cachedCurrentGameData);
                if (!hasControls) bestHandler.gameObject.SetActive(false);
            }

            this.positionInstructionsColumn(hasControls);
        }
    }

    //Centers the instruction column and disables the joystick display if the game doesn't have any instructions.
    void positionInstructionsColumn(bool hasControLabels)
    {
        var r = !hasControLabels ? descriptionColumnNoControls : descriptionColumnDefault;
        descriptionColumn.offsetMin = r.position;
        descriptionColumn.offsetMax = r.size;
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
