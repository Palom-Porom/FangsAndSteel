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
    //NativeReference<NativeArray<int>> arrCopiedRef;

    protected override void OnCreate()
    {
        RequireForUpdate<VisionMapBuffer>();
        RequireForUpdate<GameTag>();
        visionMapCopied = new NativeArray<int>(250000, Allocator.Persistent);
        //arrCopiedRef = new NativeReference<NativeArray<int>>(Allocator.Persistent);
        //arrCopiedRef.Value = visionMapCopied;
        Debug.Log(0 / 1);
        Debug.Log(1 / 1);
        Debug.Log(2 / 1);
        Debug.Log(3 / 1);
    }

    protected override void OnStartRunning()
    {
        material = FogMaterial.material;
        computeBuffer = new ComputeBuffer(250000, sizeof(int), ComputeBufferType.Default, ComputeBufferMode.SubUpdates);
        //unsafe
        //{
        //    void* visionMap = SystemAPI.GetSingletonBuffer<VisionMapBuffer>().GetUnsafePtr();
        //    computeBuffer.BeginWrite<int>(0, 250000).CopyFrom((int*)visionMap);
        //    computeBuffer.EndWrite<int>(249999);
        //}
        //material.SetBuffer("_VisionMap", computeBuffer);
        computeBuffer.BeginWrite<int>(0, 250000).CopyFrom(visionMapCopied);
        computeBuffer.EndWrite<int>(250000);
        material.SetBuffer("_VisionMap", computeBuffer);
    }

    protected override void OnUpdate()
    {
        var visionMap = SystemAPI.GetSingletonBuffer<VisionMapBuffer>();
        computeBuffer.BeginWrite<int>(0, 250000).CopyFrom(visionMap.Reinterpret<int>().AsNativeArray());
        computeBuffer.EndWrite<int>(250000);
        //computeBuffer.SetData(visionMap);
        //int limit = 0;
        //int limitMax = 500;
        //for (int i = 0; i < 250000; i++)
        //{
        //    visionMapCopied[i] = visionMap[i];
            //if (visionMapCopied[i] != 0 && limit < limitMax)
            //{
            //    Debug.Log(visionMapCopied[i]);
            //    limit++;
            //}
        //}
    }

    protected override void OnDestroy()
    {
        computeBuffer?.Release();
    }
}

[BurstCompile]
public partial struct FillFogOfWarTextureJob : IJob
{
    [ReadOnly] public DynamicBuffer<VisionMapBuffer> visionMap;
    public NativeReference<NativeArray<int>> visionMapCopied;
    public void Execute()
    {
        NativeArray<int> temp = new NativeArray<int>(250000, Allocator.TempJob);
        for (int i = 0; i < 250000; i++)
            temp[i] = visionMap[i];
        visionMapCopied.Value = temp;
    }
}