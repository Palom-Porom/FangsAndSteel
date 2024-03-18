using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyArtilleryButtonScript : MonoBehaviour
{
    Button EnemyArtilleryButton;

    public void EnemyArtilleryButtonClick()
    {
        Debug.Log("Противник артиллерия");
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
