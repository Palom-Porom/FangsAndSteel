using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[UpdateInGroup(typeof(StaticUISystemGroup), OrderFirst = true)]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class StaticUISystem : SystemBase
{
    //RefRW<StaticUIData> uiData;

    protected override void OnCreate()
    {
        //EntityManager.AddComponent<StaticUIData>(SystemHandle);
    }

    protected override void OnUpdate()
    {
        //uiData = SystemAPI.GetComponentRW<StaticUIData>(SystemHandle);

        foreach (RefRW<StaticUIData> uiData in SystemAPI.Query<RefRW<StaticUIData>>().WithAll<GhostOwnerIsLocal>())
        {

            //if (StaticUIRefs.Instance == null) return;
            if (StaticUIRefs.Instance.endTurnBut) uiData.ValueRW.endTurnBut.Set();
            //uiData.ValueRW.endTurnBut = StaticUIRefs.Instance.endTurnBut;
            StaticUIRefs.Instance.endTurnBut = false;

            if (StaticUIRefs.Instance.stopMoveBut) uiData.ValueRW.stopMoveBut.Set();
            //uiData.ValueRW.stopMoveBut = StaticUIRefs.Instance.stopMoveBut;
            StaticUIRefs.Instance.stopMoveBut = false;

            if (StaticUIRefs.Instance.shootModeBut) uiData.ValueRW.shootModeBut.Set();
            //uiData.ValueRW.shootModeBut = StaticUIRefs.Instance.shootModeBut;
            StaticUIRefs.Instance.shootModeBut = false;

        }
        

    }
}

public struct StaticUIData : IInputComponentData
{
    public int teamInd;

    public InputEvent endTurnBut;
    public InputEvent stopMoveBut;
    public InputEvent shootModeBut;
}


