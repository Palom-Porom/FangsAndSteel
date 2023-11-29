using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayButtonScript : MonoBehaviour
{
    Button button;
    public void PlayGame()
    {
        //SceneManager.LoadScene("SampleScene");
        //SceneManager.UnloadSceneAsync("MainMenu");
        SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
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
