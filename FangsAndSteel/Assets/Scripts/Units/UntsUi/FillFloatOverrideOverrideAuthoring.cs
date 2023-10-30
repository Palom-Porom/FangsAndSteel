using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

public class FillFloatOverrideAuthoring : MonoBehaviour
{
    class Baker : Baker<FillFloatOverrideAuthoring>
    {
        public override void Bake(FillFloatOverrideAuthoring authoring)
        {
            var e = GetEntity(TransformUsageFlags.None);
            AddComponent(e, new FillFloatOverride { Value = 1});
        }
    }
}


[MaterialProperty("_Fill")]
public struct FillFloatOverride : IComponentData
{
    public float Value;
}

