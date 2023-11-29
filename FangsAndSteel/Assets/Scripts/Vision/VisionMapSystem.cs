using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


[UpdateAfter(typeof(FillVisionMapTextureSystem))]
[BurstCompile]
public partial struct VisionMapSystem : ISystem, ISystemStartStop
{
    #region Debug Vars
    Entity noTeamPrefub;
    Entity firstTeamPrefub;
    Entity secondTeamPrefub;
    Entity bothTeamsPrefub;

    bool needToDebug;
    #endregion

    EntityCommandBuffer.ParallelWriter ecb;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<VisionMapBuffer>();
        state.RequireForUpdate<TeamComponent>();
        state.RequireForUpdate<VisionCharsComponent>();

        //Set true if need to visualize the views in the first frame
        needToDebug = false;
    }

    public void OnStartRunning(ref SystemState state)
    {
        DynamicBuffer<VisionMapBuffer> visionMap = SystemAPI.GetSingletonBuffer<VisionMapBuffer>();

        unsafe
        {
            UnsafeUtility.MemClear(visionMap.GetUnsafePtr(), (long)visionMap.Length * sizeof(int));
        }
        FillVisionMapJob fillVisionMapJob = new FillVisionMapJob
        {
            visionMap = visionMap,
        };

        fillVisionMapJob.Schedule();
    }

    public void OnStopRunning(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (Time.frameCount % 5 == 0)
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
            FillVisionMapJob fillVisionMapJob = new FillVisionMapJob
            {
                visionMap = visionMap,
            };

            //Планировщик работы Job. Когда будет возможность начинает работу Job
            state.Dependency = fillVisionMapJob.Schedule(state.Dependency);

            #region Debug
            if (needToDebug)
            {
                noTeamPrefub = SystemAPI.GetSingleton<DebugCube>().noTeamPrefub;
                firstTeamPrefub = SystemAPI.GetSingleton<DebugCube>().firstTeamPrefub;
                secondTeamPrefub = SystemAPI.GetSingleton<DebugCube>().secondTeamPrefub;
                bothTeamsPrefub = SystemAPI.GetSingleton<DebugCube>().bothTeamsPrefub;

                ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

                state.Dependency = new DebugVissionMapJob
                {
                    noTeamPrefub = noTeamPrefub,
                    firstTeamPrefub = firstTeamPrefub,
                    secondTeamPrefub = secondTeamPrefub,
                    bothTeamsPrefub = bothTeamsPrefub,
                    visionMap = visionMap,
                    ecb = ecb
                }.Schedule(250000, 100, state.Dependency);

                needToDebug = false;
            }
            #endregion
        }
    }
}

/// <summary>
/// Fills the VisionMap with actual vision data
/// </summary>
[BurstCompile]
public partial struct FillVisionMapJob : IJobEntity
{
    //  [NativeDisableParallelForRestriction] отвечает за разрешение записи в одну и ту же ячейку доступа
    [NativeDisableParallelForRestriction] public DynamicBuffer<VisionMapBuffer> visionMap;

    const int ORIG_MAP_SIZE = 1000;
    const int VIS_MAP_SIZE = 500;
    const int ORIG_TO_VIS_MAP_RATIO = 2;

    bool isOnLeftSideOfField;
    bool isOnRightSideOfField;

    // Execute - обязательный метод IJobEntity, являющийся аналогом foreach
    public void Execute(in LocalToWorld localtoworld, in VisionCharsComponent visionChars, in TeamComponent team)
    {
        float2 curpointline;
        float2 point;
        //for (var x := ..... to visionChars.radius step ORIG_TO_....)
        //for (int i = 0; i < size; i++) - for (int x...) но более понятным языком
        int idx = (int)((localtoworld.Position.x + VIS_MAP_SIZE) / ORIG_TO_VIS_MAP_RATIO + math.floor((localtoworld.Position.y + VIS_MAP_SIZE) / ORIG_TO_VIS_MAP_RATIO) * VIS_MAP_SIZE);
        if (idx % 500 < visionChars.radius / 2 || idx % 500 > 500 - visionChars.radius / 2)
        {
            isOnLeftSideOfField = idx % 500 < 250;
            isOnRightSideOfField = !isOnLeftSideOfField;
        }
        else
        {
            isOnLeftSideOfField = false;
            isOnRightSideOfField = false;
        }
        for (int x = (-visionChars.radius) - visionChars.radius % ORIG_TO_VIS_MAP_RATIO; x <= visionChars.radius; x += ORIG_TO_VIS_MAP_RATIO)
        {
            for (int y = (-visionChars.radius) - visionChars.radius % ORIG_TO_VIS_MAP_RATIO; y <= visionChars.radius; y += ORIG_TO_VIS_MAP_RATIO)
            {
                curpointline = new float2(x, y);
                if (math.length(curpointline) > visionChars.radius)
                    continue;
                point = math.floor(localtoworld.Position.xz + curpointline + VIS_MAP_SIZE);
                idx = (int)(point.x / ORIG_TO_VIS_MAP_RATIO + math.floor(point.y / ORIG_TO_VIS_MAP_RATIO) * VIS_MAP_SIZE);
                if ((isOnLeftSideOfField && idx % 500 >= 250) || (isOnRightSideOfField && idx % 500 < 250))
                    continue;
                if (idx >= 0 && idx < visionMap.Length)
                    //    |  -  побитовое "или"
                    visionMap[idx] |= team.teamInd;
            }
        }

    }

}

#region DebugJob
/// <summary>
/// Visualize the current visionMap state (don't destroy the debugCubes)
/// </summary>
//[BurstCompile]
public partial struct DebugVissionMapJob : IJobParallelFor
{
    [ReadOnly] public DynamicBuffer<VisionMapBuffer> visionMap;

    public Entity noTeamPrefub;
    public Entity firstTeamPrefub;
    public Entity secondTeamPrefub;
    public Entity bothTeamsPrefub;

    public EntityCommandBuffer.ParallelWriter ecb;

    const int ORIG_MAP_SIZE = 1000;
    const int VIS_MAP_SIZE = 500;
    const int ORIG_TO_VIS_MAP_RATIO = 2;
    public void Execute(int index)
    {
        int x = (index * ORIG_TO_VIS_MAP_RATIO) % ORIG_MAP_SIZE;
        int y = (index * ORIG_TO_VIS_MAP_RATIO - x) / VIS_MAP_SIZE;
        float2 plainPoint = new float2(x, y) - VIS_MAP_SIZE;
        float3 pos = new float3(plainPoint.x, 10f, plainPoint.y);

        //Instantiate Debug Cubes
        Entity entity = Entity.Null;
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
        }
    }
}
#endregion
