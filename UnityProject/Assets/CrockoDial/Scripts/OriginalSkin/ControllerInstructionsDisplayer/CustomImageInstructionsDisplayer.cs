using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomImageInstructionsDisplayer : MonoBehaviour, IInstructionsDisplayer
{
    RawImage _overrideIntructionsRawImage;

    void Awake()
    {
        _overrideIntructionsRawImage = GetComponent<RawImage>();
    }

    int IInstructionsDisplayer.IsHandlerFor(GameData gameData, GameData.DisplayedControls controls)
    {
        //TODO: preferentially take an explicitly specified overrride instruction image?
        return controls != GameData.DisplayedControls.none && gameData.genericOverrideInstructionImage != null ? 5 : -1;
    }

    bool IInstructionsDisplayer.ShowGame(GameData game)
    {
        _overrideIntructionsRawImage.texture = game.genericOverrideInstructionImage;
        return game.genericOverrideInstructionImage;
    }
}
