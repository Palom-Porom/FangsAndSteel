using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StayModeButtonPush : MonoBehaviour
{
    Button StayButton;
    public GameObject BattleSettingsWindow;
    public GameObject StealthSettingsWindow;
    public GameObject StaySettingsWindow;
    public void ÑloseOtherSettingsWindows()
    {
        StaySettingsWindow.SetActive(true);
        if (StealthSettingsWindow.activeSelf) { StealthSettingsWindow.SetActive(!StealthSettingsWindow.activeSelf); }
        else if (BattleSettingsWindow.activeSelf) { BattleSettingsWindow.SetActive(!BattleSettingsWindow.activeSelf); }
    }
    private void Awake()
    {
        StayButton = GetComponent<Button>();
        StayButton.onClick.AddListener(ÑloseOtherSettingsWindows);
    }

    private void OnDestroy()
    {
        StayButton.onClick.RemoveListener(ÑloseOtherSettingsWindows);
    }
}
