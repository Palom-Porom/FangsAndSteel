using AnimCooker;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UI;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[UpdateAfter(typeof(VisionCurrentTeamSystem))]
public partial class TurnSystem : SystemBase
{
    const float TURN_LEN = 10;
    public const float TIME_TO_CLOSE_NEW_TURN_PANEL = 0.4f;
    StaticUIData uiData;
    public static float timeToRun;
    public static float timeToClose_NewTurnPanel;
    bool orderPhase;

    ComponentTypeHandle<Disabled> disabledHandle;
    ComponentTypeHandle<WasDisabledTag> wasDisabledHandle;
    EntityTypeHandle entityHandle;
    BufferTypeHandle<LinkedEntityGroup> linkedEntityGroupHandle;

    ComponentLookup<Disabled> disabledLookup;
    ComponentLookup<WasDisabledTag> wasDisabledLookup;
    ComponentLookup<SelectTag> selectLookup;
    BufferLookup<LinkedEntityGroup> linkedEntityGroupLookup;

    EntityQuery allSelected;
    EntityQuery replayStartCopiesQuery;
    EntityQuery replayCopiesQuery;
    EntityQuery actualEntitiesQuery;

    private EntityQuery flagsQuery;

    protected override void OnCreate()
    {
        RequireForUpdate<GameTag>();
        RequireForUpdate<StaticUIData>();
        RequireForUpdate<BuyStageCompletedTag>();
        timeToRun = 0;
        timeToClose_NewTurnPanel = 0;
        orderPhase = true;

        disabledHandle = GetComponentTypeHandle<Disabled>();
        wasDisabledHandle = GetComponentTypeHandle<WasDisabledTag>();
        entityHandle = GetEntityTypeHandle();
        linkedEntityGroupHandle = GetBufferTypeHandle<LinkedEntityGroup>();

        disabledLookup = GetComponentLookup<Disabled>();
        wasDisabledLookup = GetComponentLookup<WasDisabledTag>();
        selectLookup = GetComponentLookup<SelectTag>();
        linkedEntityGroupLookup = GetBufferLookup<LinkedEntityGroup>();

        allSelected = new EntityQueryBuilder(Allocator.Persistent).WithAll<SelectTag>().Build(this);
        replayStartCopiesQuery = new EntityQueryBuilder(Allocator.Persistent).WithAll<ReplayStartCopyTag, LinkedEntityGroup>().WithOptions(EntityQueryOptions.IncludeDisabledEntities).Build(this);
        replayCopiesQuery = new EntityQueryBuilder(Allocator.Persistent).WithAll<ReplayCopyTag, LinkedEntityGroup>().WithOptions(EntityQueryOptions.IncludeDisabledEntities).Build(this);
        actualEntitiesQuery = new EntityQueryBuilder(Allocator.Persistent).WithAll<ActualEntityTag, LinkedEntityGroup>().WithOptions(EntityQueryOptions.IncludeDisabledEntities).Build(this);

        flagsQuery = new EntityQueryBuilder(Allocator.Persistent).WithAll<FlagTag>().Build(this);
    }

    protected override void OnStartRunning()
    {
        EnableEngageSystems(!orderPhase);
    }

