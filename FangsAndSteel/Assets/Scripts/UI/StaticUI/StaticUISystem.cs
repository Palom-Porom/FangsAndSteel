using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(StaticUISystemGroup), OrderFirst = true)]
public partial class StaticUISystem : SystemBase
{
    RefRW<StaticUIData> uiData;

    protected override void OnCreate()
    {
        RequireForUpdate(new EntityQueryBuilder(Allocator.Temp).WithAny<GameTag, TutorialTag>().Build(this));
    }

    protected override void OnStartRunning()
    {
        if (!EntityManager.HasComponent<StaticUIData>(SystemHandle))
            EntityManager.AddComponent<StaticUIData>(SystemHandle);
    }
    protected override void OnStopRunning()
    {
        EntityManager.RemoveComponent<StaticUIData>(SystemHandle);
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


        #region Sliders
        uiData.ValueRW.newPursuitStartRadius = StaticUIRefs.Instance.newPursuitStartRadius;
        StaticUIRefs.Instance.newPursuitStartRadius = -1;

        uiData.ValueRW.newPursuitmaxHp = StaticUIRefs.Instance.newPursuitmaxHp;
        StaticUIRefs.Instance.newPursuitmaxHp = -1;

        uiData.ValueRW.newPursuitEndRadius = StaticUIRefs.Instance.newPursuitEndRadius;
        StaticUIRefs.Instance.newPursuitEndRadius = -1;

        uiData.ValueRW.newPursuitMinAttackRadius = StaticUIRefs.Instance.newPursuitMinAttackRadius;
        StaticUIRefs.Instance.newPursuitMinAttackRadius = -1;
        #endregion

        uiData.ValueRW.newPursuitTimeForEnd = StaticUIRefs.Instance.newPursuitTimeForEnd;
        //if (StaticUIRefs.Instance.newPursuitTimeForEnd != -1)
        //    Debug.Log($"{StaticUIRefs.Instance.newPursuitTimeForEnd} ===> {uiData.ValueRO.newPursuitTimeForEnd}");
        StaticUIRefs.Instance.newPursuitTimeForEnd = -1;


        uiData.ValueRW.isNeededPrioritiesUpdate = StaticUIRefs.Instance.isNeededPrioritiesUpdate;
        StaticUIRefs.Instance.isNeededPrioritiesUpdate = false;


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
    public float newPursuitStartRadius;
    public float newPursuitmaxHp;
    public float newPursuitEndRadius;
    public float newPursuitMinAttackRadius;
    public float newPursuitTimeForEnd;

    public bool isNeededPrioritiesUpdate;

    public bool newTurnStartBut;
}
