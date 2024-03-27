using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class AttackDistSliderControllerScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI AttackDistSliderText = null;
    [SerializeField] private float maxAttackDistSliderAmount = 100.0f;
    public void EndSliderChange(float value)
    {
        float localValue = value * maxAttackDistSliderAmount;
        AttackDistSliderText.text = localValue.ToString("0");
    }
}
