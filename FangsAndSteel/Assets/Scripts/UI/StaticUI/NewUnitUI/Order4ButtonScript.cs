using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Order4ButtonScript : MonoBehaviour
{
    Button Order4Button;

    public void Order4ButtonClick()
    {
        Debug.Log("������4 ��������");
    }
    private void Awake()
    {
        Order4Button = GetComponent<Button>();
        Order4Button.onClick.AddListener(Order4ButtonClick);
    }

    private void OnDestroy()
    {
        Order4Button.onClick.RemoveListener(Order4ButtonClick);

    }
}
