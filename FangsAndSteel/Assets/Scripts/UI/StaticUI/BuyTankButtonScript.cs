using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuyTankButtonScript : MonoBehaviour
{
    Button button;

    void BuyTankButtonClick()
    {
        StaticUIRefs.Instance.buyTankButton = true;
    }

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(BuyTankButtonClick);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(BuyTankButtonClick);
    }
}
