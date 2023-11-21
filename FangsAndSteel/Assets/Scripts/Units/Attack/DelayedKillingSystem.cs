using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public partial struct DelayedKillingSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<DieComponent>();
    }
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        foreach ((RefRW<DieComponent> dieComponent, Entity entity) in SystemAPI.Query<RefRW<DieComponent>>().WithEntityAccess())
        {
            if (dieComponent.ValueRO.timeToDie <= 0)
            {
                ecb.DestroyEntity(entity);
            }
            else
            {
                dieComponent.ValueRW.timeToDie -= SystemAPI.Time.DeltaTime;
            }
        }
    }
}

public struct DieComponent : IComponentData
{
    public float timeToDie;
}
