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

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NotAbleToMoveForTimeRqstComponent>();

        animCmdLookup = state.GetComponentLookup<AnimationCmdData>();
        animStateLookup = state.GetComponentLookup<AnimationStateData>();
    }

    public void OnStartRunning(ref SystemState state)
    {
        moveClips = SystemAPI.GetSingleton<AnimDbRefData>().FindClips("Movement");
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
            moveClips = moveClips
        }.Schedule(state.Dependency);
    }
}


public partial struct NotAbleToMoveForTimeJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ecb;
    public float deltaTime;

    public ComponentLookup<AnimationCmdData> animCmdLookup;
    [ReadOnly] public ComponentLookup<AnimationStateData> animStateLookup;
    [ReadOnly] public NativeArray<AnimDbEntry> moveClips;

    public void Execute(ref NotAbleToMoveForTimeRqstComponent rqst, ref MovementComponent movement,
        in DynamicBuffer<ModelsBuffer> modelsBuf, Entity entity, [ChunkIndexInQuery] int chunkIndexInQuery)
    {
        rqst.passedTime += deltaTime;
        if (rqst.passedTime >= rqst.targetTime)
        {
            movement.isAbleToMove = true;
            foreach (var modelBufElem in modelsBuf)
            {
                RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
                animCmd.ValueRW.ClipIndex = moveClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                animCmd.ValueRW.Cmd = AnimationCmd.SetPlayForever;
                Debug.Log("changed to movement anim");
            }
            ecb.RemoveComponent<NotAbleToMoveForTimeRqstComponent>(chunkIndexInQuery, entity);
        }
    }

}