using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

public partial class TransformUnitsUISystem : SystemBase
{
    Transform cameraTransform;
    protected override void OnCreate()
    { 
        
    }

    protected override void OnStartRunning()
    {
        cameraTransform = Camera.main.transform;

    }
    protected override void OnUpdate()
    {
        float3 position = cameraTransform.position;
        float3 camRight = cameraTransform.right;
        ComponentLookup<LocalTransform> componentLookup = GetComponentLookup<LocalTransform>();
        TransformUnitsUIJob transformUnitsUIJob = new TransformUnitsUIJob { position = position, camRight = camRight, componentLookUp = componentLookup };
        transformUnitsUIJob.Schedule();
        
    
    }

}

public partial struct TransformUnitsUIJob : IJobEntity
{
    public float3 position;
    public float3 camRight;
    public ComponentLookup<LocalTransform> componentLookUp;
    public void Execute(in UnitsIconsComponent unitsIconsComponent)
    {
        RefRW<LocalTransform> localTransform = componentLookUp.GetRefRW(unitsIconsComponent.infoQuadsEntity);
        float3 forward = localTransform.ValueRW.Position - position;
        forward = math.normalize(forward);
        float3 up = math.cross(forward, camRight);
        localTransform.ValueRW.Rotation = quaternion.LookRotationSafe(forward, up);

    }

}

public partial struct UpdateBarsJob : IJobEntity
{
    public void Execute(in UnitsIconsComponent unitsIconsComponent, in HpComponent hpComponent, in AttackComponent attackComponent)
    {

       
        
    }
}
