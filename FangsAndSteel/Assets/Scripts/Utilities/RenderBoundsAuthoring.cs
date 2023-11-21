using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Physics.Authoring;
using UnityEngine;

public class RenderBoundsAuthoring : MonoBehaviour
{
    //public float xExtent;
    //public float yExtent;
    //public float zExtent;
    public class Baker : Baker<RenderBoundsAuthoring>
    {
        public override void Bake(RenderBoundsAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new RenderBoundsAdjustRqstComponent
            {
                center = authoring.transform.position,
                extents = authoring.transform.parent.GetComponent<PhysicsShapeAuthoring>().GetBoxProperties().Size
                //extents = new float3(authoring.xExtent, authoring.yExtent, authoring.zExtent)
            });
        }
    }
}

public struct RenderBoundsAdjustRqstComponent : IComponentData
{
    public float3 center;
    public float3 extents;
}
