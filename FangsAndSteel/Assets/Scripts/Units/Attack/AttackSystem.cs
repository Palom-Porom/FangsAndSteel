using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

//All attacks are processed with a latency in 1 frame. May be there is a better solution?..
//Not sure about using InitializationGroup as there are a lot of .Complete()-s, as I suppose
[UpdateAfter(typeof(AttackTargetingSystem))]
[BurstCompile]
public partial struct AttackSystem : ISystem
{
    ComponentLookup<HpComponent> hpLookup;
    ComponentLookup<LocalToWorld> localToWorldLookup;
    ComponentLookup<TeamComponent> teamLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<HpComponent>();
        hpLookup = state.GetComponentLookup<HpComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        hpLookup.Update(ref state);
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        new AttackJob { hpLookup = hpLookup, ecb = ecb }.Schedule();
    }
}

[BurstCompile]
public partial struct AttackJob : IJobEntity
{
    public ComponentLookup<HpComponent> hpLookup;
    public EntityCommandBuffer.ParallelWriter ecb;
    public void Execute(in AttackRequestComponent attackRequest, Entity requestEntity, [ChunkIndexInQuery] int chunkIndexInQuery)
    {
        hpLookup.GetRefRW(attackRequest.target).ValueRW.curHp -= attackRequest.damage;
        ecb.DestroyEntity(chunkIndexInQuery, requestEntity);
    }
}