using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MaxHpForPursuitSliderControllerScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI maxHpSliderText = null;
    [SerializeField] private float maxHpSliderAmount = 100.0f;
    public bool isCosmeticChangeOfValue = false;
    public void StartSliderChange(float value)
    {
        float localValue = value * maxHpSliderAmount;
        maxHpSliderText.text = ((int)localValue).ToString();
        //Debug.Log(localValue);
        if (isCosmeticChangeOfValue) { isCosmeticChangeOfValue = false; return; }
        StaticUIRefs.Instance.newPursuitmaxHp = localValue;
    }
    //[SerializeField] private TextMeshProUGUI maxHpSliderText = null;
    //[SerializeField] private float maxHpSliderAmount = 100.0f;
    //public void StartSliderChange(float value)
    //{
    //         float localValue = value * maxHpSliderAmount;
    //         maxHpSliderText.text = localValue.ToString("0");
    //}

}
