using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyMachineGunnerButtonScript : MonoBehaviour
{
    Button EnemyMachineGunnerButton;

    public void EnemyMachineGunnerButtonClick()
    {
        //Debug.Log("Противник пулеметчик");
        StaticUIRefs.Instance.newPursuiteUnitType |= UnitTypes.MachineGunner;
        var x = EnemyMachineGunnerButton.GetComponent<Image>();
        if (x.color == Color.green) { x.color = Color.red; }
        else { x.color = Color.green; }
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
