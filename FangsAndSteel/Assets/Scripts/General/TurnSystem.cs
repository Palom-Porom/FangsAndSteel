using AnimCooker;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UI;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[UpdateAfter(typeof(UnitsSystemGroup))]
public partial class TurnSystem : SystemBase
{

    const float TURN_LEN = 10; 
    StaticUIData uiData;
    public static float timeToRun;
    bool orderPhase;

    World replayStartWorld;
    World replayWorld;

    EntityQuery allEntitiesQuery;

    ComponentTypeHandle<Disabled> disabledHandle;
    EntityTypeHandle entityHandle;

    protected override void OnCreate()
    {
        RequireForUpdate<GameTag>();
        RequireForUpdate<StaticUIData>();
        RequireForUpdate(new EntityQueryBuilder().WithNone<IsReplayActive>().Build(EntityManager));
        timeToRun = 0;
        orderPhase = true;

        allEntitiesQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Entity>().Build(EntityManager);

        disabledHandle = GetComponentTypeHandle<Disabled>(true);
        entityHandle = GetEntityTypeHandle();
    }

    protected override void OnStartRunning()
    {
        EnableEngageSystems(!orderPhase);

        disabledHandle.Update(this);
        entityHandle.Update(this);
        Dependency = new EnableAllEntities()
        {
            disabledHandle = disabledHandle,
            entityHandle = entityHandle,
            ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged).AsParallelWriter()
        }.Schedule(allEntitiesQuery, Dependency);

        foreach (var w in World.All)
        {
            if (w.Name == "ReplayStartWorld")
                replayStartWorld = w;
            if (w.Name == "ReplayWorld")
                replayWorld = w;
        }
    }

    public void OnStopRunning(ref SystemState state) 
    {
        orderPhase = !orderPhase;
    }

    
    protected override void OnUpdate()
    {
        //If Order phase
        if (orderPhase)
        {
            uiData = SystemAPI.GetSingleton<StaticUIData>();
            if (uiData.endTurnBut)
            {
                RefRW<CurrentTeamComponent> curTeam = SystemAPI.GetSingletonRW<CurrentTeamComponent>();
                switch (curTeam.ValueRO.value)
                {
                    //1 player finished order phase -> now 2 player
                    case 1:
                        curTeam.ValueRW.value = 2;

                        StaticUIRefs.Instance.NewTurnPanel.SetActive(true);
                        ///TODO: Create some animation to hide the changing players process

                        NativeArray<Entity> entities = replayStartWorld.EntityManager.GetAllEntities(Allocator.Temp);
                        if (entities.Length != 0)
                        {//Prepare to replay via replayWorld
                            //replayWorld.EntityManager.DestroyAndResetAllEntities();
                            replayWorld.EntityManager.CopyEntitiesFrom(replayStartWorld.EntityManager, entities);

                            disabledHandle.Update(this);
                            entityHandle.Update(this);
                            EntityCommandBuffer ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
                            Dependency = new DisableAllEntities()
                            {
                                disabledHandle = disabledHandle,
                                entityHandle = entityHandle,
                                ecb = ecb.AsParallelWriter()
                            }.Schedule(allEntitiesQuery, Dependency);

                            Entity tmpEntity = EntityManager.CreateEntity();
                            EntityManager.AddComponent<IsReplayActive>(tmpEntity);
                        }

                        break;

                    //2 player finished order phase -> now saving the cur gameState and playing results of turn to 1 player
                    case 2:
                        replayStartWorld.EntityManager.CopyAndReplaceEntitiesFrom(World.DefaultGameObjectInjectionWorld.EntityManager);
                        curTeam.ValueRW.value = 1;

                        StaticUIRefs.Instance.NewTurnPanel.SetActive(true);
                        ///TODO: Create some animation to hide the changing players process

                        break;
                }
            }
            else if (uiData.newTurnStartBut && SystemAPI.GetSingleton<CurrentTeamComponent>().value == 1)
            {
                StaticUIRefs.Instance.NewTurnPanel.SetActive(false);
                EnableEngage();
            }
        }
        //If Engage phase
        else
        {
            timeToRun -= SystemAPI.Time.DeltaTime;
            StaticUIRefs.Instance.TurnTimer.text = $"0:{(int)timeToRun:D2}";
            if (timeToRun <= 0)
            {
                StaticUIRefs.Instance.TurnTimer.text = "0:00";
                //Stop all Engage systems
                EnableEngageSystems(false);

                StaticUIRefs.Instance.TurnIndicator.color = Color.red;

                orderPhase = true;
            }
        }
    }

    public void EnableEngage()
    {
        //Start all Engage systems
        EnableEngageSystems(true);
        //Set Timer
        timeToRun = TURN_LEN;

        StaticUIRefs.Instance.TurnIndicator.color = Color.green;

        orderPhase = false;
    }

    private void EnableEngageSystems (bool enable)
    {
        World.Unmanaged.GetExistingSystemState<UnitsSystemGroup>().Enabled = enable;
        //World.Unmanaged.GetExistingSystemState<VisionMapSystem>().Enabled = enable;
        World.Unmanaged.GetExistingSystemState<AnimationSystem>().Enabled = enable;

        World.Unmanaged.GetExistingSystemState<TargetingMoveSystem>().Enabled = !enable;
        World.Unmanaged.GetExistingSystemState<BasicButtonSystem>().Enabled = !enable;
    }

}


public partial struct DisableAllEntities : IJobChunk
{
    public EntityCommandBuffer.ParallelWriter ecb;
    public ComponentTypeHandle<Disabled> disabledHandle;
    public EntityTypeHandle entityHandle;

    public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
    {
        NativeArray<Entity> entities = chunk.GetNativeArray(entityHandle);
        ecb.AddComponent<Disabled>(unfilteredChunkIndex, entities);
        if (chunk.Has(ref disabledHandle))
            ecb.AddComponent<WasDisabledTag>(unfilteredChunkIndex, entities);
    }
}

public partial struct EnableAllEntities : IJobChunk
{
    public EntityCommandBuffer.ParallelWriter ecb;
    public ComponentTypeHandle<Disabled> disabledHandle;
    public EntityTypeHandle entityHandle;

    public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
    {
        NativeArray<Entity> entities = chunk.GetNativeArray(entityHandle);
        if (chunk.Has(ref disabledHandle))
            ecb.AddComponent<WasDisabledTag>(unfilteredChunkIndex, entities);
        else
            ecb.RemoveComponent<Disabled>(unfilteredChunkIndex, entities);
    }
}

public struct WasDisabledTag : IComponentData { }
public struct IsReplayActive : IComponentData { }
