using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Order1ButtonScript : MonoBehaviour
{
    Button Order1Button;

    public void Order1ButtonClick()
    {
        Debug.Log("Приказ1 работает");
    }
    private void Awake()
    {
        Order1Button = GetComponent<Button>();
        Order1Button.onClick.AddListener(Order1ButtonClick);
    }

    private void OnDestroy()
    {
        Order1Button.onClick.RemoveListener(Order1ButtonClick);
        
    }
}
