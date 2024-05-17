using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

//TODO: Redo with ChunkIteration optimizations
/// <summary>
/// Putting all entities with movementComponent on terrain (by adopting position and rotation) without moving it
/// </summary>
[BurstCompile]
public partial struct PutAllOnTerrainJob : IJobEntity
{
    [ReadOnly] public CollisionWorld collisionWorld;

    public void Execute(ref LocalTransform transform, in MovementComponent movementComponent)
    {
        CollisionFilter filter = new CollisionFilter
        {
            BelongsTo = (uint)layers.Everything,
            CollidesWith = (uint)layers.Terrain,
            GroupIndex = 0
        };
        var pointDistanceInput = new PointDistanceInput { Filter = filter, Position = transform.Position, MaxDistance = 5f };
        if (collisionWorld.CalculateDistance(pointDistanceInput, out DistanceHit closestHit))
        {
            if (closestHit.SurfaceNormal.y < 0f)
                closestHit.SurfaceNormal = -closestHit.SurfaceNormal;
            transform.Rotation = quaternion.LookRotationSafe(transform.Forward(), closestHit.SurfaceNormal);
            transform.Position = closestHit.Position;
        }
        else
        {
            Debug.Log("ERROR: Terrain was not found near unit");
        }
    }
}

[WithAll(typeof(FlagTag))]
public partial struct PutFlagsOnTerrainJob : IJobEntity
{
    [ReadOnly] public CollisionWorld collisionWorld;

    public void Execute(ref LocalTransform transform)
    {
        CollisionFilter filter = new CollisionFilter
        {
            BelongsTo = (uint)layers.Everything,
            CollidesWith = (uint)layers.Terrain,
            GroupIndex = 0
        };
        var pointDistanceInput = new PointDistanceInput { Filter = filter, Position = transform.Position, MaxDistance = 10f };
        if (collisionWorld.CalculateDistance(pointDistanceInput, out DistanceHit closestHit))
        {
            if (closestHit.SurfaceNormal.y < 0f)
                closestHit.SurfaceNormal = -closestHit.SurfaceNormal;
            transform.Rotation = quaternion.LookRotationSafe(transform.Forward(), closestHit.SurfaceNormal);
            transform.Position = closestHit.Position;
        }
        else
        {
            Debug.Log("ERROR: Terrain was not found near unit");
        }
    }
}


[BurstCompile]
[WithAll(typeof(NotBoughtYetTag))]
public partial struct PutAllNotBuyedOnTerrainJob : IJobEntity
{
    [ReadOnly] public CollisionWorld collisionWorld;

    public void Execute(ref LocalTransform transform, in MovementComponent movementComponent)
    {
        CollisionFilter filter = new CollisionFilter
        {
            BelongsTo = (uint)layers.Everything,
            CollidesWith = (uint)layers.Terrain,
            GroupIndex = 0
        };
        var pointDistanceInput = new PointDistanceInput { Filter = filter, Position = transform.Position, MaxDistance = 5f };
        if (collisionWorld.CalculateDistance(pointDistanceInput, out DistanceHit closestHit))
        {
            if (closestHit.SurfaceNormal.y < 0f)
                closestHit.SurfaceNormal = -closestHit.SurfaceNormal;
            transform.Rotation = quaternion.LookRotationSafe(transform.Forward(), closestHit.SurfaceNormal);
            transform.Position = closestHit.Position;
        }
        else
        {
            Debug.Log("ERROR: Terrain was not found near unit");
        }
    }
}