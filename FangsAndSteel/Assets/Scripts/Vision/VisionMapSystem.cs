using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static FillVisionMapJob;
using static UnityEditor.PlayerSettings;

public partial struct VisionMapSystem : ISystem
{
    Entity noTeamPrefub;
    Entity firstTeamPrefub;
    Entity secondTeamPrefub;
    Entity bothTeamsPrefub;

    EntityCommandBuffer.ParallelWriter ecb;

    bool debugWasDone;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<VisionMapBuffer>();
        state.RequireForUpdate<DebugCube>();
        state.RequireForUpdate<TeamComponent>();
        state.RequireForUpdate<VisionCharsComponent>();

        debugWasDone = false;
    }
    public void OnUpdate(ref SystemState state)
    {
        //Debug.Log("started");

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
        FillVisionMapJob fillVisionMapJob = new FillVisionMapJob
        {
            visionMap = visionMap,
        };
        //Планировщик работы Job. Когда будет возможность начинает работу Job
        fillVisionMapJob.Schedule();

        if (!debugWasDone)
        {
            Debug.Log("Entered If");
            noTeamPrefub = SystemAPI.GetSingleton<DebugCube>().noTeamPrefub;
            firstTeamPrefub = SystemAPI.GetSingleton<DebugCube>().firstTeamPrefub;
            secondTeamPrefub = SystemAPI.GetSingleton<DebugCube>().secondTeamPrefub;
            bothTeamsPrefub = SystemAPI.GetSingleton<DebugCube>().bothTeamsPrefub;

            ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            state.Dependency = new DebugVissionMapJob
            {
                noTeamPrefub = noTeamPrefub,
                firstTeamPrefub = firstTeamPrefub,
                secondTeamPrefub = secondTeamPrefub,
                bothTeamsPrefub = bothTeamsPrefub,
                visionMap = visionMap,
                ecb = ecb
            }.Schedule(250000, 100, state.Dependency);

            debugWasDone = true;
        }
    }
}

public partial struct FillVisionMapJob : IJobEntity
{
    [NativeDisableParallelForRestriction] public DynamicBuffer<VisionMapBuffer> visionMap;

    public void Execute(in LocalToWorld localtoworld, in VisionCharsComponent visionChars, in TeamComponent team)
    {
        float2 curpointline;
        float2 point;
        int mapsize = 1000;
        int halfMapSize = mapsize/2;
        for (int x = (-visionChars.radius) - visionChars.radius % 2; x <= visionChars.radius; x +=2 )
        {
            for (int y = (-visionChars.radius) - visionChars.radius % 2 ; y <= visionChars.radius; y += 2)
            {
                curpointline = new float2(x, y);
                if (math.length(curpointline) > visionChars.radius)
                    continue;
                point = math.floor(localtoworld.Position.xz + curpointline + 500);
                int idx = (int)(point.x / 2 + math.floor(point.y / 2) * 500);
                //Debug.Log($"{point} = {idx}");
                //if (idx > 124500 && idx < 125500)
                //    Debug.Log($"{idx} = {point} = {point.x / 2} + {point.y / 2} * 500 (={point.y / 2 * 500})");
                if (idx >= 0 && idx < visionMap.Length)
                    visionMap[idx] |= team.teamInd;
            }
        }

    }

}

public partial struct DebugVissionMapJob : IJobParallelFor
{
    [ReadOnly] public DynamicBuffer<VisionMapBuffer> visionMap;

    public Entity noTeamPrefub;
    public Entity firstTeamPrefub;
    public Entity secondTeamPrefub;
    public Entity bothTeamsPrefub;

    public EntityCommandBuffer.ParallelWriter ecb;

    const int mapsize = 1000;
    const int halfMapsize = 500;
    public void Execute(int index)
    {
        int x = (index * 2) % 1000;
        int y = (index * 2 - x) / 500;
        float2 plainPoint = new float2(x, y) - 500;
        //float2 plainPoint = new float2((index % mapsize) * 2, (index / mapsize) * 2) - mapsize;
        float3 pos = new float3(plainPoint.x, 10f, plainPoint.y);
        if (index > 124500 && index < 125500)
            Debug.Log($"{index} = {pos}");
        Entity entity = Entity.Null;
        //Instantiate Debug Cubes
        switch (visionMap[index])
        {
            case 0:
                entity = ecb.Instantiate(index, noTeamPrefub);
                break;
            case 1:
                entity = ecb.Instantiate(index, firstTeamPrefub);
                break;
            case 2:
                entity = ecb.Instantiate(index, secondTeamPrefub);
                break;
            case 3:
                entity = ecb.Instantiate(index, bothTeamsPrefub);
                break;
            default:
                Debug.Log("Incorrect TeamTag");
                break;
        }
        if (entity != Entity.Null)
        {
            ecb.SetComponent(index, entity, new LocalTransform { Position = pos, Rotation = quaternion.identity, Scale = 1 });
            //Debug.Log($"{index} was done at the point {pos}");
        }
    }
}
