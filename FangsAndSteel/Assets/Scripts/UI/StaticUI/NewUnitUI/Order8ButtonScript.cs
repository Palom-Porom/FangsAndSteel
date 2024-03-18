using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Order8ButtonScript : MonoBehaviour
{
    Button Order8Button;

    public void Order8ButtonClick()
    {
        Debug.Log("Приказ8 работает");
    }
    private void Awake()
    {
        Order8Button = GetComponent<Button>();
        Order8Button.onClick.AddListener(Order8ButtonClick);
    }

    private void OnDestroy()
    {
        Order8Button.onClick.RemoveListener(Order8ButtonClick);

    }
}
