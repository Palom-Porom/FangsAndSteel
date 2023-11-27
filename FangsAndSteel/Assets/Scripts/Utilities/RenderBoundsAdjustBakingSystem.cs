using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Rendering;
using Unity.Mathematics;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
public partial struct RenderBoundsAdjustBakingSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        //EntityCommandBuffer ecb = new EntityCommandBuffer();
        var queryNeedAdjustBounds = SystemAPI.QueryBuilder().WithAll<RenderBounds, RenderBoundsAdjustRqstComponent>().Build();
        foreach ((RefRW<RenderBounds> bounds, RenderBoundsAdjustRqstComponent newBounds, Entity entity) in SystemAPI.Query<RefRW<RenderBounds>, RenderBoundsAdjustRqstComponent>().WithEntityAccess())
        {
            bounds.ValueRW.Value = new AABB { Center = newBounds.center, Extents = newBounds.extents };
            //ecb.RemoveComponent<RenderBoundsAdjustRqstComponent>(entity);
        }
        //ecb.Playback(state.EntityManager);
        state.EntityManager.RemoveComponent<RenderBoundsAdjustRqstComponent>(queryNeedAdjustBounds);
    }

}
