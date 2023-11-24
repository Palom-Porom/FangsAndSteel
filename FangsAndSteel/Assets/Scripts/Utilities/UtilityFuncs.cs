using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
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
}