    protected override void OnUpdate()
    {
        disabledHandle.Update(this);
        wasDisabledHandle.Update(this);
        entityHandle.Update(this);
        linkedEntityGroupHandle.Update(this);

        disabledLookup.Update(this);
        wasDisabledLookup.Update(this);
        selectLookup.Update(this);
        linkedEntityGroupLookup.Update(this);

        //  ||---------------||
        //  ||If Order  phase||
        //  ||---------------||
        if (orderPhase)
        {
            uiData = SystemAPI.GetSingleton<StaticUIData>();
            if (uiData.endTurnBut)
            {
                RefRW<CurrentTeamComponent> curTeam = SystemAPI.GetSingletonRW<CurrentTeamComponent>();
                EntityCommandBuffer ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
                //EntityCommandBuffer.ParallelWriter ecb_parallel = ecb.AsParallelWriter();
                //Debug.Log($"curTeam.value = {curTeam.ValueRO.value}");
                //Debug.Log("TurnSystem/OrderMode/Case2 - activated");
                //Dependency = new DeselectAllUnitsJob
                //{
                //    ecb = ecb_parallel,
                //    selectLookup = selectLookup
                //}.Schedule(allSelected, Dependency);

                //Open new turn button
                StaticUIRefs.Instance.NewTurnPanel.SetActive(true);
                ///TODO: Create some animation to hide the changing players process
                
                SelectionSystem.needUpdateUIPanelInfo = true;

                ecb.DestroyEntity(flagsQuery, EntityQueryCaptureMode.AtRecord);

                switch (curTeam.ValueRO.value)
                {
                    //1 player finished order phase -> now 2 player
                    case 1:
                        {
                            //EntityCommandBuffer ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
                            EntityCommandBuffer.ParallelWriter ecb_parallel = ecb.AsParallelWriter();
                            Dependency = new DeselectAllUnitsJob
                            {
                                ecb = ecb_parallel,
                                selectLookup = selectLookup
                            }.Schedule(allSelected, Dependency);

                            //Change cur team
                            curTeam.ValueRW.value = 2;
                            StaticUIRefs.Instance.NewTurnText.text = "Ход игрока 2";

                            //Disable actual units, copy and enable ReplayStartCopies (if no ReplayStartCopies -> no "copy and enable" as it is the first turn)
                            if (!replayStartCopiesQuery.IsEmpty)
                            {


                                Dependency = new DisableActualEntitiesJob
                                {
                                    ecb = ecb_parallel,
                                    //disabledHandle = disabledHandle,
                                    disabledLookup = disabledLookup,
                                    entityHandle = entityHandle,
                                    linkedEntityGroupHandle = linkedEntityGroupHandle
                                }.Schedule(actualEntitiesQuery, Dependency);

                                Dependency = new CopyAndEnableReplayStartEntitiesJob
                                {
                                    ecb = ecb_parallel,
                                    //wasDisabledHandle = wasDisabledHandle,
                                    wasDisabledLookup = wasDisabledLookup,
                                    disabledLookup = disabledLookup,
                                    entityHandle = entityHandle,
                                    linkedEntityGroupLookup = linkedEntityGroupLookup
                                }.Schedule(replayStartCopiesQuery, Dependency);
                            }
                            else
                                Debug.Log("First turn - no Engage phase");
                        }
                        break;

                    //2 player finished order phase -> now saving the cur gameState and playing results of turn to 1 player
                    case 2:
                        {
                            //Destroy old ReplayStartCopies
                            ecb.DestroyEntity(replayStartCopiesQuery, EntityQueryCaptureMode.AtRecord);

                            //EntityCommandBuffer ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
                            EntityCommandBuffer.ParallelWriter ecb_parallel = ecb.AsParallelWriter();
                            Dependency = new DeselectAllUnitsJob
                            {
                                ecb = ecb_parallel,
                                selectLookup = selectLookup
                            }.Schedule(allSelected, Dependency);

                            //Change cur team
                            curTeam.ValueRW.value = 1;
                            StaticUIRefs.Instance.NewTurnText.text = "Ход игрока 1";

                            //Create new ReplayStartCopies
                            Dependency = new CreateNewReplayStartEntitiesJob
                            {
                                ecb = ecb_parallel,
                                entityHandle = entityHandle,
                                //disabledHandle = disabledHandle,
                                disabledLookup = disabledLookup,
                                wasDisabledLookup = wasDisabledLookup,
                                linkedEntityGroupLookup = linkedEntityGroupLookup
                            }.Schedule(actualEntitiesQuery, Dependency);
                        }
                        break;
                }
            }
            else if (uiData.newTurnStartBut)
            {
                //Set time for closing newTurnPanel
                timeToClose_NewTurnPanel = TIME_TO_CLOSE_NEW_TURN_PANEL;
                
                //Close the newTurnPanel
                StaticUIRefs.Instance.NewTurnPanel.SetActive(false);
            }
            else if (timeToClose_NewTurnPanel > 0)
            {
                timeToClose_NewTurnPanel -= SystemAPI.Time.DeltaTime;
                if (timeToClose_NewTurnPanel <= 0)
                {
                    //Enable EngageMode (= Start Replay if the 2 player's turn) (if no ReplayStartCopies -> no engageMode as it is the first turn)
                    if (!replayStartCopiesQuery.IsEmpty)
                        EnableEngage();
                    //else
                        //Debug.Log("First turn - no Engage phase");
                }
            }
        }
        //  ||---------------||
        //  ||If Engage phase||
        //  ||---------------||
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

                //If turn of 2nd player -> remove copies and enable actual entities
                if (SystemAPI.GetSingleton<CurrentTeamComponent>().value == 2)
                {
                    EntityCommandBuffer ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
                    ecb.DestroyEntity(replayCopiesQuery, EntityQueryCaptureMode.AtRecord);
                    Dependency = new EnableActualEntitiesJob
                    {
                        ecb = ecb.AsParallelWriter(),
                        entityHandle = entityHandle,
                        //wasDisabledHandle = wasDisabledHandle,
                        wasDisabledLookup = wasDisabledLookup,
                        linkedEntityGroupHandle = linkedEntityGroupHandle
                    }.Schedule(actualEntitiesQuery, Dependency);
                }

                orderPhase = true;
            }
        }
    }


    public void EnableEngage()
    {
        var a = replayStartCopiesQuery.ToEntityArray(Allocator.Temp);
        Debug.Log($"num of replayStartCopies = {a.Length}");

        //Start all Engage systems
        EnableEngageSystems(true);

        //Set Timer
        timeToRun = TURN_LEN;

        StaticUIRefs.Instance.TurnIndicator.color = Color.green;

        orderPhase = false;
    }

    private void EnableEngageSystems(bool enable)
    {
        World.Unmanaged.GetExistingSystemState<UnitsSystemGroup>().Enabled = enable;
        //World.Unmanaged.GetExistingSystemState<VisionMapSystem>().Enabled = enable;
        World.Unmanaged.GetExistingSystemState<AnimationSystem>().Enabled = enable;

        World.Unmanaged.GetExistingSystemState<TargetingMoveSystem>().Enabled = !enable;
        World.Unmanaged.GetExistingSystemState<BasicButtonSystem>().Enabled = !enable;
    }
}



