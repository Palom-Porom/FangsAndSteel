using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExitButtonScript : MonoBehaviour
{
    Button button;
    public void GoInMainMenu()
    {
        _TurnSystem.timeToRun = 0;
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(GoInMainMenu);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(GoInMainMenu); 
    }
}
