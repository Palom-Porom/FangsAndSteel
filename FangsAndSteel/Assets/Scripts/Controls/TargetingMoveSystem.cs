using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using UnityEngine;

[UpdateBefore(typeof(MovementSystem))]
[BurstCompile]
public partial struct TargetingMoveSystem : ISystem, ISystemStartStop
{
    private InputData inputData;

    [BurstCompile]
    public void OnCreate (ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
        state.RequireForUpdate<InputData>();
        state.RequireForUpdate<MovementComponent>();
    }

    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        new PutAllOnTerrainJob { collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld }.Schedule();
    }

    [BurstCompile]
    public void OnStopRunning(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        inputData = SystemAPI.GetSingleton<InputData>();

        if (!inputData.neededTargeting)
            return;

        RaycastInput raycastInput = new RaycastInput
        {
            Start = inputData.cameraPosition,
            End = inputData.mouseTargetingPoint,
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

        new ChangeTargetJob { raycastResult = raycastResult, shiftTargeting = inputData.shiftTargeting }.Schedule();

        raycastResult.Dispose(state.Dependency);
    }
}


///TODO: Redo with ChunkIteration optimization (so that it doesn't go through all selected units if raycastResult.Value.hasHit == false)
/// <summary>
/// Changes targets for all selected units if raycast of new target was succesfull
/// </summary>
[BurstCompile]
public partial struct ChangeTargetJob : IJobEntity
{
    [ReadOnly]
    public NativeReference<RaycastResult> raycastResult;
    public bool shiftTargeting;
    public void Execute (ref MovementComponent movementComponent, DynamicBuffer<MovementCommandsBuffer> moveComBuf, in SelectTag selectTag)
    {
        var result = raycastResult.Value;
        if (!result.hasHit)
            return;
        if (shiftTargeting && movementComponent.isMoving)
        {
            moveComBuf.Add(new MovementCommandsBuffer { target = result.raycastHitInfo.Position });
        }
        else
        {
            moveComBuf.Clear();
            movementComponent.target = result.raycastHitInfo.Position;
            movementComponent.isMoving = true;
        }
    }
}
