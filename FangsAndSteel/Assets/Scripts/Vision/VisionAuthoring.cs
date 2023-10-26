using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class VisionAuthoring : MonoBehaviour
{
    public float VisionRadius;
    public class Baker : Baker<VisionAuthoring>
    {
        public override void Bake(VisionAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new VisionCharsComponent { radius = authoring.VisionRadius });
            AddComponent(entity, new VisibiliyComponent { });
        }
    }
}

public struct VisionCharsComponent : IComponentData
{
    public float radius;
}
public struct VisibiliyComponent : IComponentData
{

}
