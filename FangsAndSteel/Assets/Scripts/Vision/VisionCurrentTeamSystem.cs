using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;


[UpdateInGroup(typeof(UnitsSystemGroup))]
[BurstCompile]
public partial struct VisionCurrentTeamSystem : ISystem 
{
    BufferLookup<Child> children;
    ComponentLookup<DisableRendering> disableRendLookup;

    EntityCommandBuffer ecb;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<VisionMapBuffer>();
        state.RequireForUpdate<TeamComponent>();
        state.RequireForUpdate<VisibilityComponent>();

        children = SystemAPI.GetBufferLookup<Child>();
        disableRendLookup = SystemAPI.GetComponentLookup<DisableRendering>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        children.Update(ref state);
        disableRendLookup.Update(ref state);
        ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        DynamicBuffer<VisionMapBuffer> visionMap = SystemAPI.GetSingletonBuffer<VisionMapBuffer>();

        int teamInd = -1;
        foreach (var inputData in SystemAPI.Query<GlobalInputData>().WithAll<GhostOwnerIsLocal>())
            teamInd = inputData.teamInd;

        VisionCurrentTeamJob visionCurrentTeamJob = new VisionCurrentTeamJob
        {
            visionMap = visionMap,
            //currentTeamComponent = SystemAPI.GetSingleton<CurrentTeamComponent>(),
            teamInd = teamInd,
            childrenLookup = children,
            disableRendLookup = disableRendLookup,
            ecb = ecb.AsParallelWriter()
        };
        state.Dependency = visionCurrentTeamJob.Schedule(state.Dependency);
    }
}


[BurstCompile]
public partial struct VisionCurrentTeamJob : IJobEntity
{
    // in - чтение компонента, ref - чтение и запись (¬озможность редактировани€) компонента
    public DynamicBuffer<VisionMapBuffer>  visionMap;
    //public CurrentTeamComponent currentTeamComponent;
    public int teamInd;
    public BufferLookup<Child> childrenLookup;
    public ComponentLookup<DisableRendering> disableRendLookup;

    public EntityCommandBuffer.ParallelWriter ecb;

    const int ORIG_MAP_SIZE = 1000;
    const int VIS_MAP_SIZE = 500;
    const int ORIG_TO_VIS_MAP_RATIO = 2;

    public void Execute(in LocalToWorld localtoworld, in TeamComponent team, ref VisibilityComponent visibility, Entity entity, [ChunkIndexInQuery] int chunkIndexInQuery, in UnitIconsComponent unitsIconsComponent)
    {
        float2 position;
        position = localtoworld.Position.xz + VIS_MAP_SIZE; 
        //(x,z) -> k перевод из двумерного массива позиций в одномерный массив позиций, в тот с помощью которого и работает у нас карта
        int k = (int)(position.x / ORIG_TO_VIS_MAP_RATIO + math.floor(position.y / ORIG_TO_VIS_MAP_RATIO) * VIS_MAP_SIZE);
        visibility.visibleToTeams = visionMap[k];

        // if not visible to current team
        if ((visibility.visibleToTeams & teamInd) == 0)
        {
            if (!disableRendLookup.HasComponent(unitsIconsComponent.VisualizationEntity))
                DisableParentAndAllChildrenRender(unitsIconsComponent.VisualizationEntity, chunkIndexInQuery);
        }
        // if visible to current team
        else
            if (disableRendLookup.HasComponent(unitsIconsComponent.VisualizationEntity))
                EnableParentAndAllChildrenRender(unitsIconsComponent.VisualizationEntity, chunkIndexInQuery);
    }


    private void DisableParentAndAllChildrenRender(Entity parent, int chunkIndexInQuery)
    {
        ecb.AddComponent<DisableRendering>(chunkIndexInQuery, parent);
        if (!childrenLookup.HasBuffer(parent))
            return;
        foreach(var child in childrenLookup[parent])
        {
            DisableParentAndAllChildrenRender(child.Value, chunkIndexInQuery);
        }
    }

    private void EnableParentAndAllChildrenRender(Entity parent, int chunkIndexInQuery)
    {
        ecb.RemoveComponent<DisableRendering>(chunkIndexInQuery, parent);
        if (!childrenLookup.HasBuffer(parent))
        {
            return;
        }
        foreach (var child in childrenLookup[parent])
        {
            EnableParentAndAllChildrenRender(child.Value, chunkIndexInQuery);
        }
    }
}
