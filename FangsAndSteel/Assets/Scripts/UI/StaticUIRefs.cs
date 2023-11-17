using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StaticUIRefs : MonoBehaviour
{
    [SerializeField] public static StaticUIRefs Instance { get; private set; }
    [SerializeField] public Image TurnIndicator { get { return turnIndicator; } }
    public Image turnIndicator;

    [SerializeField] public TextMeshProUGUI TurnTimer { get { return turnTimer; } }
    public TextMeshProUGUI turnTimer;

    void Awake()
    {
        Instance = this;
    }

    [SerializeField] public bool endTurnBut;
    [SerializeField] public bool stopMoveBut;
    [SerializeField] public bool changeSpeedBut;
}
