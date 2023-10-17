using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.TerrainUtils;

public class TerrainAuthoring : MonoBehaviour
{
    [SerializeField] LayerMask belongsToLayers;
    [SerializeField] LayerMask collidesWithLayers;
    [SerializeField] int groupIndex;

    private void Awake()
    {
        if (!TryGetComponent(out Terrain terrain))
        {
            Debug.Log("ERROR: Terrain was not found!");
            return;
        }

        CollisionFilter filter = new CollisionFilter
        {
            BelongsTo = (uint)belongsToLayers.value,
            CollidesWith = (uint)collidesWithLayers.value,
            GroupIndex = groupIndex
        };

        PhysicsCollider terrainCollider = CreateTerrainCollider(terrain.terrainData, filter);

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        Entity terrainEntity = entityManager.CreateEntity();
        entityManager.SetName(terrainEntity, "TerrainEntity");
        entityManager.AddComponent<PhysicsCollider>(terrainEntity);
        entityManager.SetComponentData<PhysicsCollider>(terrainEntity, terrainCollider);
        entityManager.AddComponent<LocalToWorld>(terrainEntity);
        entityManager.SetComponentData(terrainEntity, new LocalToWorld());
        entityManager.AddComponent<LocalTransform>(terrainEntity);
        entityManager.SetComponentData(terrainEntity, new LocalTransform
        {
            Position = transform.position,
            Rotation = quaternion.identity,
            Scale = 1f
        });
        entityManager.AddComponent<PhysicsWorldIndex>(terrainEntity);
        entityManager.AddComponent<TerrainTag>(terrainEntity);
    }

    private PhysicsCollider CreateTerrainCollider (TerrainData terrainData, CollisionFilter filter)
    {
        int resolution = terrainData.heightmapResolution;
        int2 size = new int2(resolution, resolution);
        Vector3 scale = terrainData.heightmapScale;

        NativeArray<float> heights = new NativeArray<float>(resolution * resolution, Allocator.Temp);
        float[,] temporaryHeightsMatrice = terrainData.GetHeights(0, 0, resolution, resolution);
        for (int j = 0; j < size.y; j++)
        {
            for (int i = 0; i < size.x; i++)
            {
                var h = temporaryHeightsMatrice[i, j];
                heights[j + i * size.x] = h;
            }
        }

        PhysicsCollider collider = new PhysicsCollider
        {
            Value = Unity.Physics.TerrainCollider.Create(heights, size, scale, Unity.Physics.TerrainCollider.CollisionMethod.Triangles, filter)
        };
        heights.Dispose();
        return collider;
    }
}


public struct TerrainTag : IComponentData
{
}
