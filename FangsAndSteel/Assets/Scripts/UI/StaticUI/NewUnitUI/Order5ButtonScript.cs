using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Order5ButtonScript : MonoBehaviour
{
    Button Order5Button;

    public void Order5ButtonClick()
    {
        Debug.Log("Приказ5 работает");
    }
    private void Awake()
    {
        Order5Button = GetComponent<Button>();
        Order5Button.onClick.AddListener(Order5ButtonClick);
    }

    private void OnDestroy()
    {
        Order5Button.onClick.RemoveListener(Order5ButtonClick);

    }
}

