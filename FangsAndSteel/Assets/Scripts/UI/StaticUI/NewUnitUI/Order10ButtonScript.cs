using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Order10ButtonScript : MonoBehaviour
{
    Button Order10Button;

    public void Order10ButtonClick()
    {
        Debug.Log("Приказ10 работает");
    }
    private void Awake()
    {
        Order10Button = GetComponent<Button>();
        Order10Button.onClick.AddListener(Order10ButtonClick);
    }

    private void OnDestroy()
    {
        Order10Button.onClick.RemoveListener(Order10ButtonClick);

    }
}
