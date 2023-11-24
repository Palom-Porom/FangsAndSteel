using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;


[UpdateInGroup(typeof(AfterUnitsSystemGroup))]
public partial class TransformUnitsUISystem : SystemBase
{
    Transform cameraTransform;
    ComponentLookup<LocalTransform> localTransformLookup;
    ComponentLookup<FillFloatOverride> fillBarLookup;

    protected override void OnCreate()
    {
        EntityQuery q = new EntityQueryBuilder(Allocator.TempJob).WithAny<AttackComponent, HpComponent>().Build(this);
        RequireAnyForUpdate(q);
        RequireForUpdate<UnitsIconsComponent>();


        localTransformLookup = GetComponentLookup<LocalTransform>();
        fillBarLookup = GetComponentLookup<FillFloatOverride>();
    }

    protected override void OnStartRunning()
    {
        cameraTransform = Camera.main.transform;
        new InitializeBarsJob { fillBarLookup = GetComponentLookup<FillFloatOverride>() }.Schedule();

    }
    protected override void OnUpdate()
    {
        float3 position = cameraTransform.position;
        float3 camRight = cameraTransform.right;
        localTransformLookup.Update(this);
        fillBarLookup.Update(this);
        TransformUnitsUIJob transformUnitsUIJob = new TransformUnitsUIJob 
        {
            camPosition = position, camRight = camRight,
            localTransformLookup = localTransformLookup
        };
        transformUnitsUIJob.Schedule();

        new UpdateBarsJob { fillBarLookup = GetComponentLookup<FillFloatOverride>() }.Schedule();
    }

}

[BurstCompile]
public partial struct TransformUnitsUIJob : IJobEntity
{
    public float3 camPosition;
    public float3 camRight;

    public ComponentLookup<LocalTransform> localTransformLookup;
    public void Execute(in UnitsIconsComponent unitsIconsComponent, in LocalToWorld unitL2W)
    {
        RefRW<LocalTransform> localTransform = localTransformLookup.GetRefRW(unitsIconsComponent.infoQuadsEntity);

        float3 forward = unitL2W.Position + localTransform.ValueRO.Position - camPosition;
        forward = math.normalize(forward);
        float3 up = math.cross(forward, camRight);

        localTransform.ValueRW.Rotation = unitL2W.Value.InverseTransformRotation(quaternion.LookRotationSafe(forward, up));
    }

}

[BurstCompile]
public partial struct UpdateBarsJob : IJobEntity
{
    public ComponentLookup<FillFloatOverride> fillBarLookup;

    public void Execute(in UnitsIconsComponent unitsIconsComponent, in HpComponent hpComponent, in AttackComponent attackComponent)
    {
        //Update ReloadBar
        fillBarLookup.GetRefRW(unitsIconsComponent.reloadBarEntity).ValueRW.Value = attackComponent.curReload / attackComponent.reloadLen;
    }
}


[BurstCompile]
public partial struct InitializeBarsJob : IJobEntity
{
    public ComponentLookup<FillFloatOverride> fillBarLookup;

    public void Execute(in UnitsIconsComponent unitsIconsComponent, in HpComponent hpComponent, in AttackComponent attackComponent)
    {
        //RefRW<FillFloatOverride> fillComponent = fillBarLookup.GetRefRW(unitsIconsComponent.healthBarEntity);
        //fillComponent.ValueRW.Value = hpComponent.curHp / hpComponent.maxHp;
        //Update HealthBar
        fillBarLookup.GetRefRW(unitsIconsComponent.healthBarEntity).ValueRW.Value = hpComponent.curHp / (float)hpComponent.maxHp;
        //Update ReloadBar
        fillBarLookup.GetRefRW(unitsIconsComponent.reloadBarEntity).ValueRW.Value = attackComponent.curReload / attackComponent.reloadLen;
    }
}