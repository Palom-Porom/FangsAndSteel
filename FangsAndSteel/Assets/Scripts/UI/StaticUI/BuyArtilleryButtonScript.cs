using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuyArtilleryButtonScript : MonoBehaviour
{
    Button button;

    void BuyArtilleryButtonClick()
    {
        StaticUIRefs.Instance.buyArtilleryButton = true;
    }

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(BuyArtilleryButtonClick);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(BuyArtilleryButtonClick);
    }
}
