using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExitFromApplicationButtonScript : MonoBehaviour
{
    Button button;
    public void PlayGame()
    {
        Application.Quit();
    }

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(PlayGame);

    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(PlayGame);
    }
}
