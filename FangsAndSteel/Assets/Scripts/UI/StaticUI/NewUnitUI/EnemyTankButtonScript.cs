using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyTankButtonScript : MonoBehaviour
{
    Button EnemyTankButton;

    public void EnemyTankButtonClick()
    {
        //Debug.Log("Противник танк");
        StaticUIRefs.Instance.newPursuiteUnitType |= UnitTypes.Tank;
        var x = EnemyTankButton.GetComponent<Image>();
        if (x.color == Color.green) { x.color = Color.red; }
        else { x.color = Color.green; }
    }
    private void Awake()
    {
        EnemyTankButton = GetComponent<Button>();
        EnemyTankButton.onClick.AddListener(EnemyTankButtonClick);
    }

    private void OnDestroy()
    {
        EnemyTankButton.onClick.RemoveListener(EnemyTankButtonClick);

    }
}
