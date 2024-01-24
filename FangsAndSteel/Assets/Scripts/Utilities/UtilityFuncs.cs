using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

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


    const float LOW_FLOAT = 0.005f;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Nearly_Equals_quaternion(quaternion q1, quaternion q2)
    {
        return math.abs(q1.value.x - q2.value.x) < LOW_FLOAT 
           && math.abs(q1.value.y - q2.value.y) < LOW_FLOAT
           && math.abs(q1.value.z - q2.value.z) < LOW_FLOAT 
           && math.abs(q1.value.w - q2.value.w) < LOW_FLOAT;
    }
}
