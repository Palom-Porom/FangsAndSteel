using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

public class FogMaterial : MonoBehaviour
{
    public static Material material;
    public Material materialNonStatic;

    private void Awake()
    {
        material = materialNonStatic;
    }
}