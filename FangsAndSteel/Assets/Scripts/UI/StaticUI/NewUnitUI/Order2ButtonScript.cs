using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Order2ButtonScript : MonoBehaviour
{
    Button Order2Button;

    public void Order2ButtonClick()
    {
        Debug.Log("Приказ2 работает");
    }
    private void Awake()
    {
        Order2Button = GetComponent<Button>();
        Order2Button.onClick.AddListener(Order2ButtonClick);
    }

    private void OnDestroy()
    {
        Order2Button.onClick.RemoveListener(Order2ButtonClick);

    }
}
