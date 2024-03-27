using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Order3ButtonScript : MonoBehaviour
{
    Button Order3Button;

    public void Order3ButtonClick()
    {
        Debug.Log("Приказ3 работает");
    }
    private void Awake()
    {
        Order3Button = GetComponent<Button>();
        Order3Button.onClick.AddListener(Order3ButtonClick);
    }

    private void OnDestroy()
    {
        Order3Button.onClick.RemoveListener(Order3ButtonClick);

    }
}