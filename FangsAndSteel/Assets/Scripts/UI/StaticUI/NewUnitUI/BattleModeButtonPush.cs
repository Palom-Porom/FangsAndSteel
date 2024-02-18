using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleModeButtonPush : MonoBehaviour
{
    Button BattleButton;
    public GameObject BattleSettingsWindow;
    public GameObject StealthSettingsWindow;
    public GameObject StaySettingsWindow;
    public void OpenBattleSettingsWindow()
    {
        BattleSettingsWindow.SetActive(true);
        if (StealthSettingsWindow.activeSelf) {StealthSettingsWindow.SetActive(!StealthSettingsWindow.activeSelf);}
        else if (StaySettingsWindow.activeSelf) { StaySettingsWindow.SetActive(!StaySettingsWindow.activeSelf); }
    }
    private void Awake()
    {
        BattleButton = GetComponent<Button>();
        BattleButton.onClick.AddListener(OpenBattleSettingsWindow);
    }

    private void OnDestroy()
    {
        BattleButton.onClick.RemoveListener(OpenBattleSettingsWindow);
    }
}
