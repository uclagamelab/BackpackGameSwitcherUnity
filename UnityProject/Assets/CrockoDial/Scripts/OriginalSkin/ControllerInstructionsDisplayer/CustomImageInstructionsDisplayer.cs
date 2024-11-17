using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomImageInstructionsDisplayer : MonoBehaviour, IInstructionsDisplayer
{
    RawImage _overrideIntructionsRawImage;
    GameData _lastShownGame = null;

    void Awake()
    {
        _overrideIntructionsRawImage = GetComponent<RawImage>();
        GameData.OnGameDataImageLoad += OnGameDataImageLoad;
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

    public bool ShowGame(GameData game)
    {
        _lastShownGame = game;
        //#2 preference has generic image
        Texture2D finalTexture = game.genericOverrideInstructionImage;
        
        //#1 preference has control specific image
        //TODO: ugh, need to factor out determination of environment, override etc... for this.
        GameData.DisplayedControls controlType = GameInfoUI.GetAppropriateInstructionDisplayType(game);
        
        if (game.hasCustomInstructionImage(controlType))
        {
            finalTexture = game.GetCustomInstructionImage(controlType);
        }

        _overrideIntructionsRawImage.enabled = finalTexture != null;
        _overrideIntructionsRawImage.texture = finalTexture;
        return game.genericOverrideInstructionImage;
    }

    void OnGameDataImageLoad(GameData game)
    {
        if (this.isActiveAndEnabled && game == _lastShownGame)
        {
            ShowGame(game);
        }
    }
}
