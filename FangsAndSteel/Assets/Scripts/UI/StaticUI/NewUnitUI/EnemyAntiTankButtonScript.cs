using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyAntiTankButtonScript : MonoBehaviour
{
    Button EnemyAntiTankButton;

    public void EnemyAntiTankButtonClick()
    {
        //Debug.Log("ѕротивник противотанковый пехотинец");
        StaticUIRefs.Instance.newPursuiteUnitType |= UnitTypes.AntyTank;
        var x = EnemyAntiTankButton.GetComponent<Image>();
        if (x.color == Color.green) { x.color = Color.red; }
        else { x.color = Color.green; }
    }
    private void Awake()
    {
        EnemyAntiTankButton = GetComponent<Button>();
        EnemyAntiTankButton.onClick.AddListener(EnemyAntiTankButtonClick);
    }

    private void OnDestroy()
    {
        EnemyAntiTankButton.onClick.RemoveListener(EnemyAntiTankButtonClick);

    }
}
