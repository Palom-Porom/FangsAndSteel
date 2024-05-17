using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuyMachineGunnerButtonScript : MonoBehaviour
{
    Button button;

    void BuyMachineGunnerButtonClick()
    {
        StaticUIRefs.Instance.buyMachineGunnerButton = true;
    }

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(BuyMachineGunnerButtonClick);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(BuyMachineGunnerButtonClick);
    }
}
