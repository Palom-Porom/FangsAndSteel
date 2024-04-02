using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(StaticUISystemGroup), OrderFirst = true)]
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

        
        uiData.ValueRW.shootOnMoveBut = StaticUIRefs.Instance.shootOnMoveBut;
        StaticUIRefs.Instance.shootOnMoveBut = false;

        uiData.ValueRW.shootOffBut = StaticUIRefs.Instance.shootOffBut;
        StaticUIRefs.Instance.shootOffBut = false;

        uiData.ValueRW.autoPursuitBut = StaticUIRefs.Instance.autoPursuitBut;
        StaticUIRefs.Instance.autoPursuitBut = false;
        
        uiData.ValueRW.enemyListBut = StaticUIRefs.Instance.enemyListBut;
        StaticUIRefs.Instance.enemyListBut = false;

        uiData.ValueRW.newPursuiteUnitType = StaticUIRefs.Instance.newPursuiteUnitType;
        StaticUIRefs.Instance.newPursuiteUnitType = 0;


        uiData.ValueRW.newTurnStartBut = StaticUIRefs.Instance.newTurnStartBut;
        StaticUIRefs.Instance.newTurnStartBut = false;

    }
}

public struct StaticUIData : IComponentData
{
    public bool endTurnBut;
    public bool stopMoveBut;


    public bool shootOnMoveBut;
    public bool shootOffBut;
    public bool autoPursuitBut;
    public bool enemyListBut;
    public UnitTypes newPursuiteUnitType;

    public bool newTurnStartBut;
}
