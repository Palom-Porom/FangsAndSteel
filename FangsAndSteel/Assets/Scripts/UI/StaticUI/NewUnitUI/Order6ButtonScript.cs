using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Order6ButtonScript : MonoBehaviour
{
    Button Order6Button;

    public void Order6ButtonClick()
    {
        Debug.Log("Приказ6 работает");
    }
    private void Awake()
    {
        Order6Button = GetComponent<Button>();
        Order6Button.onClick.AddListener(Order6ButtonClick);
    }

    private void OnDestroy()
    {
        Order6Button.onClick.RemoveListener(Order6ButtonClick);

    }
}
