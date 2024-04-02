using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HintScript : MonoBehaviour
{
    public GameObject HintText;

    private void Start()
    {
        HintText.SetActive(false);
    }

    public void HintOpen()
    {
        HintText.SetActive(true);
        //Debug.Log("включилась");
        
    }

    public void HintClose()
    {
        HintText.SetActive(false);
        //Debug.Log("выключилась");
    }
}
