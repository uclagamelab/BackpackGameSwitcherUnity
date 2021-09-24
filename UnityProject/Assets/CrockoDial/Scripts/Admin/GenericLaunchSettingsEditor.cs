using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GenericLaunchSettingsEditor : MonoBehaviour
{
    [SerializeField]
    InputField _commandArgsInputField;


    private void OnEnable()
    {
        if (GameInfoEditor.instance.currentSelectedGame != null)
        {
            _commandArgsInputField.text = GameInfoEditor.instance.currentSelectedGame.launchSettings.genericStartupOptions.commandLineArguments;
        }
    }

    public void ApplyChangesToInMemoryGameData()
    {
        if (GameInfoEditor.instance.currentSelectedGame != null)
        {
            GameInfoEditor.instance.currentSelectedGame.launchSettings.genericStartupOptions.commandLineArguments = _commandArgsInputField.text;
        }
    }
}
