using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FullScreenButton : MonoBehaviour
{
    public Button fullScreenButton;
    public void ChangeScreenMode()
    {
        Screen.fullScreen = !Screen.fullScreen;
        fullScreenButton.transform.GetChild(0).gameObject.SetActive(!fullScreenButton.transform.GetChild(0).gameObject.activeSelf);
    }

    private void Awake()
    {
        fullScreenButton = GetComponent<Button>();
        fullScreenButton.onClick.AddListener(ChangeScreenMode);
    }

    private void OnDestroy()
    {
        fullScreenButton.onClick.RemoveAllListeners();
    }

}
