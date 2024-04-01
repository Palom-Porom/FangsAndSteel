using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoPursuitButtonPush : MonoBehaviour
{
    Button AutoPursuitButton;
    public GameObject HidePursuitPanelPanel;
    public GameObject EnemyListPanel;
    public Button EnemyListButton;
    public void ActivateAutoPursuit()
    {
        if (AutoPursuitButton.image.color == Color.green)
        { AutoPursuitButton.image.color = Color.red; }
        else if (AutoPursuitButton.image.color == Color.red)
        {AutoPursuitButton.image.color = Color.green; }

        HidePursuitPanelPanel.SetActive(!HidePursuitPanelPanel.activeSelf);
        if (EnemyListPanel.activeSelf) 
        { 
            EnemyListPanel.SetActive(false);
            EnemyListButton.image.color = Color.red; 
        }
    }

    private void Awake()
    {
        AutoPursuitButton = GetComponent<Button>();
        AutoPursuitButton.onClick.AddListener(ActivateAutoPursuit);
    }

    private void OnDestroy()
    {
        AutoPursuitButton.onClick.RemoveListener(ActivateAutoPursuit);
    }
}
