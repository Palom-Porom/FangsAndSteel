using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


public partial struct VisionCurrentTeamSystem : ISystem 
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<VisionMapBuffer>();
        state.RequireForUpdate<TeamComponent>();
        state.RequireForUpdate<VisibilityComponent>();
    }
    public void OnUpdate(ref SystemState state)
    {
        DynamicBuffer<VisionMapBuffer> visionMap = SystemAPI.GetSingletonBuffer<VisionMapBuffer>();
        VisionCurrentTeamJob visionCurrentTeamJob = new VisionCurrentTeamJob { visionMap = visionMap };
        visionCurrentTeamJob.Schedule();
    }
}

public partial struct VisionCurrentTeamJob : IJobEntity
{
    // in - чтение компонента, ref - чтение и запись (¬озможность редактировани€) компонента
    public DynamicBuffer<VisionMapBuffer>  visionMap;

    const int ORIG_MAP_SIZE = 1000;
    const int VIS_MAP_SIZE = 500;
    const int ORIG_TO_VIS_MAP_RATIO = 2;

    public void Execute(in LocalToWorld localtoworld, in TeamComponent team, ref VisibilityComponent visibility)
    {
        float2 position;
        position = localtoworld.Position.xz + VIS_MAP_SIZE; 
        //(x,z) -> k перевод из двумерного массива позиций в одномерный массив позиций, в тот с помощью которого и работает у нас карта
        int k = (int)(position.x / ORIG_TO_VIS_MAP_RATIO + math.floor(position.y / ORIG_TO_VIS_MAP_RATIO) * VIS_MAP_SIZE);
        visibility.visibleToTeams = visionMap[k];
    }
}
