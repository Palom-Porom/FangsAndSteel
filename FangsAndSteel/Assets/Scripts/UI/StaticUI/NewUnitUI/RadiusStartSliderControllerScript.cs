using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RadiusSliderControllerScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI startSliderText = null;
    [SerializeField] private float maxStartSliderAmount = 100.0f;
    public bool isCosmeticChangeOfValue = false;
    public void StartSliderChange(float value)
    {
        float localValue = value * maxStartSliderAmount;
        startSliderText.text = ((int)localValue).ToString();
        //Debug.Log(localValue);
        if (isCosmeticChangeOfValue) { isCosmeticChangeOfValue = false; return; }
        StaticUIRefs.Instance.newPursuitStartRadius = localValue;
    }
}
