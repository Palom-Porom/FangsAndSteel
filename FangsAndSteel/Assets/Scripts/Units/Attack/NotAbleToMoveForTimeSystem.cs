using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public partial struct NotAbleToMoveForTimeSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NotAbleToMoveForTimeRqstComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        state.Dependency = new NotAbleToMoveForTimeJob 
        { 
            deltaTime = SystemAPI.Time.DeltaTime, 
            ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter() 
        }.Schedule(state.Dependency);
    }
}


public partial struct NotAbleToMoveForTimeJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ecb;
    public float deltaTime;

    public void Execute(ref NotAbleToMoveForTimeRqstComponent rqst, ref AttackSettingsComponent attackSettingsComponent, Entity entity, [ChunkIndexInQuery] int chunkIndexInQuery)
    {
        rqst.passedTime += deltaTime;
        if (rqst.passedTime >= rqst.targetTime)
        {
            attackSettingsComponent.isAbleToMove = true;
            ecb.RemoveComponent<NotAbleToMoveForTimeRqstComponent>(chunkIndexInQuery, entity);
        }
    }

}