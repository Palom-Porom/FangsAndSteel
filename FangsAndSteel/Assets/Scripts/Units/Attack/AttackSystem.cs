using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEditor.SceneManagement;
using UnityEngine;

//All attacks are processed with a latency in 1 frame. May be there is a better solution?..
//Not sure about using InitializationGroup as there are a lot of .Complete()-s, as I suppose
[UpdateAfter(typeof(TargetingAttackSystem))]
[BurstCompile]
public partial struct AttackSystem : ISystem
{
    ComponentLookup<HpComponent> hpLookup;
    ComponentLookup<FillFloatOverride> fillBarLookup;
    ComponentLookup<UnitsIconsComponent> unitsIconsLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AttackRequestComponent>();
        hpLookup = state.GetComponentLookup<HpComponent>();
        fillBarLookup = state.GetComponentLookup<FillFloatOverride>();
        unitsIconsLookup = state.GetComponentLookup<UnitsIconsComponent>(true);

    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        hpLookup.Update(ref state);
        fillBarLookup.Update(ref state);
        unitsIconsLookup.Update(ref state);
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        new AttackJob { hpLookup = hpLookup, fillBarLookup = fillBarLookup, unitsIconsLookup = unitsIconsLookup, ecb = ecb }.Schedule();

        foreach((MovementComponent move, Entity entity) in SystemAPI.Query<MovementComponent>().WithEntityAccess() )
        {
            
        }
    }
}

[BurstCompile]
public partial struct AttackJob : IJobEntity
{
    public ComponentLookup<HpComponent> hpLookup;
    public ComponentLookup<FillFloatOverride> fillBarLookup;
    [ReadOnly] public ComponentLookup<UnitsIconsComponent> unitsIconsLookup;
    public EntityCommandBuffer.ParallelWriter ecb;
    public void Execute(in AttackRequestComponent attackRequest, Entity requestEntity, [ChunkIndexInQuery] int chunkIndexInQuery)
    {
        RefRW<HpComponent> hpComponent = hpLookup.GetRefRW(attackRequest.target);
        //Decrease health
        hpComponent.ValueRW.curHp -= attackRequest.damage;
        //Update HealthBar
        fillBarLookup.GetRefRW(unitsIconsLookup[attackRequest.target].healthBarEntity).ValueRW.Value = hpComponent.ValueRO.curHp / hpComponent.ValueRO.maxHp;
        ecb.DestroyEntity(chunkIndexInQuery, requestEntity);
        //Killing Unit
        if (hpComponent.ValueRO.curHp <= 0)
        {
            ecb.RemoveComponent<HpComponent>(chunkIndexInQuery, attackRequest.target);
            ecb.RemoveComponent<TeamComponent>(chunkIndexInQuery, attackRequest.target);
            ecb.RemoveComponent<AttackComponent>(chunkIndexInQuery, attackRequest.target);
            ecb.RemoveComponent<MovementComponent>(chunkIndexInQuery, attackRequest.target);
        }
    }
}