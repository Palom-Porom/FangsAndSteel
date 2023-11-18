using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StaticUIRefs : MonoBehaviour
{
    [HideInInspector] public static StaticUIRefs Instance { get; private set; }
    [HideInInspector] public TextMeshProUGUI HpText { get { return hpText; } }
    private TextMeshProUGUI hpText;
    [HideInInspector] public TextMeshProUGUI AttackText { get { return attackText; } }
    private TextMeshProUGUI attackText;
    [HideInInspector] public TextMeshProUGUI ReloadText { get { return reloadText; } }
    private TextMeshProUGUI reloadText;
    [HideInInspector] public TextMeshProUGUI AttackRadiusText { get { return attackRadiusText; } }
    private TextMeshProUGUI attackRadiusText;
    [HideInInspector] public TextMeshProUGUI MovementText { get { return movementText; } }
    private TextMeshProUGUI movementText;

    public GameObject unitStats;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        hpText = unitStats.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        attackText = unitStats.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        reloadText = unitStats.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        attackRadiusText = unitStats.transform.GetChild(3).GetComponent<TextMeshProUGUI>();
        movementText = unitStats.transform.GetChild(4).GetComponent<TextMeshProUGUI>();
    }

    [SerializeField] public bool endTurnBut;
    [SerializeField] public bool stopMoveBut;
    [SerializeField] public bool changeSpeedBut;
}
