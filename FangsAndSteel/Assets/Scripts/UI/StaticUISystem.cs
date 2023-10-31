using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[UpdateAfter(typeof(ControlSystem))]
public partial class StaticUISystem : SystemBase
{
    RefRW<StaticUIData> uiData;

    protected override void OnCreate()
    {
        EntityManager.AddComponent<StaticUIData>(SystemHandle);
    }

    protected override void OnUpdate()
    {
        uiData = SystemAPI.GetComponentRW<StaticUIData>(SystemHandle);

        //if (StaticUIRefs.Instance == null) return;
        uiData.ValueRW.endTurnBut = StaticUIRefs.Instance.endTurnBut;
        StaticUIRefs.Instance.endTurnBut = false;


        uiData.ValueRW.stopMoveBut = StaticUIRefs.Instance.stopMoveBut;
        StaticUIRefs.Instance.stopMoveBut = false;
    }
}

public struct StaticUIData : IComponentData
{
    public bool endTurnBut;
    public bool stopMoveBut;
}
