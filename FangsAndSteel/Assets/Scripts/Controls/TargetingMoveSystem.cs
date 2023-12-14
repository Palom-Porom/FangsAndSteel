using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using UnityEngine;
using AnimCooker;
using System.Linq;

[UpdateInGroup(typeof(ControlsSystemGroup), OrderFirst = true)]
[UpdateAfter(typeof(ControlSystem))]
[BurstCompile]
public partial struct TargetingMoveSystem : ISystem, ISystemStartStop
{
    private InputData inputData;

    ComponentLookup<AnimationCmdData> animCmdLookup;
    ComponentLookup<AnimationStateData> animStateLookup;
    NativeArray<AnimDbEntry> moveClips;

    [BurstCompile]
    public void OnCreate (ref SystemState state)
    {
        state.RequireForUpdate<GameTag>();
        state.RequireForUpdate<PhysicsWorldSingleton>();
        state.RequireForUpdate<InputData>();
        state.RequireForUpdate<MovementComponent>();
        state.RequireForUpdate<AnimDbRefData>();

        animCmdLookup = state.GetComponentLookup<AnimationCmdData>();
        animStateLookup = state.GetComponentLookup<AnimationStateData>();
    }

    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        new PutAllOnTerrainJob { collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld }.Schedule();
        moveClips = SystemAPI.GetSingleton<AnimDbRefData>().FindClips("Movement");
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

        animCmdLookup.Update(ref state);
        animStateLookup.Update(ref state);


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

        new ChangeTargetJob 
        { 
            raycastResult = raycastResult,
            shiftTargeting = inputData.shiftTargeting,
            animCmdLookup = animCmdLookup,
            animStateLookup = animStateLookup,
            moveClips = moveClips 
        }.Schedule();

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

    public ComponentLookup<AnimationCmdData> animCmdLookup;
    [ReadOnly] public ComponentLookup<AnimationStateData> animStateLookup;
    [ReadOnly] public NativeArray<AnimDbEntry> moveClips;
    public void Execute (ref MovementComponent movementComponent, DynamicBuffer<MovementCommandsBuffer> moveComBuf, in SelectTag selectTag, in DynamicBuffer<ModelsBuffer> modelsBuf, in AttackSettingsComponent attackSets)
    {
        var result = raycastResult.Value;
        if (!result.hasHit)
            return;
        if (shiftTargeting && movementComponent.hasMoveTarget)
        {
            if (!moveComBuf.IsEmpty)
                moveComBuf.Add(new MovementCommandsBuffer 
                { 
                    target = result.raycastHitInfo.Position, 
                    //Copying the previous settings by default
                    targettingMinHP = moveComBuf[moveComBuf.Length - 1].targettingMinHP, 
                    shootingOnMoveMode = moveComBuf[moveComBuf.Length - 1].shootingOnMoveMode 
                });
            else
                moveComBuf.Add(new MovementCommandsBuffer
                {
                    target = result.raycastHitInfo.Position,
                    //Copying the current settings by default
                    targettingMinHP = attackSets.shootingOnMoveMode,
                    shootingOnMoveMode = attackSets.shootingOnMoveMode
                });
        }
        else
        {
            moveComBuf.Clear();
            movementComponent.target = result.raycastHitInfo.Position;
            movementComponent.hasMoveTarget = true;
            if (attackSets.isAbleToMove)
            {
                //Play move anim
                foreach (var modelBufElem in modelsBuf)
                {
                    RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
                    animCmd.ValueRW.ClipIndex = moveClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                    animCmd.ValueRW.Cmd = AnimationCmd.SetPlayForever;
                }
            }
        }
    }
}
