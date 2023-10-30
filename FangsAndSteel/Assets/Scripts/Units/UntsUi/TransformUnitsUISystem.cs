using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;


public partial class TransformUnitsUISystem : SystemBase
{
    Transform cameraTransform;
    ComponentLookup<LocalTransform> localTransformLookup;

    protected override void OnCreate()
    {
        localTransformLookup = GetComponentLookup<LocalTransform>();
    }

    protected override void OnStartRunning()
    {
        cameraTransform = Camera.main.transform;

    }
    protected override void OnUpdate()
    {
        float3 position = cameraTransform.position;
        float3 camRight = cameraTransform.right;
        localTransformLookup.Update(this);
        TransformUnitsUIJob transformUnitsUIJob = new TransformUnitsUIJob 
        {
            camPosition = position, camRight = camRight,
            localTransformLookup = localTransformLookup
        };
        transformUnitsUIJob.Schedule();
        
    
    }

}

public partial struct TransformUnitsUIJob : IJobEntity
{
    public float3 camPosition;
    public float3 camRight;

    public ComponentLookup<LocalTransform> localTransformLookup;
    public void Execute(in UnitsIconsComponent unitsIconsComponent, in LocalToWorld unitL2W)
    {
        RefRW<LocalTransform> localTransform = localTransformLookup.GetRefRW(unitsIconsComponent.infoQuadsEntity);

        float3 forward = localTransform.ValueRO.Position - camPosition;
        forward = math.normalize(forward);
        float3 up = math.cross(forward, camRight);

        localTransform.ValueRW.Rotation = unitL2W.Value.InverseTransformRotation(quaternion.LookRotationSafe(forward, up));
    }

}

public partial struct UpdateBarsJob : IJobEntity
{
    public void Execute(in UnitsIconsComponent unitsIconsComponent, in HpComponent hpComponent, in AttackComponent attackComponent)
    {

       
        
    }
}
