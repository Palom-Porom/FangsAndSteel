using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Order9ButtonScript : MonoBehaviour
{
    Button Order9Button;

    public void Order9ButtonClick()
    {
        Debug.Log("Приказ9 работает");
    }
    private void Awake()
    {
        Order9Button = GetComponent<Button>();
        Order9Button.onClick.AddListener(Order9ButtonClick);
    }

    private void OnDestroy()
    {
        Order9Button.onClick.RemoveListener(Order9ButtonClick);

    }
}
