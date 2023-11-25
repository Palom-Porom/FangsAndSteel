using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(StaticUISystemGroup))]
[BurstCompile]
public partial struct BasicButtonSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<StaticUIData>();
        state.RequireForUpdate<AttackSettingsComponent>();
    }


    StaticUIData uiData;
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        uiData = SystemAPI.GetSingleton<StaticUIData>();
        if (uiData.stopMoveBut)
        {
            foreach ((SelectTag selectTag, RefRW<MovementComponent> movementComponent, LocalTransform localTransform) in SystemAPI.Query<SelectTag, RefRW<MovementComponent>, LocalTransform>())
            {
                movementComponent.ValueRW.target = localTransform.Position;
            }
        }

        if (uiData.changeSpeedBut)
        {
            new ChangeShootModeJob().Schedule();
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
    

