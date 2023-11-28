using AnimCooker;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(UnitsSystemGroup))]
[BurstCompile]
public partial struct TargetingAttackSystem : ISystem, ISystemStartStop
{
    ComponentLookup<HpComponent> hpLookup;
    ComponentLookup<LocalToWorld> localToWorldLookup;
    ComponentLookup<TeamComponent> teamLookup;
    ComponentLookup<FillFloatOverride> fillBarLookup;

    EntityQuery potentialTargetsQuery;

    ComponentLookup<AnimationCmdData> animCmdLookup;
    ComponentLookup<AnimationStateData> animStateLookup;
    NativeArray<AnimDbEntry> attackClips;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AttackComponent>();
        state.RequireForUpdate<HpComponent>();

        hpLookup = state.GetComponentLookup<HpComponent>(true);
        localToWorldLookup = state.GetComponentLookup<LocalToWorld>(true);
        teamLookup = state.GetComponentLookup<TeamComponent>(true);
        fillBarLookup = state.GetComponentLookup<FillFloatOverride>();

        potentialTargetsQuery = new EntityQueryBuilder(Allocator.TempJob).WithAll<HpComponent, LocalToWorld, TeamComponent>().Build(ref state);

        animCmdLookup = state.GetComponentLookup<AnimationCmdData>();
        animStateLookup = state.GetComponentLookup<AnimationStateData>();
    }

    public void OnStartRunning(ref SystemState state)
    {
        attackClips = SystemAPI.GetSingleton<AnimDbRefData>().FindClips("Attack");
    }

    public void OnStopRunning(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        hpLookup.Update(ref state);
        localToWorldLookup.Update(ref state);
        teamLookup.Update(ref state);
        fillBarLookup.Update(ref state);
        animCmdLookup.Update(ref state);
        animStateLookup.Update(ref state);
        NativeArray<Entity> potentialTargetsArr = potentialTargetsQuery.ToEntityArray(Allocator.TempJob);

        var ecb =  SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

        AttackTargetingJob attackTargetingJob = new AttackTargetingJob
        {
            hpLookup = hpLookup,
            localToWorldLookup = localToWorldLookup,
            teamLookup = teamLookup,
            fillBarLookup = fillBarLookup,

            potentialTargetsArr = potentialTargetsArr,

            ecb = ecb,
            deltaTime = Time.deltaTime,

            animCmdLookup = animCmdLookup,
            animStateLookup = animStateLookup,
            attackClips = attackClips
        };
        state.Dependency = attackTargetingJob.Schedule(state.Dependency);

        //World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>().AddJobHandleForProducer(state.Dependency);
    }
}

//If ComponentLookups are heavy indeed, then it is better to rewrite as 3 jobs connected with NativeReferences<info>
/// <summary>
/// Checks all current attack targets and searches for new ones
/// </summary>
[BurstCompile]
public partial struct AttackTargetingJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<HpComponent> hpLookup;
    [ReadOnly] public ComponentLookup<LocalToWorld> localToWorldLookup;
    [ReadOnly] public ComponentLookup<TeamComponent> teamLookup;
    public ComponentLookup<FillFloatOverride> fillBarLookup;

    [ReadOnly] public NativeArray<Entity> potentialTargetsArr;

    public EntityCommandBuffer.ParallelWriter ecb;

    public float deltaTime;

    private float modHp;
    private float modDist;

    public ComponentLookup<AnimationCmdData> animCmdLookup;
    [ReadOnly] public ComponentLookup<AnimationStateData> animStateLookup;
    [ReadOnly] public NativeArray<AnimDbEntry> attackClips;

    public void Execute(ref AttackComponent attack, in AttackSettingsComponent attackSettings, in LocalToWorld localToWorld, in TeamComponent team, in UnitsIconsComponent unitsIcons, [ChunkIndexInQuery] int chunkIndexInQuery, in DynamicBuffer<ModelsBuffer> modelsBuf)
    {
        //For now put the reloading here, but maybe then it is a good idea to put it in another Job with updating all units characteristics (MAYBE for example hp from healing)
        attack.curReload += deltaTime;
        if (attack.curReload - attack.reloadLen > float.Epsilon)
            attack.curReload = attack.reloadLen;
        fillBarLookup.GetRefRW(unitsIcons.reloadBarEntity).ValueRW.Value = attack.curReload / attack.reloadLen;

        //If not reloaded yet then no need for search for target
        if (math.abs(attack.curReload - attack.reloadLen) > float.Epsilon)
            return;

        //If has valid target -> create an attackRequest and return
        if (attack.target != Entity.Null
            && math.distancesq(localToWorld.Position, localToWorldLookup[attack.target].Position) <= attack.radiusSq
            && hpLookup.HasComponent(attack.target))
        {
            CreateAttackRequest(chunkIndexInQuery, attack.target, attack.damage, modelsBuf);
            //Entity attackRequest = ecb.CreateEntity(chunkIndexInQuery);
            //ecb.AddComponent(chunkIndexInQuery, attackRequest, new AttackRequestComponent { target = attack.target, damage = attack.damage });
            attack.curReload = 0;
            return;
        }

        //else -> find new target
        attack.target = Entity.Null;

        float bestScore = float.MinValue;
        Entity bestScoreEntity = Entity.Null;

        if (attackSettings.targettingMinHP)
        {
            modHp = 10000;
            modDist = 0.1f;
        }

        foreach(Entity potentialTarget in potentialTargetsArr)
        {
            //Check if they are in different teams
            if (teamLookup[potentialTarget].teamInd - team.teamInd == 0)
                continue;

            float curScore = 0;

            float distanceScore = attack.radiusSq - math.distancesq(localToWorld.Position, localToWorldLookup[potentialTarget].Position)/* * distScoreMultiplier*/;
            //Check if target is not in the attack radius
            if (distanceScore < 0) 
                continue;
            curScore += distanceScore * modDist;

            float hpScore = -(hpLookup[potentialTarget].curHp);
            curScore += hpScore * modHp;
            ///TODO Other score affectors

            if (curScore > bestScore)
            {
                bestScore = curScore;
                bestScoreEntity = potentialTarget;
            }
        }
        attack.target = bestScoreEntity;

        if (bestScoreEntity != Entity.Null)
        {
            CreateAttackRequest(chunkIndexInQuery, bestScoreEntity, attack.damage, modelsBuf);
            attack.curReload = 0;
            //Entity attackRequest = ecb.CreateEntity(chunkIndexInQuery);
            //ecb.AddComponent(chunkIndexInQuery, attackRequest, new AttackRequestComponent { target = bestScoreEntity, damage = attack.damage });
            //attack.curReload = 0;

            ////Play Attack Anim
            //foreach (var modelBufElem in modelsBuf)
            //{
            //    RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
            //    animCmd.ValueRW.ClipIndex = attackClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
            //    animCmd.ValueRW.Cmd = AnimationCmd.PlayOnce;
            //}
        }
    }

    private void CreateAttackRequest(int chunkIndexInQuery, Entity target, int damage, in DynamicBuffer<ModelsBuffer> modelsBuf)
    {
        Entity attackRequest = ecb.CreateEntity(chunkIndexInQuery);
        ecb.AddComponent(chunkIndexInQuery, attackRequest, new AttackRequestComponent { target = target, damage = damage });

        //Play Attack Anim
        foreach (var modelBufElem in modelsBuf)
        {
            RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
            animCmd.ValueRW.ClipIndex = attackClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
            animCmd.ValueRW.Cmd = AnimationCmd.PlayOnce;
        }
    }
}


public struct AttackRequestComponent : IComponentData
{
    public Entity target;
    public int damage;
}
