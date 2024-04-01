using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyInfantryManButtonScript : MonoBehaviour
{
    Button EnemyInfantryManButton;

    public void EnemyInfantryManButtonClick()
    {
        Debug.Log("Противник пехотинец");
        var x = EnemyInfantryManButton.GetComponent<Image>();
        if (x.color == Color.green) { x.color = Color.red; }
        else { x.color = Color.green; }
    }
    private void Awake()
    {
        EnemyInfantryManButton = GetComponent<Button>();
        EnemyInfantryManButton.onClick.AddListener(EnemyInfantryManButtonClick);
    }

    private void OnDestroy()
    {
        EnemyInfantryManButton.onClick.RemoveListener(EnemyInfantryManButtonClick);

    }
}

