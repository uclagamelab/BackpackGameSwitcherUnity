using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnityLaunchSettingsEditor : MonoBehaviour
{
    [SerializeField]
    Toggle _hasResolutionDialogToggle;
    // Start is called before the first frame update


    private void OnEnable()
    {
        if (GameInfoEditor.instance.currentSelectedGame != null)
        {
            _hasResolutionDialogToggle.isOn = GameInfoEditor.instance.currentSelectedGame.launchSettings.unityStartupOptions.hasResolutionSetupScreen;
        }
    }

    public void ApplyChangesToInMemoryGameData()
    {
        if (GameInfoEditor.instance.currentSelectedGame != null)
        {
            GameInfoEditor.instance.currentSelectedGame.launchSettings.unityStartupOptions.hasResolutionSetupScreen = _hasResolutionDialogToggle.isOn;
        }
    }
}
