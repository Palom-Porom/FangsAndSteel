using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class VisionMapsAuthoring : MonoBehaviour
{
    public int mapSize = 500;
    public class Baker : Baker<VisionMapsAuthoring>
    {
        public override void Bake(VisionMapsAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            //AddComponent(entity, new VisionMapsComponent { VisionMap = new NativeArray<int>(500*500,Allocator.Persistent), HeightsMap = new NativeArray<int>(500*500, Allocator.Persistent)});
            var buf = AddBuffer<VisionMapBuffer>(entity);
            for (int i = 0; i<authoring.mapSize*authoring.mapSize; i++)
            {
                buf.Add(0);
            }
        }
    }

}

// public struct VisionMapsComponent : IComponentData 
//{
  //  public int value;
 //   public NativeArray<int> VisionMap;
 //   public NativeArray<int> HeightsMap;
//}

[InternalBufferCapacity(0)] 
public struct VisionMapBuffer : IBufferElementData
{
    public int value;
    public static implicit operator VisionMapBuffer(int value)
    {
        return new VisionMapBuffer {  value = value };
    }

    public static implicit operator int (VisionMapBuffer element)
    {
        return element.value;
    }
}





