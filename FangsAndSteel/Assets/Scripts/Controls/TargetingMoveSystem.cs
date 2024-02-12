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
using Unity.Burst.Intrinsics;

[UpdateInGroup(typeof(ControlsSystemGroup), OrderFirst = true)]
[UpdateAfter(typeof(ControlSystem))]
[BurstCompile]
public partial struct TargetingMoveSystem : ISystem, ISystemStartStop
{
    private InputData inputData;

    ComponentLookup<AnimationCmdData> animCmdLookup;
    ComponentLookup<AnimationStateData> animStateLookup;
    NativeArray<AnimDbEntry> moveClips;

    EntityQuery changeTargetJobQuery;

    ComponentTypeHandle<MovementComponent> movementTypeHandle;
    ComponentTypeHandle<BattleModeComponent> battleModeTypeHandleRO;
    ComponentTypeHandle<PursuingModeComponent> pursuingModeTypeHandleRO;
    BufferTypeHandle<MovementCommandsBuffer> moveComsBuffTypeHandle;
    BufferTypeHandle<ModelsBuffer> modelsBuffsTypeHandle;

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

        changeTargetJobQuery = new EntityQueryBuilder(Allocator.Temp).
            WithAllRW<MovementComponent, MovementCommandsBuffer>().
            WithAll<ModelsBuffer>().
            WithAny<BattleModeComponent, PursuingModeComponent>().
            WithAll<SelectTag>().
            Build(ref state);

        movementTypeHandle = SystemAPI.GetComponentTypeHandle<MovementComponent>();
        battleModeTypeHandleRO = SystemAPI.GetComponentTypeHandle<BattleModeComponent>(true);
        pursuingModeTypeHandleRO = SystemAPI.GetComponentTypeHandle<PursuingModeComponent>(true);
        moveComsBuffTypeHandle = SystemAPI.GetBufferTypeHandle<MovementCommandsBuffer>();
        modelsBuffsTypeHandle = SystemAPI.GetBufferTypeHandle<ModelsBuffer>(true);
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

        movementTypeHandle.Update(ref state);
        battleModeTypeHandleRO.Update(ref state);
        pursuingModeTypeHandleRO.Update(ref state);
        moveComsBuffTypeHandle.Update(ref state);
        modelsBuffsTypeHandle.Update(ref state);


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

        JobHandle targetRayCastJobHandle = new TargetRayCastJob
        {
            collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld,
            raycastInput = raycastInput,
            raycastResult = raycastResult
        }.Schedule(state.Dependency);

        state.Dependency = new _ChangeTargetJob 
        { 
            raycastResult = raycastResult,
            shiftTargeting = inputData.shiftTargeting,

            animCmdLookup = animCmdLookup,
            animStateLookup = animStateLookup,
            moveClips = moveClips,

            movementTypeHandle = movementTypeHandle,
            battleModeTypeHandleRO = battleModeTypeHandleRO,
            pursuingModeTypeHandleRO = pursuingModeTypeHandleRO,
            moveComsBuffTypeHandle = moveComsBuffTypeHandle,
            modelsBuffTypeHandle = modelsBuffsTypeHandle
        }.Schedule(changeTargetJobQuery, targetRayCastJobHandle);

        raycastResult.Dispose(state.Dependency);
    }
}



///<summary> Changes targets for all selected units if raycast of new target was succesfull </summary>
[BurstCompile]
public partial struct _ChangeTargetJob : IJobChunk
{
    [ReadOnly]
    public NativeReference<RaycastResult> raycastResult;
    public bool shiftTargeting;

    public ComponentLookup<AnimationCmdData> animCmdLookup;
    [ReadOnly] public ComponentLookup<AnimationStateData> animStateLookup;
    [ReadOnly] public NativeArray<AnimDbEntry> moveClips;

    public ComponentTypeHandle<MovementComponent> movementTypeHandle;
    [ReadOnly] public ComponentTypeHandle<BattleModeComponent> battleModeTypeHandleRO;
    [ReadOnly] public ComponentTypeHandle<PursuingModeComponent> pursuingModeTypeHandleRO;
    public BufferTypeHandle<MovementCommandsBuffer> moveComsBuffTypeHandle;
    [ReadOnly] public BufferTypeHandle<ModelsBuffer> modelsBuffTypeHandle;

    public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
    {
        var result = raycastResult.Value;
        if (!result.hasHit)
            return;

        unsafe
        {
            MovementComponent* movements = chunk.GetComponentDataPtrRW(ref movementTypeHandle);
            NativeArray<BattleModeComponent> battleModeSettsArr = chunk.GetNativeArray(ref battleModeTypeHandleRO);
            NativeArray<PursuingModeComponent> pursuingModeSettsArr = chunk.GetNativeArray(ref pursuingModeTypeHandleRO);
            BufferAccessor<MovementCommandsBuffer> moveComsBuffs = chunk.GetBufferAccessor(ref moveComsBuffTypeHandle);
            BufferAccessor<ModelsBuffer> modelsBuffs = chunk.GetBufferAccessor(ref modelsBuffTypeHandle);

            ChunkEntityEnumerator chunkEnum = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);

            while (chunkEnum.NextEntityIndex(out int i))
            {
                if (shiftTargeting && movements[i].hasMoveTarget)
                {
                    if (!moveComsBuffs[i].IsEmpty)
                    { //Add new command and copy the settings of order before (n - 1 order)
                        moveComsBuffs[i].Add(new MovementCommandsBuffer
                        {
                            target = result.raycastHitInfo.Position,
                            battleModeSetts = moveComsBuffs[i].ElementAt(moveComsBuffs[i].Length - 1).battleModeSetts,
                            pursuingModeSetts = moveComsBuffs[i].ElementAt(moveComsBuffs[i].Length - 1).pursuingModeSetts,
                            ///TODO: StealthModeSetts
                        });
                    }
                    else
                        moveComsBuffs[i].Add(new MovementCommandsBuffer
                        { //Add new command and copy the settings of zero order
                            target = result.raycastHitInfo.Position,
                            battleModeSetts = battleModeSettsArr[i],
                            pursuingModeSetts = pursuingModeSettsArr[i],
                            ///TODO: StealthModeSetts
                        });
                }
                else
                { // all previous commands are cleared and zero command is added. Settings are same as before
                    moveComsBuffs[i].Clear();
                    movements[i].target = result.raycastHitInfo.Position;
                    movements[i].hasMoveTarget = true;
                    if (movements[i].isAbleToMove)
                    {
                        //Play move anim
                        foreach (var modelBufElem in modelsBuffs[i])
                        {
                            RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
                            animCmd.ValueRW.ClipIndex = moveClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                            animCmd.ValueRW.Cmd = AnimationCmd.SetPlayForever;
                        }
                    }
                }
            }
            
        }
    }
}
