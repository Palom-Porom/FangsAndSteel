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

    // in - ������ ����������, ref - ������ � ������ (����������� ��������������) ����������
    public DynamicBuffer<VisionMapBuffer>  visionMap;
    public void Execute(in LocalToWorld localtoworld, in TeamComponent team, ref VisibilityComponent visibility)
    {
        float2 position;
        position = localtoworld.Position.xz; //(x,z) -> k, int k = 4 ������� �� ���������� ������� ������� � ���������� ������ �������, � ��� � ������� �������� � �������� � ��� �����
        int k = 0;
        visibility.visibleToTeams = visionMap[k];
    }
}
