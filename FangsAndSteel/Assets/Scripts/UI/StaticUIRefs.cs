using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticUIRefs : MonoBehaviour
{
    [SerializeField] public static StaticUIRefs Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    [SerializeField] public bool endTurnBut;
}
