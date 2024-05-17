using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuyAntiTankButtonScript : MonoBehaviour
{
    Button button;

    void BuyAntiTankButtonClick()
    {
        StaticUIRefs.Instance.buyAntiTankButton = true;
    }

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(BuyAntiTankButtonClick);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(BuyAntiTankButtonClick);
    }
}
