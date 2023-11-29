using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public static class UtilityFuncs
{
    public static void DestroyParentAndAllChildren(EntityCommandBuffer ecb, BufferLookup<Child> childrenLookup, Entity parent)
    {
        if (!childrenLookup.HasBuffer(parent))
            return;
        foreach (var child in childrenLookup[parent])
        {
            DestroyParentAndAllChildren(ecb, childrenLookup, child.Value);
        }
        ecb.DestroyEntity(parent);
    }

    public static void DestroyParentAndAllChildren(EntityCommandBuffer.ParallelWriter ecb, BufferLookup<Child> childrenLookup, Entity parent, int chunkIndexInQuery)
    {
        if (childrenLookup.HasBuffer(parent))
        {
            foreach (var child in childrenLookup[parent])
            {
                DestroyParentAndAllChildren(ecb, childrenLookup, child.Value, chunkIndexInQuery);
            }
        }
        ecb.DestroyEntity(chunkIndexInQuery, parent);
    }

    public static Entity FindBestTarget (in NativeArray<Entity> potentialTargetsArr,
        in ComponentLookup<TeamComponent> teamLookup, in ComponentLookup<HpComponent> hpLookup, in ComponentLookup<LocalToWorld> localToWorldLookup,
        float3 position, int radiusSq,  int teamInd, float modDist, float modHp)
    {
        float bestScore = float.MinValue;
        Entity bestScoreEntity = Entity.Null;

        foreach (Entity potentialTarget in potentialTargetsArr)
        {
            //Check if they are in different teams
            if (teamLookup[potentialTarget].teamInd - teamInd == 0)
                continue;

            float curScore = 0;

            float distanceScore = radiusSq - math.distancesq(position, localToWorldLookup[potentialTarget].Position)/* * distScoreMultiplier*/;
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
        return bestScoreEntity;
    }
}
