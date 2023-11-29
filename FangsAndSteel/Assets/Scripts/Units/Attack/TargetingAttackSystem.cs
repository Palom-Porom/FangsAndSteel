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
using static UnityEngine.EventSystems.EventTrigger;

[UpdateInGroup(typeof(UnitsSystemGroup))]
[BurstCompile]
public partial struct TargetingAttackSystem : ISystem, ISystemStartStop
{
    ComponentLookup<HpComponent> hpLookup;
    ComponentLookup<LocalToWorld> localToWorldLookup;
    ComponentLookup<TeamComponent> teamLookup;
    ComponentLookup<FillFloatOverride> fillBarLookup;

    EntityQuery usualUnitsQuery;
    EntityQuery deployableUnitsQuery;

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

        //usualUnitsQuery = state.GetEntityQuery
        //    (
        //    typeof(AttackComponent),
        //    typeof(AttackSettingsComponent),
        //    typeof(LocalToWorld),
        //    typeof(TeamComponent),
        //    typeof(UnitsIconsComponent)
        //    );
        usualUnitsQuery = new EntityQueryBuilder (Allocator.TempJob).WithAll< AttackComponent, AttackSettingsComponent, LocalToWorld, TeamComponent, UnitsIconsComponent, ModelsBuffer, MovementComponent > ().WithNone<Deployable>().Build(state.EntityManager);
        deployableUnitsQuery = new EntityQueryBuilder (Allocator.TempJob).WithAll< AttackComponent, AttackSettingsComponent, LocalToWorld, TeamComponent, UnitsIconsComponent, ModelsBuffer, Deployable > ().WithAll<MovementComponent>().Build(state.EntityManager);
        //deployableUnitsQuery = state.GetEntityQuery
        //    (
        //    typeof(AttackComponent),
        //    typeof(AttackSettingsComponent),
        //    typeof(LocalToWorld),
        //    typeof(TeamComponent),
        //    typeof(UnitsIconsComponent)
        //    );

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

        if (!usualUnitsQuery.IsEmpty)
        {
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
            state.Dependency = attackTargetingJob.Schedule(usualUnitsQuery, state.Dependency);
        }
        if (!deployableUnitsQuery.IsEmpty)
        {
            AttackTargetingDeployableJob attackTargetingDeployableJob = new AttackTargetingDeployableJob
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
            state.Dependency = attackTargetingDeployableJob.Schedule(deployableUnitsQuery, state.Dependency);
        }
    }
}

//If ComponentLookups are heavy indeed, then it is better to rewrite as 3 jobs connected with NativeReferences<info>
/// <summary>
/// Checks all current attack targets and searches for new ones
/// </summary>
//[BurstCompile]
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

    public void Execute(ref AttackComponent attack, ref AttackSettingsComponent attackSettings, in LocalToWorld localToWorld, in TeamComponent team, MovementComponent movement,
        in UnitsIconsComponent unitsIcons, [ChunkIndexInQuery] int chunkIndexInQuery, in DynamicBuffer<ModelsBuffer> modelsBuf, Entity entity)
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
            attackSettings.isAbleToMove = attackSettings.shootingOnMoveMode;
            CreateAttackRequest(chunkIndexInQuery, attack.target, attack.damage, modelsBuf);
            //Entity attackRequest = ecb.CreateEntity(chunkIndexInQuery);
            //ecb.AddComponent(chunkIndexInQuery, attackRequest, new AttackRequestComponent { target = attack.target, damage = attack.damage });
            attack.curReload = 0;
            return;
        }
        //else -> find new target

        if (attackSettings.targettingMinHP)
        {
            modHp = 10000;
            modDist = 0.1f;
        }

        attack.target = UtilityFuncs.FindBestTarget(in potentialTargetsArr, teamLookup, hpLookup, localToWorldLookup, localToWorld.Position, attack.radiusSq, team.teamInd, modDist, modHp);

        if (attack.target != Entity.Null)
        {
            attackSettings.isAbleToMove = attackSettings.shootingOnMoveMode;
            CreateAttackRequest(chunkIndexInQuery, attack.target, attack.damage, modelsBuf);
            attack.curReload = 0;
        }
        else
        {
            attackSettings.isAbleToMove = true;
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

public partial struct AttackTargetingDeployableJob : IJobEntity
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

    public void Execute(ref AttackComponent attack, ref AttackSettingsComponent attackSettings, in LocalToWorld localToWorld, in TeamComponent team, in MovementComponent movement,
        in UnitsIconsComponent unitsIcons, [ChunkIndexInQuery] int chunkIndexInQuery, in DynamicBuffer<ModelsBuffer> modelsBuf, ref Deployable deployable)
    {
        //Deploy Handle
        if (!deployable.deployedState)
        {
            if (deployable.deployTimeCur > 0)
            {
                deployable.deployTimeCur -= deltaTime;
                if (deployable.deployTimeCur <= 0)
                    attackSettings.isAbleToMove = true;
            }
        }
        else if (deployable.deployTimeCur < deployable.deployTimeMax)
        {
            deployable.deployTimeCur += deltaTime;
            return;
        }

        bool fullyDeployed = deployable.deployTimeCur >= deployable.deployTimeMax;

        if (fullyDeployed)
        {
            //For now put the reloading here, but maybe then it is a good idea to put it in another Job with updating all units characteristics (MAYBE for example hp from healing)
            attack.curReload += deltaTime;
            if (attack.curReload - attack.reloadLen > float.Epsilon)
                attack.curReload = attack.reloadLen;
            fillBarLookup.GetRefRW(unitsIcons.reloadBarEntity).ValueRW.Value = attack.curReload / attack.reloadLen;

            //If has valid target -> create an attackRequest and return
            if (attack.target != Entity.Null
                && math.abs(attack.curReload - attack.reloadLen) <= float.Epsilon
                && math.distancesq(localToWorld.Position, localToWorldLookup[attack.target].Position) <= attack.radiusSq
                && hpLookup.HasComponent(attack.target))
            {
                CreateAttackRequest(chunkIndexInQuery, attack.target, attack.damage, modelsBuf);
                attack.curReload = 0;
                return;
            }
        }

        //find new target

        if (attackSettings.targettingMinHP)
        {
            modHp = 10000;
            modDist = 0.1f;
        }


        attack.target = UtilityFuncs.FindBestTarget(potentialTargetsArr, teamLookup, hpLookup, localToWorldLookup, localToWorld.Position, attack.radiusSq, team.teamInd, modDist, modHp);


        if (attack.target != Entity.Null)
        {
            deployable.waitingTimeCur = 0;
            if (!deployable.deployedState)
            {
                deployable.deployedState = true;
                attackSettings.isAbleToMove = false;
            }
            else if (fullyDeployed && math.abs(attack.curReload - attack.reloadLen) <= float.Epsilon)
            {
                CreateAttackRequest(chunkIndexInQuery, attack.target, attack.damage, modelsBuf);
                attack.curReload = 0;
            }
        }
        else if (!movement.hasActualTarget)
        {
            if (deployable.waitingTimeCur < deployable.waitingTimeMax)
                deployable.waitingTimeCur += deltaTime;
            deployable.deployedState = (deployable.waitingTimeCur < deployable.waitingTimeMax);
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
