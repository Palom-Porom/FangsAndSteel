using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class EscButtonPush : MonoBehaviour
{
    public static bool gameIsPaused = false;
    public GameObject gameActiveUI;
    public GameObject escUI;
    public GameObject settingsWindow;
    private ControlsAsset escPush;
    
    private void Awake()
    {
        escPush = new ControlsAsset();
        escPush.Game.OpenCloseEscMenu.performed += context => OpenCloseMenu();
    }

    private void OnEnable()
    {
        escPush.Enable();    
    }

    private void OnDisable()
    {
        escPush.Disable();
    }

    public void OpenCloseMenu()
    {
        if (settingsWindow.activeSelf) { settingsWindow.SetActive(!settingsWindow.activeSelf); }
        else
        {
            escUI.SetActive(!escUI.activeSelf);
            gameActiveUI.SetActive(!gameActiveUI.activeSelf);
        }
     }


}
