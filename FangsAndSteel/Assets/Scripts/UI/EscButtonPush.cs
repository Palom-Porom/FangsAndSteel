using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EscButtonPush : MonoBehaviour
{
    public static bool gameIsPaused = false;
    public GameObject escMenuUI;
    public GameObject gameActiveUI;
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (gameIsPaused)
            {
                Resume();
            }
            else
            { 
                Pause();
            }
        }
    }

    private void Pause()
    {
        escMenuUI.SetActive(true);
        gameActiveUI.SetActive(false);
        gameIsPaused = true;
    }

    public void Resume()
    { 
        escMenuUI.SetActive(false);
        gameActiveUI.SetActive(true);
        gameIsPaused = false;
    }

    
}
