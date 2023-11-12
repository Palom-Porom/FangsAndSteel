using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaticUIRefs : MonoBehaviour
{
    public static StaticUIRefs Instance { get; private set; }
    public Image TurnIndicator { get { return turnIndicator; } }
    [SerializeField] private Image turnIndicator;
    void Awake()
    {
        Instance = this;
    }

    [HideInInspector] public bool endTurnBut;
    [HideInInspector] public bool stopMoveBut;
    [HideInInspector] public bool changeSpeedBut;
}