public partial struct DisableActualEntitiesJob : IJobChunk
{
    public EntityCommandBuffer.ParallelWriter ecb;
    //public ComponentTypeHandle<Disabled> disabledHandle;
    public ComponentLookup<Disabled> disabledLookup;
    public EntityTypeHandle entityHandle;
    public BufferTypeHandle<LinkedEntityGroup> linkedEntityGroupHandle;

    public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
    {
        NativeArray<Entity> entities = chunk.GetNativeArray(entityHandle);
        BufferAccessor<LinkedEntityGroup> linkedEntityBuffs = chunk.GetBufferAccessor(ref linkedEntityGroupHandle);
        //bool isDisabled = chunk.Has(ref disabledHandle);
        for (int i = 0; i < chunk.Count; i++)
        {
            if (disabledLookup.HasComponent(entities[i]))
                ecb.AddComponent<WasDisabledTag>(unfilteredChunkIndex, entities[i]);
            else
                ecb.AddComponent<Disabled>(unfilteredChunkIndex, entities[i]);

            foreach (var child in linkedEntityBuffs[i])
            {
                if (disabledLookup.HasComponent(child.Value))
                    ecb.AddComponent<WasDisabledTag>(unfilteredChunkIndex, child.Value);
                else
                    ecb.AddComponent<Disabled>(unfilteredChunkIndex, child.Value);
            }
        }
    }
}

