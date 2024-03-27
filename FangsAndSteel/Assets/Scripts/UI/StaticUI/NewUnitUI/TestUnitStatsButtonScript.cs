using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ShowCloseUnitStats;

public class TestUnitStatsButtonScript : MonoBehaviour
{
    Button button;
    private void ButtonClick()
    {
        UnitStats unitStats;
        unitStats.hp = 100;
        unitStats.speed = 5;
        unitStats.damage = 50;
        unitStats.attackRadius = 10;
        unitStats.reload = 35;
        unitStats.tapePresence = true;
        unitStats.bullets = 10;
        ShowStats(unitStats);
    }

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(ButtonClick);
    }
    private void OnDestroy()
    { 
        button.onClick.RemoveListener(ButtonClick);
    }
}
