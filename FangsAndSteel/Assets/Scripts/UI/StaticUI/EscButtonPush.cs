using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class EscButtonPush : MonoBehaviour
{
    public static bool gameIsPaused = false;
    public GameObject gameActiveUI;
    public GameObject escUI;
    public GameObject settingsWindow;
    
    private void Awake()
    {
        ControlSystem.controlsAssetClass.Game.OpenCloseEscMenu.performed += OpenCloseMenu;
    }

    private void OnDestroy()
    {
        ControlSystem.controlsAssetClass.Game.OpenCloseEscMenu.performed -= OpenCloseMenu;
    }

    public void OpenCloseMenu(InputAction.CallbackContext context) => OpenCloseMenu();
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