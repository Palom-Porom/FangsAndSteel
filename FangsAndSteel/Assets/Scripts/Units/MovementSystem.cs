using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;

[BurstCompile]
public partial struct MovementSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
        state.RequireForUpdate<MovementComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) 
    {
        var moveJob = new MovementJob { time = SystemAPI.Time.DeltaTime, collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld };
        moveJob.Schedule();
    }


}

/// <summary>
/// Moves all entities with MovementComponent to their target over the terrain
/// </summary>
[BurstCompile]
public partial struct MovementJob : IJobEntity
{

    public float time;
    [ReadOnly] public CollisionWorld collisionWorld;


    public void Execute(ref LocalTransform transform, ref MovementComponent movementComponent)
    {
        if (!movementComponent.isMoving)
            return;

        if (math.distancesq(movementComponent.target, transform.Position) < (time * movementComponent.speed) / 2)
        {
            movementComponent.isMoving = false;
            return;
        }

        float3 tempDir = movementComponent.target - transform.Position;
        tempDir.y = 0;
        tempDir = math.normalize(tempDir);
        float speed = time * movementComponent.speed;

        CollisionFilter filter = new CollisionFilter
        {
            BelongsTo = (uint)layers.Everything,
            CollidesWith = (uint)layers.Terrain,
            GroupIndex = 0
        };
        float3 pointDistancePos = transform.Position + tempDir * speed;
        var pointDistanceInput = new PointDistanceInput { Filter = filter, Position = pointDistancePos, MaxDistance = 1.5f };
        if (collisionWorld.CalculateDistance(pointDistanceInput, out DistanceHit closestHit))
        {
            if (closestHit.SurfaceNormal.y < 0f)
                closestHit.SurfaceNormal = -closestHit.SurfaceNormal;
            transform.Rotation = quaternion.LookRotationSafe(closestHit.Position - transform.Position, closestHit.SurfaceNormal);
            transform.Position = closestHit.Position;
        }
        else
        {
            Debug.Log("ERROR: Terrain was not found near unit");
        }
    }
}
