using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RadiusSliderControllerScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI startSliderText = null;
    [SerializeField] private float maxStartSliderAmount = 100.0f;
    public void StartSliderChange(float value)
    {
        float localValue = value * maxStartSliderAmount;
        startSliderText.text = localValue.ToString("0");
    }
}
