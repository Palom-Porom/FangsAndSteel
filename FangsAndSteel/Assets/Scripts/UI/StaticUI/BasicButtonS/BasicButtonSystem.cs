using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(StaticUISystemGroup))]
public partial class BasicButtonSystem : SystemBase
{

    ShootModeButChangeColorRqst shootButColorChangeRqst;
    EntityCommandBuffer ecb;

    protected override void OnCreate()
    {
        RequireForUpdate<StaticUIData>();
        RequireForUpdate<AttackSettingsComponent>();
    }

    StaticUIData uiData;
    protected override void OnUpdate()
    {
        uiData = SystemAPI.GetSingleton<StaticUIData>();
        if (uiData.stopMoveBut)
        {
            foreach ((SelectTag selectTag, RefRW<MovementComponent> movementComponent, LocalTransform localTransform) in SystemAPI.Query<SelectTag, RefRW<MovementComponent>, LocalTransform>())
            {
                movementComponent.ValueRW.target = localTransform.Position;
            }
        }

        if (SystemAPI.TryGetSingleton<ShootModeButChangeColorRqst>(out shootButColorChangeRqst))
        {
            StaticUIRefs.Instance.ShootModeButton.color = shootButColorChangeRqst.color;
            ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            ecb.DestroyEntity(SystemAPI.GetSingletonEntity<ShootModeButChangeColorRqst>());
        }
        if (uiData.changeSpeedBut)
        {
            new ChangeShootModeJob().Schedule();
            Color c = StaticUIRefs.Instance.ShootModeButton.color;
            StaticUIRefs.Instance.ShootModeButton.color = new Color((c.r + 1) % 2, (c.g + 1) % 2, 0);

        }
    }
}
public partial struct ChangeShootModeJob : IJobEntity
{
    public void Execute(ref AttackSettingsComponent attackSettingsComponent, in SelectTag selectTag)
    {
        attackSettingsComponent.shootingOnMoveMode = !(attackSettingsComponent.shootingOnMoveMode);
    }
}

public struct ShootModeButChangeColorRqst : IComponentData
{
    public Color color;
}
    

