using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using AnimCooker;
using Unity.Mathematics;

//All attacks are processed with a latency in 1 frame. May be there is a better solution?..
[UpdateInGroup(typeof(UnitsSystemGroup))]
[UpdateAfter(typeof(TargetingAttackSystem))]
[BurstCompile]
public partial struct AttackSystem : ISystem, ISystemStartStop
{
    ComponentLookup<HpComponent> hpLookup;
    ComponentLookup<FillFloatOverride> fillBarLookup;
    ComponentLookup<UnitIconsComponent> unitsIconsLookup;
    BufferLookup<Child> childrenLookup;

    ComponentLookup<LocalTransform> localTransformLookup;
    ComponentLookup<AnimationCmdData> animCmdLookup;
    ComponentLookup<AnimationStateData> animStateLookup;
    BufferLookup<ModelsBuffer> modelBufLookup;
    NativeArray<AnimDbEntry> deathClips;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AttackRequestComponent>();
        state.RequireForUpdate<AnimDbRefData>();

        hpLookup = state.GetComponentLookup<HpComponent>();
        fillBarLookup = state.GetComponentLookup<FillFloatOverride>();
        unitsIconsLookup = state.GetComponentLookup<UnitIconsComponent>(true);
        childrenLookup = state.GetBufferLookup<Child>(true);

        localTransformLookup = state.GetComponentLookup<LocalTransform>();
        animCmdLookup = state.GetComponentLookup<AnimationCmdData>();
        animStateLookup = state.GetComponentLookup<AnimationStateData>();
        modelBufLookup = state.GetBufferLookup<ModelsBuffer>();
    }

    public void OnStartRunning(ref SystemState state)
    {
        deathClips = SystemAPI.GetSingleton<AnimDbRefData>().FindClips("Death_2");        
    }

    public void OnStopRunning(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        hpLookup.Update(ref state);
        fillBarLookup.Update(ref state);
        unitsIconsLookup.Update(ref state);
        childrenLookup.Update(ref state);
        localTransformLookup.Update(ref state);
        animCmdLookup.Update(ref state);
        animStateLookup.Update(ref state);
        modelBufLookup.Update(ref state);
        var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        new DealingDamageJob 
        { 
            hpLookup = hpLookup, 
            fillBarLookup = fillBarLookup, 
            unitsIconsLookup = unitsIconsLookup, 
            childrenLookup = childrenLookup,
            ecb = ecb,

            localTransformLookup = localTransformLookup,
            animCmdLookup = animCmdLookup,
            animStateLookup = animStateLookup,
            modelBufLookup = modelBufLookup,
            deathClips = deathClips
        }.Schedule();

        //foreach((MovementComponent move, Entity entity) in SystemAPI.Query<MovementComponent>().WithEntityAccess() )
        //{
            
        //}
    }
}

//[BurstCompile]
public partial struct DealingDamageJob : IJobEntity
{
    public ComponentLookup<HpComponent> hpLookup;
    public ComponentLookup<FillFloatOverride> fillBarLookup;
    [ReadOnly] public ComponentLookup<UnitIconsComponent> unitsIconsLookup;
    [ReadOnly] public BufferLookup<Child> childrenLookup;
    public EntityCommandBuffer.ParallelWriter ecb;

    public ComponentLookup<LocalTransform> localTransformLookup;
    public ComponentLookup<AnimationCmdData> animCmdLookup;
    [ReadOnly] public ComponentLookup<AnimationStateData> animStateLookup;
    [ReadOnly] public BufferLookup<ModelsBuffer> modelBufLookup;
    public NativeArray<AnimDbEntry> deathClips;

    public void Execute(in AttackRequestComponent attackRequest, Entity requestEntity, [ChunkIndexInQuery] int chunkIndexInQuery)
    {
        RefRW<HpComponent> hpComponent = hpLookup.GetRefRW(attackRequest.target);
        //Decrease health
        hpComponent.ValueRW.curHp -= attackRequest.damage;
        //Update HealthBar
        fillBarLookup.GetRefRW(unitsIconsLookup[attackRequest.target].healthBarEntity).ValueRW.Value = (float)hpComponent.ValueRO.curHp / hpComponent.ValueRO.maxHp;

        //Killing the functionality of Unit and setting request for delayed physcial death (DeadComponent)
        if (hpComponent.ValueRO.curHp <= 0)
        {
            ecb.AddComponent(chunkIndexInQuery, attackRequest.target, new DeadComponent { timeToDie = hpComponent.ValueRO.timeToDie});

            ecb.RemoveComponent<HpComponent>(chunkIndexInQuery, attackRequest.target);
            ecb.RemoveComponent<AttackCharsComponent>(chunkIndexInQuery, attackRequest.target);
            ecb.RemoveComponent<BattleModeComponent>(chunkIndexInQuery, attackRequest.target);
            ecb.RemoveComponent<MovementComponent>(chunkIndexInQuery, attackRequest.target);
            ecb.RemoveComponent<MovementCommandsBuffer>(chunkIndexInQuery, attackRequest.target);
            ecb.RemoveComponent<VisibilityComponent>(chunkIndexInQuery, attackRequest.target);
            ecb.RemoveComponent<VisionCharsComponent>(chunkIndexInQuery, attackRequest.target);
            ecb.RemoveComponent<TeamComponent>(chunkIndexInQuery, attackRequest.target);
            ///TODO: Redo the line below as it shouldn't use children unstable nature
            ecb.DestroyEntity(chunkIndexInQuery, childrenLookup[attackRequest.target][1].Value); //Destroying Selection Ring (it must be a child with index 1!) 
            ecb.RemoveComponent<SelectTag>(chunkIndexInQuery, attackRequest.target);
            //Destroying all Unit Icons
            UtilityFuncs.DestroyParentAndAllChildren(ecb, childrenLookup, unitsIconsLookup[attackRequest.target].infoQuadsEntity, chunkIndexInQuery);
            ecb.RemoveComponent<UnitIconsComponent>(chunkIndexInQuery, attackRequest.target);
            ecb.RemoveComponent<UnitStatsRequestTag>(chunkIndexInQuery, attackRequest.target);

            //Play Death anim
            //localTransformLookup.GetRefRW(attackRequest.target).ValueRW.Rotation = quaternion.LookRotationSafe(attackRequest.attackerPos, localTransformLookup[attackRequest.target].Up());
            float maxTimeToDie = float.MinValue;
            if (modelBufLookup.HasBuffer(attackRequest.target))
            {
                foreach (var modelBufElem in modelBufLookup[attackRequest.target])
                {
                    AnimDbEntry deathClip = deathClips[animStateLookup[modelBufElem.model].ModelIndex];
                    RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
                    animCmd.ValueRW.ClipIndex = deathClip.ClipIndex;
                    animCmd.ValueRW.Cmd = AnimationCmd.PlayOnceAndStop;
                    maxTimeToDie = math.max(maxTimeToDie, deathClip.GetLength());
                }
            }
            
        }

        ecb.DestroyEntity(chunkIndexInQuery, requestEntity);
    }
}