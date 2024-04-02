using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using static ShowCloseUnitStats;

public class ShowCloseUnitStats : MonoBehaviour
{
    public static GameObject StatsPanel;
    private void Start()
    {
        StatsPanel = gameObject;
    }
    public static void ShowStats(UnitStats unitStats)
    {
        var hp = StatsPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        hp.text = $"Здоровье: {unitStats.hp}";

        var speed = StatsPanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        speed.text = $"Скорость: {unitStats.speed}";

        var damage = StatsPanel.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        damage.text = $"Урон: {unitStats.damage}";

        var attackRadius = StatsPanel.transform.GetChild(3).GetComponent<TextMeshProUGUI>();
        attackRadius.text = $"Радиус атаки: {unitStats.attackRadius}";

        var reload = StatsPanel.transform.GetChild(4).GetComponent<TextMeshProUGUI>();
        reload.text = $"Скорость перезарядки: {unitStats.reload:F2}";

        if (unitStats.tapePresence == true) 
        {
            var bullets = StatsPanel.transform.GetChild(5).GetComponent<TextMeshProUGUI>();
            bullets.text = $"Kоличество патронов: {unitStats.bullets}";
            StatsPanel.transform.GetChild(5).gameObject.SetActive(true);
        }
        else if (unitStats.tapePresence == false) 
        {  
            StatsPanel.transform.GetChild(5).gameObject.SetActive(false);
        }


        

    }
    public static void CloseStats()
    {
        var hp = StatsPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        hp.text = "";

        var speed = StatsPanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        speed.text ="";

        var damage = StatsPanel.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        damage.text ="";

        var attackRadius = StatsPanel.transform.GetChild(3).GetComponent<TextMeshProUGUI>();
        attackRadius.text ="";

        var reload = StatsPanel.transform.GetChild(4).GetComponent<TextMeshProUGUI>();
        reload.text = "";
        
        var bullets =StatsPanel.transform.GetChild(5).GetComponent<TextMeshProUGUI>();
        bullets.text = "";
    }

    public struct UnitStats
    {
        public int hp;
        public int speed;
        public int damage;
        public int attackRadius;
        public float reload;
        public bool tapePresence;
        public int bullets;
    }
}
