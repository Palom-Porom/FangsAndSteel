using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RadiusEndSliderControllerScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI endSliderText = null;
    [SerializeField] private float maxEndSliderAmount = 100.0f;
    public bool isCosmeticChangeOfValue = false;
    public void StartSliderChange(float value)
    {
        float localValue = value * maxEndSliderAmount;
        endSliderText.text = ((int)localValue).ToString();
        //Debug.Log(localValue);
        if (isCosmeticChangeOfValue) { isCosmeticChangeOfValue = false; return; }
        StaticUIRefs.Instance.newPursuitEndRadius = localValue;
    }
    //[SerializeField] private TextMeshProUGUI endSliderText = null;
    //[SerializeField] private float maxEndSliderAmount = 100.0f;
    //public void EndSliderChange(float value)
    //{
    //    float localValue = value * maxEndSliderAmount;
    //    endSliderText.text = localValue.ToString("0");
    //}
}
