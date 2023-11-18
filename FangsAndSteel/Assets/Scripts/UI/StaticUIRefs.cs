using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticUIRefs : MonoBehaviour
{
    [SerializeField] public static StaticUIRefs Instance { get; private set; }
    [SerializeField] public static StaticUIRefs hpText { get; }
    [SerializeField] public static StaticUIRefs attackText { get; }
    [SerializeField] public StaticUIRefs speedText { get; }

    void Awake()
    {
        Instance = this;
    }

    [SerializeField] public bool endTurnBut;
    [SerializeField] public bool stopMoveBut;
    [SerializeField] public bool changeSpeedBut;
}
