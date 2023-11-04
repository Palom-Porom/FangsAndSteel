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
        //Очистка буфера (Vision map) отвечающего за то что видит та или иная команда. Очистка значений (квадратиков) этого буфера нужна,
        // потому что наши юниты передвигаются, и двигаясь вперед/вправо/влево/назад они видят новые квандратики, которых ранее не видили, 
        //а те квадратики, что ранее они видели - частично перестают видеть
        //(те что больше не входят в радиус относительно новых координат мы прекращаем видеть, а те что еще входят в данный радиус - видим всё ещё)
        //Так вот следующие 3 строчки отвечают за очистку буфера, массива инфы. Де-факто за обновление значений, чтобы старые исчезали, когда появятся новые.
        unsafe
        {
            UnsafeUtility.MemClear(visionMap.GetUnsafePtr(), (long)visionMap.Length * sizeof(int));
        }
        FillVisionMapJob fillVisionMapJob = new FillVisionMapJob{visionMap = visionMap};
        //Планировщик работы Job. Когда будет возможность начинает работу Job
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

