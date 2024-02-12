using AnimCooker;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public partial struct NotAbleToMoveForTimeSystem : ISystem, ISystemStartStop
{
    ComponentLookup<AnimationCmdData> animCmdLookup;
    ComponentLookup<AnimationStateData> animStateLookup;
    NativeArray<AnimDbEntry> moveClips;
    NativeArray<AnimDbEntry> restClips;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NotAbleToMoveForTimeRqstComponent>();

        animCmdLookup = state.GetComponentLookup<AnimationCmdData>();
        animStateLookup = state.GetComponentLookup<AnimationStateData>();
    }

    public void OnStartRunning(ref SystemState state)
    {
        moveClips = SystemAPI.GetSingleton<AnimDbRefData>().FindClips("Movement");
        restClips = SystemAPI.GetSingleton<AnimDbRefData>().FindClips("Rest");
    }

    public void OnStopRunning(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        animCmdLookup.Update(ref state);
        animStateLookup.Update(ref state);
        state.Dependency = new NotAbleToMoveForTimeJob 
        { 
            deltaTime = SystemAPI.Time.DeltaTime, 
            ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),

            animCmdLookup = animCmdLookup,
            animStateLookup = animStateLookup,
            moveClips = moveClips,
            restClips = restClips
        }.Schedule(state.Dependency);
    }
}


[WithNone(typeof(AttackModelsBuffer))]
public partial struct NotAbleToMoveForTimeJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ecb;
    public float deltaTime;

    public ComponentLookup<AnimationCmdData> animCmdLookup;
    [ReadOnly] public ComponentLookup<AnimationStateData> animStateLookup;
    [ReadOnly] public NativeArray<AnimDbEntry> moveClips;
    [ReadOnly] public NativeArray<AnimDbEntry> restClips;

    public void Execute(ref NotAbleToMoveForTimeRqstComponent rqst, ref MovementComponent movement,
        in DynamicBuffer<ModelsBuffer> modelsBuf, Entity entity, [ChunkIndexInQuery] int chunkIndexInQuery)
    {
        rqst.passedTime += deltaTime;
        if (rqst.passedTime >= rqst.targetTime)
        {
            movement.isAbleToMove = true;

            if (movement.hasMoveTarget)
            {
                foreach (var modelBufElem in modelsBuf)
                {
                    RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
                    animCmd.ValueRW.ClipIndex = moveClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                    animCmd.ValueRW.Cmd = AnimationCmd.SetPlayForever;
                }
            }
            else
            {
                foreach (var modelBufElem in modelsBuf)
                {
                    RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
                    animCmd.ValueRW.ClipIndex = restClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                    animCmd.ValueRW.Cmd = AnimationCmd.SetPlayForever;
                }
            }
            ecb.RemoveComponent<NotAbleToMoveForTimeRqstComponent>(chunkIndexInQuery, entity);
        }
    }
}



public partial struct ChangeAnimAfterReloadJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ecb;
    public float deltaTime;

    public ComponentLookup<AnimationCmdData> animCmdLookup;
    [ReadOnly] public ComponentLookup<AnimationStateData> animStateLookup;
    [ReadOnly] public NativeArray<AnimDbEntry> moveClips;
    [ReadOnly] public NativeArray<AnimDbEntry> restClips;

    public void Execute(ref NotAbleToMoveForTimeRqstComponent rqst, ref MovementComponent movement, 
        in DynamicBuffer<AttackModelsBuffer> attackModels, Entity entity, [ChunkIndexInQuery] int chunkIndexInQuery)
    {
        rqst.passedTime += deltaTime;
        if (rqst.passedTime >= rqst.targetTime)
        {
            if (movement.hasMoveTarget)
            {
                foreach (var modelBufElem in attackModels)
                {
                    RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
                    animCmd.ValueRW.ClipIndex = moveClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                    animCmd.ValueRW.Cmd = AnimationCmd.SetPlayForever;
                }
            }
            else
            {
                foreach (var modelBufElem in attackModels)
                {
                    RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
                    animCmd.ValueRW.ClipIndex = restClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                    animCmd.ValueRW.Cmd = AnimationCmd.SetPlayForever;
                }
            }
            ecb.RemoveComponent<NotAbleToMoveForTimeRqstComponent>(chunkIndexInQuery, entity);
        }
    }
}