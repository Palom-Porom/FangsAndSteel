using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsButtonScript : MonoBehaviour
{
    Button button;
    public GameObject settingsWindow;
    public GameObject mainButtons;

    public void OpenSettingWindow()
    {
        settingsWindow.SetActive(true);
    }

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OpenSettingWindow);

    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(OpenSettingWindow);
    }
}
