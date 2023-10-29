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

[UpdateInGroup(typeof(SimulationSystemGroup),OrderFirst = true)]
[BurstCompile]
public partial struct AttackTargetingSystem : ISystem
{
    ComponentLookup<HpComponent> hpLookup;
    ComponentLookup<LocalToWorld> localToWorldLookup;
    ComponentLookup<TeamComponent> teamLookup;

    EntityQuery potentialTargetsQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AttackComponent>();
        state.RequireForUpdate<HpComponent>();

        hpLookup = state.GetComponentLookup<HpComponent>(true);
        localToWorldLookup = state.GetComponentLookup<LocalToWorld>(true);
        teamLookup = state.GetComponentLookup<TeamComponent>(true);

        potentialTargetsQuery = new EntityQueryBuilder(Allocator.TempJob).WithAll<HpComponent, LocalToWorld, TeamComponent>().Build(ref state);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        hpLookup.Update(ref state);
        localToWorldLookup.Update(ref state);
        teamLookup.Update(ref state);
        NativeArray<Entity> potentialTargetsArr = potentialTargetsQuery.ToEntityArray(Allocator.TempJob);

        var ecb =  SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

        AttackTargetingJob attackTargetingJob = new AttackTargetingJob
        {
            hpLookup = hpLookup,
            localToWorldLookup = localToWorldLookup,
            teamLookup = teamLookup,
            potentialTargetsArr = potentialTargetsArr,
            ecb = ecb,
            deltaTime = Time.deltaTime  
        };
        state.Dependency = attackTargetingJob.Schedule(state.Dependency);

        //World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>().AddJobHandleForProducer(state.Dependency);
    }
}


/// <summary>
/// Checks all current attack targets and searches for new ones
/// </summary>
[BurstCompile]
public partial struct AttackTargetingJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<HpComponent> hpLookup;
    [ReadOnly] public ComponentLookup<LocalToWorld> localToWorldLookup;
    [ReadOnly] public ComponentLookup<TeamComponent> teamLookup;

    [ReadOnly] public NativeArray<Entity> potentialTargetsArr;

    public EntityCommandBuffer.ParallelWriter ecb;

    public float deltaTime;

    public void Execute(ref AttackComponent attack, in LocalToWorld localToWorld, in TeamComponent team, [ChunkIndexInQuery] int chunkIndexInQuery)
    {
        //For now put the reloading here, but maybe then it is a good idea to put it in another Job with updating all units characteristics (MAYBE for example hp from healing)
        attack.curReload += deltaTime;
        if (attack.curReload - attack.reloadLen > float.Epsilon)
            attack.curReload = attack.reloadLen;

        //If not reloaded yet then no need for search for target
        if (math.abs(attack.curReload - attack.reloadLen) > float.Epsilon)
            return;

        //If has valid target -> create an attackRequest and return
        if (attack.target != Entity.Null
            && math.distancesq(localToWorld.Position, localToWorldLookup[attack.target].Position) <= attack.radiusSq
            && hpLookup[attack.target].curHp > 0)
        {
            Entity attackRequest = ecb.CreateEntity(chunkIndexInQuery);
            ecb.AddComponent(chunkIndexInQuery, attackRequest, new AttackRequestComponent { target = attack.target, damage = attack.damage });
            attack.curReload = 0;
            return;
        }

        //else -> find new target
        attack.target = Entity.Null;

        float bestScore = float.MinValue;
        Entity bestScoreEntity = Entity.Null;
        foreach(Entity potentialTarget in potentialTargetsArr)
        {
            //Check if they are in different teams
            if (teamLookup[potentialTarget].teamInd - team.teamInd == 0)
                continue;

            float curScore = 0;

            //Temporary check if the target is already dead
            //I suppose it will be removed after we make a processing of dead units (destroying them)
            if (hpLookup[potentialTarget].curHp <= 0)
                continue;

            float distanceScore = attack.radiusSq - math.distancesq(localToWorld.Position, localToWorldLookup[potentialTarget].Position)/* * distScoreMultiplier*/;
            //Check if target is not in the attack radius
            if (distanceScore < 0) 
                continue;
            curScore += distanceScore;
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
            Entity attackRequest = ecb.CreateEntity(chunkIndexInQuery);
            ecb.AddComponent(chunkIndexInQuery, attackRequest, new AttackRequestComponent { target = bestScoreEntity, damage = attack.damage });
            attack.curReload = 0;
        }
    }
}


public struct AttackRequestComponent : IComponentData
{
    public Entity target;
    public int damage;
}