public partial struct CopyAndEnableReplayStartEntitiesJob : IJobChunk
{
    public EntityCommandBuffer.ParallelWriter ecb;
    //public ComponentTypeHandle<WasDisabledTag> wasDisabledHandle;
    public ComponentLookup<WasDisabledTag> wasDisabledLookup;
    public ComponentLookup<Disabled> disabledLookup;
    public EntityTypeHandle entityHandle;
    public BufferLookup<LinkedEntityGroup> linkedEntityGroupLookup;

    public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
    {
        NativeArray<Entity> entities = chunk.GetNativeArray(entityHandle);
        //BufferAccessor<LinkedEntityGroup> linkedEntityBuffs = chunk.GetBufferAccessor(ref linkedEntityGroupLookup);
        for (int i = 0; i < chunk.Count; i++)
        {
            DynamicBuffer<LinkedEntityGroup> entityGroup = linkedEntityGroupLookup[entities[i]];
            NativeArray<bool> hadWasDisabled = new NativeArray<bool>(entityGroup.Length, Allocator.Temp);
            //foreach (var child in linkedEntityGroupLookup[entities[i]])
            for (int j = 0; j < entityGroup.Length; j++)
            {
                hadWasDisabled[j] = wasDisabledLookup.HasComponent(entityGroup[j].Value);
                if (hadWasDisabled[j])
                    ecb.RemoveComponent<WasDisabledTag>(unfilteredChunkIndex, entityGroup[j].Value);
                else
                    ecb.RemoveComponent<Disabled>(unfilteredChunkIndex, entityGroup[j].Value);
            }

            Entity tmp = ecb.Instantiate(unfilteredChunkIndex, entities[i]);
            ecb.RemoveComponent<ReplayStartCopyTag>(unfilteredChunkIndex, tmp);
            ecb.AddComponent<ReplayCopyTag>(unfilteredChunkIndex, tmp);
            //if (wasDisabledLookup.HasComponent(entities[i]))
            //    ecb.RemoveComponent<WasDisabledTag>(unfilteredChunkIndex, tmp);
            //else
            //    ecb.RemoveComponent<Disabled>(unfilteredChunkIndex, tmp);

            //foreach (var child in linkedEntityGroupLookup[entities[i]])
            for (int j = 0; j < entityGroup.Length; j++)
            {
                if (hadWasDisabled[j])
                    ecb.AddComponent<WasDisabledTag>(unfilteredChunkIndex, entityGroup[j].Value);
                else
                    ecb.AddComponent<Disabled>(unfilteredChunkIndex, entityGroup[j].Value);
            }

            //foreach (var child in linkedEntityGroupLookup[tmp])
            //{
            //    if (wasDisabledLookup.HasComponent(child.Value))
            //        ecb.RemoveComponent<WasDisabledTag>(unfilteredChunkIndex, child.Value);
            //    else
            //        ecb.RemoveComponent<Disabled>(unfilteredChunkIndex, child.Value);
            //}
        }
    }
}

public partial struct CreateNewReplayStartEntitiesJob : IJobChunk
{
    public EntityCommandBuffer.ParallelWriter ecb;
    //public ComponentTypeHandle<Disabled> disabledHandle;
    public ComponentLookup<Disabled> disabledLookup;
    public ComponentLookup<WasDisabledTag> wasDisabledLookup;
    public EntityTypeHandle entityHandle;
    public BufferLookup<LinkedEntityGroup> linkedEntityGroupLookup;

