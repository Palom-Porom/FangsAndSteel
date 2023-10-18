using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using UnityEngine;

public partial struct TargetingSystem : ISystem, ISystemStartStop
{
    private InputData inputData;

    public void OnCreate (ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
        state.RequireForUpdate<InputData>();
        state.RequireForUpdate<MovementComponent>();
    }

    public void OnStartRunning(ref SystemState state)
    {
        new PutAllOnTerrainJob { collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld }.Schedule();
    }

    public void OnStopRunning(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        inputData = SystemAPI.GetSingleton<InputData>();

        if (!inputData.neededTargeting)
            return;

        RaycastInput raycastInput = new RaycastInput
        {
            Start = inputData.cameraPosition,
            End = inputData.cameraPosition + inputData.mouseDirection * 1000,
            Filter = new CollisionFilter
            {
                BelongsTo = (uint)layers.Everything,
                CollidesWith = (uint)layers.Terrain,
                GroupIndex = 0
            }
        };

        NativeReference<RaycastResult> raycastResult = new NativeReference<RaycastResult>(Allocator.TempJob);

        state.Dependency = new TargetRayCastJob
        {
            collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld,
            raycastInput = raycastInput,
            raycastResult = raycastResult
        }.Schedule(state.Dependency);

        new ChangeTargetJob { raycastResult = raycastResult }.Schedule();

        raycastResult.Dispose(state.Dependency);
    }
}


//TODO: Redo with ChunkIteration optimization (so that it doesn't go through all selected units if raycastResult.Value.hasHit == false)
/// <summary>
/// Changes targets for all selected units if raycast of new target was succesfull
/// </summary>
[BurstCompile]
public partial struct ChangeTargetJob : IJobEntity
{
    [ReadOnly]
    public NativeReference<RaycastResult> raycastResult;
    public void Execute (ref MovementComponent movementComponent)
    {
        var result = raycastResult.Value;
        if (!result.hasHit)
            return;
        movementComponent.target = result.raycastHitInfo.Position;
        movementComponent.isMoving = true;
    }
}
