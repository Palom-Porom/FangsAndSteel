using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyTankButtonScript : MonoBehaviour
{
    Button EnemyAntiTankButton;

    public void EnemyTankButtonClick()
    {
        Debug.Log("Противник танк");
    }
    private void Awake()
    {
        EnemyAntiTankButton = GetComponent<Button>();
        EnemyAntiTankButton.onClick.AddListener(EnemyTankButtonClick);
    }

    private void OnDestroy()
    {
        EnemyAntiTankButton.onClick.RemoveListener(EnemyTankButtonClick);

    }
}