    public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
    {
        NativeArray<Entity> entities = chunk.GetNativeArray(entityHandle);
        //BufferAccessor<LinkedEntityGroup> linkedEntityBuffs = chunk.GetBufferAccessor(ref linkedEntityGroupLookup);
        //bool isDisabled = chunk.Has(ref disabledHandle);
        for (int i = 0; i < chunk.Count; i++)
        {
            DynamicBuffer<LinkedEntityGroup> entityGroup = linkedEntityGroupLookup[entities[i]];
            NativeArray<bool> hadDisabled = new NativeArray<bool>(entityGroup.Length, Allocator.Temp);
            for (int j = 0; j < entityGroup.Length; j++)
            {
                hadDisabled[j] = disabledLookup.HasComponent(entityGroup[j].Value);
                if (hadDisabled[j])
                    ecb.AddComponent<WasDisabledTag>(unfilteredChunkIndex, entityGroup[j].Value);
                else
                    ecb.AddComponent<Disabled>(unfilteredChunkIndex, entityGroup[j].Value);
            }

            Entity tmp = ecb.Instantiate(unfilteredChunkIndex, entities[i]);
            ecb.RemoveComponent<ActualEntityTag>(unfilteredChunkIndex, tmp);
            ecb.AddComponent<ReplayStartCopyTag>(unfilteredChunkIndex, tmp);
            //if (disabledLookup.HasComponent(entities[i]))
            //    ecb.AddComponent<WasDisabledTag>(unfilteredChunkIndex, tmp);
            //else
            //    ecb.AddComponent<Disabled>(unfilteredChunkIndex, tmp);

            for (int j = 0; j < entityGroup.Length; j++)
            {
                if (hadDisabled[j])
                    ecb.RemoveComponent<WasDisabledTag>(unfilteredChunkIndex, entityGroup[j].Value);
                else
                    ecb.RemoveComponent<Disabled>(unfilteredChunkIndex, entityGroup[j].Value);
            }

            //foreach (var child in linkedEntityGroupLookup[entities[i]])
            //{
            //    if (wasDisabledLookup.HasComponent(child.Value))
            //        ecb.RemoveComponent<WasDisabledTag>(unfilteredChunkIndex, child.Value);
            //    else
            //        ecb.RemoveComponent<Disabled>(unfilteredChunkIndex, child.Value);
            //}


            //foreach (var child in linkedEntityGroupLookup[tmp])
            //{
            //    if (disabledLookup.HasComponent(child.Value))
            //        ecb.AddComponent<WasDisabledTag>(unfilteredChunkIndex, child.Value);
            //    else
            //        ecb.AddComponent<Disabled>(unfilteredChunkIndex, child.Value);
            //}
            Debug.Log($"New ReplayStartCopy entity was created - Index = {tmp.Index}; Version = {tmp.Version}");
        }
    }
}


public partial struct EnableActualEntitiesJob : IJobChunk
{
    public EntityCommandBuffer.ParallelWriter ecb;
    //public ComponentTypeHandle<WasDisabledTag> wasDisabledHandle;
    public ComponentLookup<WasDisabledTag> wasDisabledLookup;
    public EntityTypeHandle entityHandle;
    public BufferTypeHandle<LinkedEntityGroup> linkedEntityGroupHandle;

    public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
    {
        NativeArray<Entity> entities = chunk.GetNativeArray(entityHandle);
        BufferAccessor<LinkedEntityGroup> linkedEntityBuffs = chunk.GetBufferAccessor(ref linkedEntityGroupHandle);
        //bool wasDisabled = chunk.Has(ref wasDisabledHandle);
        for (int i = 0; i < chunk.Count; i++)
        {
            if (wasDisabledLookup.HasComponent(entities[i]))
                ecb.RemoveComponent<WasDisabledTag>(unfilteredChunkIndex, entities[i]);
            else
                ecb.RemoveComponent<Disabled>(unfilteredChunkIndex, entities[i]);

            foreach (var child in linkedEntityBuffs[i])
            {
                if (wasDisabledLookup.HasComponent(child.Value))
                    ecb.RemoveComponent<WasDisabledTag>(unfilteredChunkIndex, child.Value);
                else
                    ecb.RemoveComponent<Disabled>(unfilteredChunkIndex, child.Value);
            }
        }

        //NativeArray<Entity> entities = chunk.GetNativeArray(entityHandle);
        //if(chunk.Has(ref wasDisabledHandle))
        //    ecb.RemoveComponent<WasDisabledTag>(unfilteredChunkIndex, entities);
        //else
        //    ecb.RemoveComponent<Disabled>(unfilteredChunkIndex, entities);
    }
}

