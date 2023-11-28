using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;

public partial class FillVisionMapTextureSystem : SystemBase
{
    Material material;
    ComputeBuffer computeBuffer;
    NativeArray<int> visionMapCopied;

    protected override void OnCreate()
    {
        RequireForUpdate<VisionMapBuffer>();
        RequireForUpdate<GameTag>();
        visionMapCopied = new NativeArray<int>(250000, Allocator.Persistent);
        Debug.Log(0 / 1);
        Debug.Log(1 / 1);
        Debug.Log(2 / 1);
        Debug.Log(3 / 1);
    }

    protected override void OnStartRunning()
    {
        material = FogMaterial.material;
        computeBuffer = new ComputeBuffer(250000, sizeof(int), ComputeBufferType.Default, ComputeBufferMode.SubUpdates);
        computeBuffer.BeginWrite<int>(0, 250000).CopyFrom(visionMapCopied);
        computeBuffer.EndWrite<int>(250000);
        material.SetBuffer("_VisionMap", computeBuffer);
        material.SetInteger("_curTeam", SystemAPI.GetSingleton<CurrentTeamComponent>().currentTeam);    
    }

    protected override void OnUpdate()
    {
        var visionMap = SystemAPI.GetSingletonBuffer<VisionMapBuffer>();
        computeBuffer.BeginWrite<int>(0, 250000).CopyFrom(visionMap.Reinterpret<int>().AsNativeArray());
        computeBuffer.EndWrite<int>(250000);
    }

    protected override void OnDestroy()
    {
        computeBuffer?.Release();
    }
}