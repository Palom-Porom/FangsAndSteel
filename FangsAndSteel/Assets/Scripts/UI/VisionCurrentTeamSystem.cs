using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


public partial struct VisionCurrentTeamSystem : ISystem 
{
    public void OnCreate(ref SystemState state)
    { 

    }
}

public partial struct VisionCurrentTeamJob : IJobEntity
{

    // in - чтение компонента, ref - чтение и запись (¬озможность редактировани€) компонента
    public DynamicBuffer<VisionMapBuffer>  visionMap;
    public void Execute(in LocalToWorld localtoworld, in TeamComponent team, ref VisibilityComponent visibility)
    {
        float2 position;
        position = localtoworld.Position.xz; //(x,z) -> k, int k = 4 перевод из двумерного массива позиций в одномерный массив позиций, в тот с помощью которого и работает у нас карта
        int k = 0;
        visibility.visibleToTeams = visionMap[k];
    }
}
