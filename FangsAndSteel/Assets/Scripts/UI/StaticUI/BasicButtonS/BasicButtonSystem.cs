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
    }

    StaticUIData uiData;
    protected override void OnUpdate()
    {
        uiData = SystemAPI.GetSingleton<StaticUIData>();
        if (uiData.stopMoveBut)
        {
            foreach ((RefRW<MovementComponent> movementComponent, DynamicBuffer<MovementCommandsBuffer> moveComBuf, LocalTransform localTransform) 
                in SystemAPI.Query<RefRW<MovementComponent>, DynamicBuffer<MovementCommandsBuffer>, LocalTransform>().WithAll<SelectTag>())
            {
                moveComBuf.Clear();
                movementComponent.ValueRW.target = localTransform.Position;
                movementComponent.ValueRW.hasMoveTarget = false;
            }
        }

        if (SystemAPI.TryGetSingleton<ShootModeButChangeColorRqst>(out shootButColorChangeRqst))
        {
            StaticUIRefs.Instance.ShootModeButton.color = shootButColorChangeRqst.color;
            ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            ecb.DestroyEntity(SystemAPI.GetSingletonEntity<ShootModeButChangeColorRqst>());
        }
        if (uiData.shootModeBut)
        {
            Debug.Log("1");
            new ChangeShootModeJob().Schedule();
            Color c = StaticUIRefs.Instance.ShootModeButton.color;
            if (c.r != c.g)
            {
                Debug.Log("2");
                StaticUIRefs.Instance.ShootModeButton.color = new Color((c.r + 1) % 2, (c.g + 1) % 2, 0);
            }

        }
    }
}

[WithAll(typeof(SelectTag))]
public partial struct ChangeShootModeJob : IJobEntity
{
    public void Execute(ref BattleModeComponent battleModeSettings, ref ReloadComponent reloadComponent, ref MovementComponent movement)
    {
        battleModeSettings.shootingOnMove = !(battleModeSettings.shootingOnMove);
        reloadComponent.curDebaff += battleModeSettings.shootingOnMove ? reloadComponent.reload_SoM_Debaff : -reloadComponent.reload_SoM_Debaff;
        movement.curDebaff += battleModeSettings.shootingOnMove ? movement.movement_SoM_Debaff : -movement.movement_SoM_Debaff;
    }
}

public struct ShootModeButChangeColorRqst : IComponentData
{
    public Color color;
}
    

