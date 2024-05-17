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


        #region Buy/RemoveUnit
        uiData.ValueRW.removeUnitButton = StaticUIRefs.Instance.removeUnitButton;
        StaticUIRefs.Instance.removeUnitButton = false;
        
        uiData.ValueRW.buyInfantryManButton = StaticUIRefs.Instance.buyInfantryManButton;
        StaticUIRefs.Instance.buyInfantryManButton = false;

        uiData.ValueRW.buyMachineGunnerButton = StaticUIRefs.Instance.buyMachineGunnerButton;
        StaticUIRefs.Instance.buyMachineGunnerButton = false;

        uiData.ValueRW.buyAntyTankButton = StaticUIRefs.Instance.buyAntiTankButton;
        StaticUIRefs.Instance.buyAntiTankButton = false;

        uiData.ValueRW.buyTankButton = StaticUIRefs.Instance.buyTankButton;
        StaticUIRefs.Instance.buyTankButton = false;

        uiData.ValueRW.BuyArtilleryButton = StaticUIRefs.Instance.buyArtilleryButton;
        StaticUIRefs.Instance.buyArtilleryButton = false;
        #endregion
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


    public bool removeUnitButton;
    public bool buyInfantryManButton;
    public bool buyMachineGunnerButton;
    public bool buyAntyTankButton;
    public bool buyTankButton;
    public bool BuyArtilleryButton;
}
