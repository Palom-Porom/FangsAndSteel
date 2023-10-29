using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct VisionMapSystem : ISystem
{
   public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<VisionMapBuffer>();
    }
    public void OnUpdate(ref SystemState state)
    {
        DynamicBuffer<VisionMapBuffer> visionMap = SystemAPI.GetSingletonBuffer<VisionMapBuffer>();
        //������� ������ (Vision map) ����������� �� �� ��� ����� �� ��� ���� �������. ������� �������� (�����������) ����� ������ �����,
        // ������ ��� ���� ����� �������������, � �������� ������/������/�����/����� ��� ����� ����� �����������, ������� ����� �� ������, 
        //� �� ����������, ��� ����� ��� ������ - �������� ��������� ������
        //(�� ��� ������ �� ������ � ������ ������������ ����� ��������� �� ���������� ������, � �� ��� ��� ������ � ������ ������ - ����� �� ���)
        //��� ��� ��������� 3 ������� �������� �� ������� ������, ������� ����. ��-����� �� ���������� ��������, ����� ������ ��������, ����� �������� �����.
        unsafe
        {
            UnsafeUtility.MemClear(visionMap.GetUnsafePtr(), (long)visionMap.Length * sizeof(int));
        }
        FillVisionMapJob fillVisionMapJob = new FillVisionMapJob{visionMap = visionMap};
        //����������� ������ Job. ����� ����� ����������� �������� ������ Job
        fillVisionMapJob.Schedule();
    }
}

public partial struct FillVisionMapJob : IJobEntity
{
    [NativeDisableParallelForRestriction] public DynamicBuffer<VisionMapBuffer> visionMap;

    public void Execute(in LocalToWorld localtoworld, in VisionCharsComponent visionChars, in TeamComponent team)
    {
        float2 curpointline;
        float2 point;
        int mapsize = 500;
        int halfMapSize = mapsize/2;
        for (int x = - visionChars.radius; x<visionChars.radius; x++)
        {
            for (int y = - visionChars.radius; y < visionChars.radius; y++)
            {
                curpointline = new float2(x, y);
                if (math.length(curpointline) > visionChars.radius)
                    continue;
                point = math.floor(localtoworld.Position.xz + curpointline + halfMapSize);
                int idx = (int)(point.x + point.y * mapsize);
                if (idx>=0 && idx<visionMap.Length) 
                    visionMap[idx] |= team.teamInd;
            }
        }

    }
    


}

