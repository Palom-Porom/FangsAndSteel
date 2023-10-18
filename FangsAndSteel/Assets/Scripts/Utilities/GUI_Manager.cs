using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class GUI_Manager : MonoBehaviour
{
    public static GUI_Manager Instance;

    [HideInInspector] public bool isDragging;
    [HideInInspector] public float2 mouseStartPos;

    public void Awake()
    {
        Instance = this;
    }

    private void OnGUI()
    {
        if (isDragging)
        {
            GUI_Utilities.DrawRectWithBorders(GUI_Utilities.GetScreenRect(mouseStartPos, Mouse.current.position.value), 3f, Color.green, Color.grey);
        }
    }
}
