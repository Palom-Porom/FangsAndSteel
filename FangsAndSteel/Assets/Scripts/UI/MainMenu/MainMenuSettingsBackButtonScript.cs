using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuSettingsBackButtonScript : MonoBehaviour
{
    Button button;
    public GameObject settingsWindow;
   
    public void CloseSettingsWindow()
    {
        settingsWindow.SetActive(false);
    }

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(CloseSettingsWindow);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(CloseSettingsWindow);
    }
}
