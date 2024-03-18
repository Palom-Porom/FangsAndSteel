using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyMachineGunnerButtonScript : MonoBehaviour
{
    Button EnemyMachineGunnerButton;

    public void EnemyMachineGunnerButtonClick()
    {
        Debug.Log("Противник пулеметчик");
    }
    private void Awake()
    {
        EnemyMachineGunnerButton = GetComponent<Button>();
        EnemyMachineGunnerButton.onClick.AddListener(EnemyMachineGunnerButtonClick);
    }

    private void OnDestroy()
    {
        EnemyMachineGunnerButton.onClick.RemoveListener(EnemyMachineGunnerButtonClick);

    }
}
