using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScreenResolutionDropdown : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    
    void Start()
    {
        Screen.fullScreen = true;
    }


    void Update()
    {

    }

    public void ChangeResolution()
    {
        switch (dropdown.value)
        {
            case 0:
                Screen.SetResolution(2560, 1440, Screen.fullScreen);
                break;

            case 1:
                Screen.SetResolution(1920, 1080, Screen.fullScreen);
                break;
            case 2:
                Screen.SetResolution(1600, 900, Screen.fullScreen);
                break;
            case 3:
                Screen.SetResolution(1536, 864, Screen.fullScreen);
                break;
            case 4:
                Screen.SetResolution(1440, 900, Screen.fullScreen);
                break;
            case 5:
                Screen.SetResolution(1366, 768, Screen.fullScreen);
                break;
            case 6:
                Screen.SetResolution(1280, 720, Screen.fullScreen);
                break;


        }
    }

    
}
