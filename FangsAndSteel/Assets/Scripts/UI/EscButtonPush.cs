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
        //escPush = new ControlsAsset();
        //escPush.Game.OpenCloseEscMenu.performed += context => OpenCloseMenu();
        ControlSystem.controlsAssetClass.Game.OpenCloseEscMenu.performed += OpenCloseMenu;
    }

    private void OnDestroy()
    {
        ControlSystem.controlsAssetClass.Game.OpenCloseEscMenu.performed -= OpenCloseMenu;
    }

    public void OpenCloseMenu(InputAction.CallbackContext context) => OpenCloseMenu();
    public void OpenCloseMenu()
    {
        if (settingsWindow == null)
        {
            Debug.Log("Settings window is null");
            return;
        }
        if (settingsWindow.activeSelf) { settingsWindow.SetActive(!settingsWindow.activeSelf); }
        else
        {
            escUI.SetActive(!escUI.activeSelf);
            gameActiveUI.SetActive(!gameActiveUI.activeSelf);
        }
    }

}
