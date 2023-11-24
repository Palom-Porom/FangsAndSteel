using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct BasicButtonSystem : ISystem
{
    const float STNDRT_SPD = 3;
    const float BSTD_SPD = 6;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<StaticUIData>();
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

            foreach ((SelectTag selectTag, RefRW<MovementComponent> movementComponent) in SystemAPI.Query<SelectTag, RefRW<MovementComponent>>())
            { 
                if (movementComponent.ValueRW.speed == BSTD_SPD) 
                {
                    movementComponent.ValueRW.speed = STNDRT_SPD;
                }
                else { movementComponent.ValueRW.speed = BSTD_SPD; }
            }
        }
    }
}
    

