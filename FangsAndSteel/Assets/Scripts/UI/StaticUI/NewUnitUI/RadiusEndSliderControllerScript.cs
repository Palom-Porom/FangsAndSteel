using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RadiusEndSliderContollerScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI endSliderText = null;
    [SerializeField] private float maxEndSliderAmount = 100.0f;
    public void EndSliderChange(float value)
    {
        float localValue = value * maxEndSliderAmount;
        endSliderText.text = localValue.ToString("0");
    }
}
