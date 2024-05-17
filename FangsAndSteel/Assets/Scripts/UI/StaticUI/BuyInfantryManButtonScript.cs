using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuyInfantryManButtonScript : MonoBehaviour
{
    Button button;

    void BuyInfantryManButtonClick()
    {
        StaticUIRefs.Instance.buyInfantryManButton = true;
    }

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(BuyInfantryManButtonClick);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(BuyInfantryManButtonClick);
    }
}
