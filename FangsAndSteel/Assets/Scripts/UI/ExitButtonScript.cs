using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExitButtonScript : MonoBehaviour
{
    Button button;
    public void GoInMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        SceneManager.UnloadSceneAsync("SampleScene");
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
