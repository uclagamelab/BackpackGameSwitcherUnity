using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ButtonGUIRefs = GameInfoUI.ButtonGUIRefs;

public interface IInstructionsDisplayer
{
    GameObject gameObject { get; }
    int IsHandlerFor(GameData gameData, GameData.DisplayedControls controls);
    bool ShowGame(GameData game);
}

public class ArcadeControlsDisplay : MonoBehaviour, IInstructionsDisplayer
{
    public ButtonGUIRefs[] buttonDisplays;
    public Image joystickFill;
    public Text joystickLabel;

    public int IsHandlerFor(GameData gameData, GameData.DisplayedControls controls)
    {

        if (controls == GameData.DisplayedControls.arcade)
        {
            return 1;
        }
        else
        {
            return -1;
        }
    }

    public bool ShowGame(GameData game)
    {
        bool hasAnyValidControlText = false;
        if (joystickLabel != null)
        {
            var instructionsText = game.instructions.joystickInstructions;
            bool hasJoystickStruction = !string.IsNullOrEmpty(game?.instructions?.joystickInstructions);

            this.joystickLabel.text = instructionsText;

            this.joystickFill.gameObject.SetActive(hasJoystickStruction);
            hasAnyValidControlText |= hasJoystickStruction;
        }

        for (int i = 0; i < 6; i++)
        {
            var instructionsText = game.instructions.getButtonLabel(i + 1);
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
        return hasAnyValidControlText;
    }
}
