using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResumeButtonScript : MonoBehaviour
{
    Button button;
    public GameObject escMenu;
    public GameObject gameOnUI;
    public void Resume()
    {
        escMenu.SetActive(false);
        EscButtonPush.gameIsPaused = false;
        gameOnUI.SetActive(true);
        
    }
    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(Resume);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(Resume);
    }
}
