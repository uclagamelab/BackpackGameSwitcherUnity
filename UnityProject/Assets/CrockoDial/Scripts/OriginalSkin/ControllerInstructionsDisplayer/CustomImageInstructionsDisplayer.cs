using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomImageInstructionsDisplayer : MonoBehaviour, IInstructionsDisplayer
{
    RawImage _overrideIntructionsImageImage;

    void Awake()
    {
        _overrideIntructionsImageImage = GetComponent<RawImage>();
    }

    int IInstructionsDisplayer.IsHandlerFor(GameData gameData)
    {
        return gameData.overrideInstructionsImage != null ? 9999 : -1;
    }

    bool IInstructionsDisplayer.ShowGame(GameData game)
    {
        _overrideIntructionsImageImage.texture = game.overrideInstructionsImage;
        return game.overrideInstructionsImage;
    }
}
