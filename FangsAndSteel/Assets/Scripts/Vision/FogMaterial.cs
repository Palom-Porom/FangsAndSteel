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
    public int width = 500;
    public int height = 500;

    private void Awake()
    {
        material = materialNonStatic;
    }
    class Baker : Baker<FogMaterial>
    {
        public override void Bake(FogMaterial authoring)
        {
            var e = GetEntity(TransformUsageFlags.None);
            //AddComponent(e, new FogMapTextureOverride { Value = authoring.material });

        }
    }
}


public class FogMapTextureOverride : IComponentData
{
    public Material Value;
}