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
        if (controls == GameData.DisplayedControls.none)
        {
            return -1;
        }
        else
        {
            if (gameData.hasCustomInstructionImage(controls) || gameData.genericOverrideInstructionImage != null)
            {
                return 50;
            }

            return -1;
        }
    }

    bool IInstructionsDisplayer.ShowGame(GameData game)
    {
        //#2 preference has generic image
        Texture2D finalTexture = game.genericOverrideInstructionImage;
        
        //#1 preference has control specific image
        //TODO: ugh, need to factor out determination of environment, override etc... for this.
        GameData.DisplayedControls controlType = GameInfoUI.GetAppropriateInstructionDisplayType(game);
        
        if (game.hasCustomInstructionImage(controlType))
        {
            finalTexture = game.GetCustomInstructionImage(controlType);
        }

        _overrideIntructionsRawImage.texture = finalTexture;
        return game.genericOverrideInstructionImage;
    }
}
