using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;
using System.Net.Security;

[UpdateInGroup(typeof(UnitsSystemGroup))]
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
        var moveJob = new MovementJob { deltaTime = SystemAPI.Time.DeltaTime, collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld };
        moveJob.Schedule();
    }


}

/// <summary>
/// Moves all entities with MovementComponent to their target over the terrain
/// </summary>
[BurstCompile]
public partial struct MovementJob : IJobEntity
{
    private const float ROT_TIME = 0.33f;

    public float deltaTime;
    [ReadOnly] public CollisionWorld collisionWorld;


    public void Execute(ref LocalTransform transform, ref MovementComponent movementComponent, DynamicBuffer<MovementCommandsBuffer> movementCommandsBuffer)
    {
        if (!movementComponent.isMoving)
            return;

        if (math.distancesq(movementComponent.target, transform.Position) < (deltaTime * movementComponent.speed) / 2)
        {
            //Has next target to move?
            if (movementCommandsBuffer.Length != 0)
            {
                movementComponent.target = movementCommandsBuffer[0].target;
                movementCommandsBuffer.RemoveAt(0);
            }
            else
            {
                movementComponent.isMoving = false;
                return;
            } 
        }

        float3 tempDir = movementComponent.target - transform.Position;
        tempDir.y = 0;
        tempDir = math.normalize(tempDir);
        float speed = deltaTime * movementComponent.speed;

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
            //Debug.Log(transform.Rotation.value - quaternion.LookRotationSafe(closestHit.Position - transform.Position, closestHit.SurfaceNormal).value);
            quaternion targetRot = quaternion.LookRotationSafe(closestHit.Position - transform.Position, closestHit.SurfaceNormal);
            if (movementComponent.lastRotTarget.Equals(targetRot))
            {
                if (movementComponent.rotTimePassed < ROT_TIME)
                {
                    movementComponent.rotTimePassed += deltaTime;
                    transform.Rotation = math.nlerp(transform.Rotation, targetRot, movementComponent.rotTimePassed / ROT_TIME);
                }
            }
            else
            {
                movementComponent.rotTimePassed = deltaTime;
                transform.Rotation = math.nlerp(transform.Rotation, targetRot, deltaTime / ROT_TIME);
            }
            transform.Position = closestHit.Position;
        }
        else
        {
            Debug.Log("ERROR: Terrain was not found near unit");
        }
    }
}
