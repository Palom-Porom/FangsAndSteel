using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MaxHpForPursuitSliderControllerScript : MonoBehaviour
{
   [SerializeField] private TextMeshProUGUI maxHpSliderText = null;
   [SerializeField] private float maxHpSliderAmount = 100.0f;
   public void StartSliderChange(float value)
   {
            float localValue = value * maxHpSliderAmount;
            maxHpSliderText.text = localValue.ToString("0");
   }
   
}
