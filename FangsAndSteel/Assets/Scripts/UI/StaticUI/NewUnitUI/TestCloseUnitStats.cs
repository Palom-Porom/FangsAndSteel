using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static ShowCloseUnitStats;

public class TestCloseUnitStats : MonoBehaviour
{
    Button button;


    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(CloseStats);
    }
    private void OnDestroy()
    {
        button.onClick.RemoveListener(CloseStats);
    }
}
