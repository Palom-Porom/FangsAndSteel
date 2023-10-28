using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct AttackTargetingSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AttackComponent>();
        state.RequireForUpdate<HpComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        ComponentLookup<HpComponent> hpLookup = state.GetComponentLookup<HpComponent>(true);
        ComponentLookup<LocalToWorld> localToWorldLookup = state.GetComponentLookup<LocalToWorld>(true);
        ComponentLookup<TeamComponent> teamLookup = state.GetComponentLookup<TeamComponent>(true);
        NativeArray<Entity> potentialTargetsArr = 
            new EntityQueryBuilder(Allocator.TempJob).WithAll<HpComponent, LocalToWorld, TeamComponent>().Build(ref state).ToEntityArray(Allocator.TempJob);

        AttackTargetingJob attackTargetingJob = new AttackTargetingJob
        {
            hpLookup = hpLookup,
            localToWorldLookup = localToWorldLookup,
            teamLookup = teamLookup,
            potentialTargetsArr = potentialTargetsArr
        };
        state.Dependency = attackTargetingJob.Schedule(state.Dependency);
    }
}


/// <summary>
/// Checks all current attack targets and searches for new ones
/// </summary>
public partial struct AttackTargetingJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<HpComponent> hpLookup;
    [ReadOnly] public ComponentLookup<LocalToWorld> localToWorldLookup;
    [ReadOnly] public ComponentLookup<TeamComponent> teamLookup;
    [ReadOnly] public NativeArray<Entity> potentialTargetsArr;


    public void Execute(ref AttackComponent attack, in LocalToWorld localToWorld, in TeamComponent team)
    {
        //If not reloaded yet then no need for search for target
        if (attack.curReload != attack.reloadLen)
            return;

        //If has valid target -> return
        if (attack.target != Entity.Null
            && math.distancesq(localToWorld.Position, localToWorldLookup[attack.target].Position) <= attack.radiusSq)
                return;

        //else -> find new target
        
        float bestScore = float.MinValue;
        Entity bestScoreEntity = Entity.Null;
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
            curScore += distanceScore;
            ///TODO Other score affectors
            
            if (curScore > bestScore)
            {
                bestScore = curScore;
                bestScoreEntity = potentialTarget;
            }
        }
        attack.target = bestScoreEntity;
    }
}
