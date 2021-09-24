using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebsiteLaunchSettingsEditor : MonoBehaviour
{
    [SerializeField]
    InputField _urlInputField;

    private void OnEnable()
    {
        if (GameInfoEditor.instance.currentSelectedGame != null)
        {
            _urlInputField.text = GameInfoEditor.instance.currentSelectedGame.launchSettings.websiteStartupOptions.url;
        }
    }

    public void ApplyChangesToInMemoryGameData()
    {
        if (GameInfoEditor.instance.currentSelectedGame != null)
        {
            GameInfoEditor.instance.currentSelectedGame.launchSettings.websiteStartupOptions.url = _urlInputField.text;
        }
    }

}