[UpdateAfter(typeof(VisionCurrentTeamSystem))]
public partial class TurnSystem_SinglePlayer : SystemBase
{
    const float TURN_LEN = 10;
    //const float TIME_TO_CLOSE_NEW_TURN_PANEL = 1;
    StaticUIData uiData;
    public static float timeToRun;
    //public static float timeToClose_NewTurnPanel;
    bool orderPhase;

    //ComponentTypeHandle<Disabled> disabledHandle;
    //ComponentTypeHandle<WasDisabledTag> wasDisabledHandle;
    //EntityTypeHandle entityHandle;
    //BufferTypeHandle<LinkedEntityGroup> linkedEntityGroupHandle;

    //ComponentLookup<Disabled> disabledLookup;
    //ComponentLookup<WasDisabledTag> wasDisabledLookup;
    //ComponentLookup<SelectTag> selectLookup;
    //BufferLookup<LinkedEntityGroup> linkedEntityGroupLookup;

    //EntityQuery allSelected;
    //EntityQuery replayStartCopiesQuery;
    //EntityQuery replayCopiesQuery;
    //EntityQuery actualEntitiesQuery;

    //private EntityQuery flagsQuery;

    protected override void OnCreate()
    {
        RequireForUpdate<TutorialTag>();
        RequireForUpdate<StaticUIData>();
        timeToRun = 0;
        //timeToClose_NewTurnPanel = 0;
        orderPhase = true;

        //disabledHandle = GetComponentTypeHandle<Disabled>();
        //wasDisabledHandle = GetComponentTypeHandle<WasDisabledTag>();
        //entityHandle = GetEntityTypeHandle();
        //linkedEntityGroupHandle = GetBufferTypeHandle<LinkedEntityGroup>();

        //disabledLookup = GetComponentLookup<Disabled>();
        //wasDisabledLookup = GetComponentLookup<WasDisabledTag>();
        //selectLookup = GetComponentLookup<SelectTag>();
        //linkedEntityGroupLookup = GetBufferLookup<LinkedEntityGroup>();

        //allSelected = new EntityQueryBuilder(Allocator.Persistent).WithAll<SelectTag>().Build(this);
        //replayStartCopiesQuery = new EntityQueryBuilder(Allocator.Persistent).WithAll<ReplayStartCopyTag, LinkedEntityGroup>().WithOptions(EntityQueryOptions.IncludeDisabledEntities).Build(this);
        //replayCopiesQuery = new EntityQueryBuilder(Allocator.Persistent).WithAll<ReplayCopyTag, LinkedEntityGroup>().WithOptions(EntityQueryOptions.IncludeDisabledEntities).Build(this);
        //actualEntitiesQuery = new EntityQueryBuilder(Allocator.Persistent).WithAll<ActualEntityTag, LinkedEntityGroup>().WithOptions(EntityQueryOptions.IncludeDisabledEntities).Build(this);

        //flagsQuery = new EntityQueryBuilder(Allocator.Persistent).WithAll<FlagTag>().Build(this);
    }

    protected override void OnStartRunning()
    {
        EnableEngageSystems(!orderPhase);
    }

