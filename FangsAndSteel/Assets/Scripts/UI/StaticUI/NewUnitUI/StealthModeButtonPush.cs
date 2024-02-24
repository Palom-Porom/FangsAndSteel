using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StealthModeButtonPush : MonoBehaviour
{
    Button StealthButton;
    public GameObject BattleSettingsWindow;
    public GameObject StealthSettingsWindow;
    public GameObject StaySettingsWindow;
    public void OpenStealthSettingsWindow()
    {
        StealthSettingsWindow.SetActive(true);
        if (BattleSettingsWindow.activeSelf) { BattleSettingsWindow.SetActive(!BattleSettingsWindow.activeSelf); }
        else if (StaySettingsWindow.activeSelf) { StaySettingsWindow.SetActive(!StaySettingsWindow.activeSelf); }
    }
    private void Awake()
    {
        StealthButton = GetComponent<Button>();
        StealthButton.onClick.AddListener(OpenStealthSettingsWindow);
    }

    private void OnDestroy()
    {
        StealthButton.onClick.RemoveListener(OpenStealthSettingsWindow);
    }
}
