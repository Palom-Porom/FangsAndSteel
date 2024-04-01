using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class VisionMapsAuthoring : MonoBehaviour
{
    public int mapSize = 500;
    public GameObject noTeamDebugCube;
    public GameObject firstTeamDebugCube;
    public GameObject secondTeamDebugCube;
    public GameObject bothTeamsDebugCube;
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

            AddComponent(entity, new DebugCube 
            {
                noTeamPrefub = GetEntity(authoring.noTeamDebugCube, TransformUsageFlags.Dynamic),
                firstTeamPrefub = GetEntity(authoring.firstTeamDebugCube, TransformUsageFlags.Dynamic),
                secondTeamPrefub = GetEntity(authoring.secondTeamDebugCube, TransformUsageFlags.Dynamic),
                bothTeamsPrefub = GetEntity(authoring.bothTeamsDebugCube, TransformUsageFlags.Dynamic)
            });
        }
    }

}

// public struct VisionMapsComponent : IComponentData 
//{
//  public int value;
//   public NativeArray<int> VisionMap;
//   public NativeArray<int> HeightsMap;
//}

[GhostComponent(PrefabType = GhostPrefabType.All)]
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

public struct DebugCube : IComponentData
{
    public Entity noTeamPrefub;
    public Entity firstTeamPrefub;
    public Entity secondTeamPrefub;
    public Entity bothTeamsPrefub;
}





