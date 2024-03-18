using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Order7ButtonScript : MonoBehaviour
{
    Button Order7Button;

    public void Order7ButtonClick()
    {
        Debug.Log("Приказ7 работает");
    }
    private void Awake()
    {
        Order7Button = GetComponent<Button>();
        Order7Button.onClick.AddListener(Order7ButtonClick);
    }

    private void OnDestroy()
    {
        Order7Button.onClick.RemoveListener(Order7ButtonClick);

    }
}
