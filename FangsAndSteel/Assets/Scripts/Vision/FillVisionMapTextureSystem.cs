using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;
using Unity.NetCode;

[UpdateAfter(typeof(BeginSimulationEntityCommandBufferSystem))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class FillVisionMapTextureSystem : SystemBase
{
    Material material;
    ComputeBuffer computeBuffer;

    protected override void OnCreate()
    {
        RequireForUpdate<VisionMapBuffer>();
        RequireForUpdate<GameTag>();
    }

    protected override void OnStartRunning()
    {
        material = FogMaterial.material;
        computeBuffer = new ComputeBuffer(250000, sizeof(int), ComputeBufferType.Default, ComputeBufferMode.SubUpdates);
        material.SetBuffer("_VisionMap", computeBuffer);
        int teamInd = -1;
        foreach (GlobalInputData inputData in SystemAPI.Query<GlobalInputData>().WithAll<GhostOwnerIsLocal>())
            teamInd = inputData.teamInd;
        material.SetInteger("_curTeam", teamInd);    
    }

    protected override void OnUpdate()
    {
        if ((UnityEngine.Time.frameCount + 2) % 5 == 0)
        {
            EntityManager.CompleteDependencyBeforeRO<VisionMapBuffer>();
            
            var visionMap = SystemAPI.GetSingletonBuffer<VisionMapBuffer>(true);
            computeBuffer.BeginWrite<int>(0, 250000).CopyFrom(visionMap.Reinterpret<int>().AsNativeArray());
            computeBuffer.EndWrite<int>(250000);
        }
    }

    protected override void OnDestroy()
    {
        computeBuffer?.Release();
    }
}