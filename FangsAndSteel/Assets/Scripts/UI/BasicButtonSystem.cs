using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public partial struct BasicButtonSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<StaticUIData>();
    }


    StaticUIData uiData;
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
    }

    
}
