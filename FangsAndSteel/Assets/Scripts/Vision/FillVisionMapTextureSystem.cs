using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;

[UpdateAfter(typeof(BeginSimulationEntityCommandBufferSystem))]
public partial class FillVisionMapTextureSystem : SystemBase
{
    Material material;
    ComputeBuffer computeBuffer;

    protected override void OnCreate()
    {
        RequireForUpdate<VisionMapBuffer>();
        RequireForUpdate(new EntityQueryBuilder(Allocator.Temp).WithAny<GameTag, TutorialTag>().Build(this));
        //RequireForUpdate<GameTag>();
    }

    protected override void OnStartRunning()
    {
        material = FogMaterial.material;
        computeBuffer = new ComputeBuffer(250000, sizeof(int), ComputeBufferType.Default, ComputeBufferMode.SubUpdates);
        material.SetBuffer("_VisionMap", computeBuffer);
        //material.SetInteger("_curTeam", SystemAPI.GetSingleton<CurrentTeamComponent>().value);    
    }

    protected override void OnUpdate()
    {
        if ((UnityEngine.Time.frameCount + 2) % 5 == 0)
        {
            material.SetInteger("_curTeam", SystemAPI.GetSingleton<CurrentTeamComponent>().value);
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