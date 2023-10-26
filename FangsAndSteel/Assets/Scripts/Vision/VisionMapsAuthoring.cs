using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class VisionMapsAuthoring : MonoBehaviour
{
    
    public class Baker : Baker<VisionMapsAuthoring>
    {
        public override void Bake(VisionMapsAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new VisionMapsComponent { VisionMap = new NativeArray<int>(500*500,Allocator.Persistent), HeightsMap = new NativeArray<int>(500*500, Allocator.Persistent)});
        }
    }

}
public struct VisionMapsComponent : IComponentData 
{
    public NativeArray<int> VisionMap;
    public NativeArray<int> HeightsMap;
} 



