using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;

public partial struct DelayedKillingSystem : ISystem
{
    BufferLookup<Child> childrenLookup;
    EntityCommandBuffer ecb;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<DeadComponent>();

        childrenLookup = state.GetBufferLookup<Child>(true);
    }
    public void OnUpdate(ref SystemState state)
    {
        childrenLookup.Update(ref state);
        ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        //foreach ((RefRW<DeadComponent> dieComponent, Entity entity) in SystemAPI.Query<RefRW<DeadComponent>>().WithEntityAccess())
        //{
        //    if (dieComponent.ValueRO.timeToDie <= 0)
        //    {
        //        UtilityFuncs.DestroyParentAndAllChildren(ecb, state.GetBufferLookup<Child>(true), entity);
        //        ecb.DestroyEntity(entity);
        //    }
        //    else
        //    {
        //        dieComponent.ValueRW.timeToDie -= SystemAPI.Time.DeltaTime;
        //    }
        //}
        new DestroyingDeadJob
        {
            childrenLookup = childrenLookup,
            ecb = ecb.AsParallelWriter(),
            deltaTime = SystemAPI.Time.DeltaTime
        }.Schedule();
    }
}

public partial struct DestroyingDeadJob : IJobEntity
{
    [ReadOnly] public BufferLookup<Child> childrenLookup;
    public EntityCommandBuffer.ParallelWriter ecb;
    public float deltaTime;
    public void Execute(ref DeadComponent dieComponent, Entity entity, [ChunkIndexInQuery] int chunkIndexInQuery)
    {
        if (dieComponent.timeToDie <= 0)
        {
            UtilityFuncs.DestroyParentAndAllChildren(ecb, childrenLookup, entity, chunkIndexInQuery);
        }
        else
        {
            dieComponent.timeToDie -= deltaTime;
        }
    }
}

public struct DeadComponent : IComponentData
{
    public float timeToDie;
}
