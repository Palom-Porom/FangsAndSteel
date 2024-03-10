using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyListButtonScript : MonoBehaviour
{
    Button EnemyListButton;
    public GameObject EnemyListPanel;
    public void ClickEnemyListButton()
    {
        if (EnemyListButton.image.color == Color.green)
        { EnemyListButton.image.color = Color.red; }
        else if (EnemyListButton.image.color == Color.red)
        { EnemyListButton.image.color = Color.green; }

        EnemyListPanel.SetActive(!EnemyListPanel.activeSelf);

    }

    private void Awake()
    {
        EnemyListButton = GetComponent<Button>();
        EnemyListButton.onClick.AddListener(ClickEnemyListButton);
    }

    private void OnDestroy()
    {
        EnemyListButton.onClick.RemoveListener(ClickEnemyListButton);
    }
}
