using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaitButtonScript : MonoBehaviour
{
    Button WaitButton;
    public GameObject HideWaitButtonPanel;
    public void WaitButtonPush()
    {
        if (WaitButton.image.color == Color.green)
        { WaitButton.image.color = Color.red; }
        else if (WaitButton.image.color == Color.red)
        { WaitButton.image.color = Color.green; }
        HideWaitButtonPanel.SetActive(!HideWaitButtonPanel.activeSelf);
    }

    private void Awake()
    {
        WaitButton = GetComponent<Button>();
        WaitButton.onClick.AddListener(WaitButtonPush);
    }

    private void OnDestroy()
    {
        WaitButton.onClick.RemoveListener(WaitButtonPush);
    }
}
