using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackButtonScript : MonoBehaviour
{
    Button button;
    public GameObject settingsWindow;
    public GameObject mainButtons;
    public void CloseSettingsWindow()
    {
        settingsWindow.SetActive(false);
        mainButtons.SetActive(true);
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