    protected override void OnUpdate()
    {
        //disabledHandle.Update(this);
        //wasDisabledHandle.Update(this);
        //entityHandle.Update(this);
        //linkedEntityGroupHandle.Update(this);

        //disabledLookup.Update(this);
        //wasDisabledLookup.Update(this);
        //selectLookup.Update(this);
        //linkedEntityGroupLookup.Update(this);

        //  ||---------------||
        //  ||If Order  phase||
        //  ||---------------||
        if (orderPhase)
        {
            uiData = SystemAPI.GetSingleton<StaticUIData>();
            if (uiData.endTurnBut)
            {
                //Start all Engage systems
                EnableEngageSystems(true);
                //Set Timer
                timeToRun = TURN_LEN;

                orderPhase = false;

                StaticUIRefs.Instance.TurnIndicator.color = Color.green;


                //RefRW<CurrentTeamComponent> curTeam = SystemAPI.GetSingletonRW<CurrentTeamComponent>();
                //EntityCommandBuffer ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
                ////EntityCommandBuffer.ParallelWriter ecb_parallel = ecb.AsParallelWriter();
                ////Debug.Log($"curTeam.value = {curTeam.ValueRO.value}");
                ////Debug.Log("TurnSystem/OrderMode/Case2 - activated");
                ////Dependency = new DeselectAllUnitsJob
                ////{
                ////    ecb = ecb_parallel,
                ////    selectLookup = selectLookup
                ////}.Schedule(allSelected, Dependency);

                ////Open new turn button
                //StaticUIRefs.Instance.NewTurnPanel.SetActive(true);
                /////TODO: Create some animation to hide the changing players process

                //SelectionSystem.needUpdateUIPanelInfo = true;

                //ecb.DestroyEntity(flagsQuery, EntityQueryCaptureMode.AtRecord);

                //switch (curTeam.ValueRO.value)
                //{
                //    //1 player finished order phase -> now 2 player
                //    case 1:
                //        {
                //            //EntityCommandBuffer ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
                //            EntityCommandBuffer.ParallelWriter ecb_parallel = ecb.AsParallelWriter();
                //            Dependency = new DeselectAllUnitsJob
                //            {
                //                ecb = ecb_parallel,
                //                selectLookup = selectLookup
                //            }.Schedule(allSelected, Dependency);

                //            //Change cur team
                //            curTeam.ValueRW.value = 2;

                //            //Disable actual units, copy and enable ReplayStartCopies (if no ReplayStartCopies -> no "copy and enable" as it is the first turn)
                //            if (!replayStartCopiesQuery.IsEmpty)
                //            {


                //                Dependency = new DisableActualEntitiesJob
                //                {
                //                    ecb = ecb_parallel,
                //                    //disabledHandle = disabledHandle,
                //                    disabledLookup = disabledLookup,
                //                    entityHandle = entityHandle,
                //                    linkedEntityGroupHandle = linkedEntityGroupHandle
                //                }.Schedule(actualEntitiesQuery, Dependency);

                //                Dependency = new CopyAndEnableReplayStartEntitiesJob
                //                {
                //                    ecb = ecb_parallel,
                //                    //wasDisabledHandle = wasDisabledHandle,
                //                    wasDisabledLookup = wasDisabledLookup,
                //                    disabledLookup = disabledLookup,
                //                    entityHandle = entityHandle,
                //                    linkedEntityGroupLookup = linkedEntityGroupLookup
                //                }.Schedule(replayStartCopiesQuery, Dependency);
                //            }
                //            else
                //                Debug.Log("First turn - no Engage phase");
                //        }
                //        break;

                //    //2 player finished order phase -> now saving the cur gameState and playing results of turn to 1 player
                //    case 2:
                //        {
                //            //Destroy old ReplayStartCopies
                //            ecb.DestroyEntity(replayStartCopiesQuery, EntityQueryCaptureMode.AtRecord);

                //            //EntityCommandBuffer ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
                //            EntityCommandBuffer.ParallelWriter ecb_parallel = ecb.AsParallelWriter();
                //            Dependency = new DeselectAllUnitsJob
                //            {
                //                ecb = ecb_parallel,
                //                selectLookup = selectLookup
                //            }.Schedule(allSelected, Dependency);

                //            //Change cur team
                //            curTeam.ValueRW.value = 1;

                //            //Create new ReplayStartCopies
                //            Dependency = new CreateNewReplayStartEntitiesJob
                //            {
                //                ecb = ecb_parallel,
                //                entityHandle = entityHandle,
                //                //disabledHandle = disabledHandle,
                //                disabledLookup = disabledLookup,
                //                wasDisabledLookup = wasDisabledLookup,
                //                linkedEntityGroupLookup = linkedEntityGroupLookup
                //            }.Schedule(actualEntitiesQuery, Dependency);
                //        }
                //        break;
                //}
            }
            //else if (uiData.newTurnStartBut)
            //{
            //    //Set time for closing newTurnPanel
            //    timeToClose_NewTurnPanel = TIME_TO_CLOSE_NEW_TURN_PANEL;

            //    //Close the newTurnPanel
            //    StaticUIRefs.Instance.NewTurnPanel.SetActive(false);
            //}
            //else if (timeToClose_NewTurnPanel > 0)
            //{
            //    timeToClose_NewTurnPanel -= SystemAPI.Time.DeltaTime;
            //    if (timeToClose_NewTurnPanel <= 0)
            //    {
            //        //Enable EngageMode (= Start Replay if the 2 player's turn) (if no ReplayStartCopies -> no engageMode as it is the first turn)
            //        if (!replayStartCopiesQuery.IsEmpty)
            //            EnableEngage();
            //        else
            //            Debug.Log("First turn - no Engage phase");
            //    }
            //}
        }
        //  ||---------------||
        //  ||If Engage phase||
        //  ||---------------||
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





                //timeToRun -= SystemAPI.Time.DeltaTime;
                //StaticUIRefs.Instance.TurnTimer.text = $"0:{(int)timeToRun:D2}";
                //if (timeToRun <= 0)
                //{
                //    StaticUIRefs.Instance.TurnTimer.text = "0:00";
                //    //Stop all Engage systems
                //    EnableEngageSystems(false);

                //    StaticUIRefs.Instance.TurnIndicator.color = Color.red;

                //    //If turn of 2nd player -> remove copies and enable actual entities
                //    if (SystemAPI.GetSingleton<CurrentTeamComponent>().value == 2)
                //    {
                //        EntityCommandBuffer ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
                //        ecb.DestroyEntity(replayCopiesQuery, EntityQueryCaptureMode.AtRecord);
                //        Dependency = new EnableActualEntitiesJob
                //        {
                //            ecb = ecb.AsParallelWriter(),
                //            entityHandle = entityHandle,
                //            //wasDisabledHandle = wasDisabledHandle,
                //            wasDisabledLookup = wasDisabledLookup,
                //            linkedEntityGroupHandle = linkedEntityGroupHandle
                //        }.Schedule(actualEntitiesQuery, Dependency);
                //    }

                //    orderPhase = true;
                //}
            }
        }
    }

    //private void EnableEngage()
    //{
    //    var a = replayStartCopiesQuery.ToEntityArray(Allocator.Temp);
    //    Debug.Log($"num of replayStartCopies = {a.Length}");

    //    //Start all Engage systems
    //    EnableEngageSystems(true);

    //    //Set Timer
    //    timeToRun = TURN_LEN;

    //    StaticUIRefs.Instance.TurnIndicator.color = Color.green;

    //    orderPhase = false;
    //}

    private void EnableEngageSystems(bool enable)
    {
        World.Unmanaged.GetExistingSystemState<UnitsSystemGroup>().Enabled = enable;
        //World.Unmanaged.GetExistingSystemState<VisionMapSystem>().Enabled = enable;
        World.Unmanaged.GetExistingSystemState<AnimationSystem>().Enabled = enable;

        World.Unmanaged.GetExistingSystemState<TargetingMoveSystem>().Enabled = !enable;
        World.Unmanaged.GetExistingSystemState<BasicButtonSystem>().Enabled = !enable;
    }
}