using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;

[UpdateAfter(typeof(VisionMapSystem))]
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
        material.SetInteger("_curTeam", SystemAPI.GetSingleton<CurrentTeamComponent>().currentTeam);    
    }

    protected override void OnUpdate()
    {
        if (UnityEngine.Time.frameCount % 5 == 0)
        {
            var visionMap = SystemAPI.GetSingletonBuffer<VisionMapBuffer>();
            computeBuffer.BeginWrite<int>(0, 250000).CopyFrom(visionMap.Reinterpret<int>().AsNativeArray());
            computeBuffer.EndWrite<int>(250000);
        }
    }

    protected override void OnDestroy()
    {
        computeBuffer?.Release();
    }
}