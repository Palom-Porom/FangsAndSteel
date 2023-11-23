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
    BufferLookup<Child> childrenLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AttackRequestComponent>();

        hpLookup = state.GetComponentLookup<HpComponent>();
        fillBarLookup = state.GetComponentLookup<FillFloatOverride>();
        unitsIconsLookup = state.GetComponentLookup<UnitsIconsComponent>(true);
        childrenLookup = state.GetBufferLookup<Child>(true);

    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        hpLookup.Update(ref state);
        fillBarLookup.Update(ref state);
        unitsIconsLookup.Update(ref state);
        childrenLookup.Update(ref state);
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        new AttackJob 
        { 
            hpLookup = hpLookup, 
            fillBarLookup = fillBarLookup, 
            unitsIconsLookup = unitsIconsLookup, 
            childrenLookup = childrenLookup,
            ecb = ecb 
        }.Schedule();

        //foreach((MovementComponent move, Entity entity) in SystemAPI.Query<MovementComponent>().WithEntityAccess() )
        //{
            
        //}
    }
}

[BurstCompile]
public partial struct AttackJob : IJobEntity
{
    public ComponentLookup<HpComponent> hpLookup;
    public ComponentLookup<FillFloatOverride> fillBarLookup;
    [ReadOnly] public ComponentLookup<UnitsIconsComponent> unitsIconsLookup;
    [ReadOnly] public BufferLookup<Child> childrenLookup;
    public EntityCommandBuffer.ParallelWriter ecb;
    public void Execute(in AttackRequestComponent attackRequest, Entity requestEntity, [ChunkIndexInQuery] int chunkIndexInQuery)
    {
        RefRW<HpComponent> hpComponent = hpLookup.GetRefRW(attackRequest.target);
        //Decrease health
        hpComponent.ValueRW.curHp -= attackRequest.damage;
        //Update HealthBar
        fillBarLookup.GetRefRW(unitsIconsLookup[attackRequest.target].healthBarEntity).ValueRW.Value = hpComponent.ValueRO.curHp / hpComponent.ValueRO.maxHp;
        //Killing the functionality of Unit and setting request for delayed physcial death (DeadComponent)
        if (hpComponent.ValueRO.curHp <= 0)
        {
            ecb.RemoveComponent<HpComponent>(chunkIndexInQuery, attackRequest.target);
            ecb.RemoveComponent<AttackComponent>(chunkIndexInQuery, attackRequest.target);
            ecb.RemoveComponent<MovementComponent>(chunkIndexInQuery, attackRequest.target);
            ecb.RemoveComponent<VisibilityComponent>(chunkIndexInQuery, attackRequest.target);
            ecb.RemoveComponent<VisionCharsComponent>(chunkIndexInQuery, attackRequest.target);
            ecb.RemoveComponent<TeamComponent>(chunkIndexInQuery, attackRequest.target);
            //Destroying Selection Ring (it must be a child with index 1!)
            ecb.DestroyEntity(chunkIndexInQuery, childrenLookup[attackRequest.target][1].Value);
            ecb.RemoveComponent<SelectTag>(chunkIndexInQuery, attackRequest.target);
            //Destroying all Unit Icons
            UtilityFuncs.DestroyParentAndAllChildren(ecb, childrenLookup, unitsIconsLookup[attackRequest.target].infoQuadsEntity, chunkIndexInQuery);
            ecb.RemoveComponent<UnitsIconsComponent>(chunkIndexInQuery, attackRequest.target);
            ecb.RemoveComponent<UnitStatsRequestComponent>(chunkIndexInQuery, attackRequest.target);
            ecb.AddComponent(chunkIndexInQuery, attackRequest.target, new DeadComponent { timeToDie = hpComponent.ValueRO.timeToDie });

        }
        ecb.DestroyEntity(chunkIndexInQuery, requestEntity);
    }

    public void OnDestroy(ref SystemState state)
    {
        Debug.Log("Destroyed Attack System");
    }
}