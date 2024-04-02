using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyArtilleryButtonScript : MonoBehaviour
{
    Button EnemyArtilleryButton;

    public void EnemyArtilleryButtonClick()
    {
        //Debug.Log("Противник артиллерия");
        StaticUIRefs.Instance.newPursuiteUnitType |= UnitTypes.Artillery;
        var x = EnemyArtilleryButton.GetComponent<Image>();
        if (x.color == Color.green) { x.color = Color.red; }
        else { x.color = Color.green; }
    }
    private void Awake()
    {
        EnemyArtilleryButton = GetComponent<Button>();
        EnemyArtilleryButton.onClick.AddListener(EnemyArtilleryButtonClick);
    }

    private void OnDestroy()
    {
        EnemyArtilleryButton.onClick.RemoveListener(EnemyArtilleryButtonClick);

    }
}
