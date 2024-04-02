using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;

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
            StaticUIRefs.Instance.ShootOnMoveButton.color = shootButColorChangeRqst.color;
            ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            ecb.DestroyEntity(SystemAPI.GetSingletonEntity<ShootModeButChangeColorRqst>());
        }
        if (uiData.shootOnMoveBut)
        {
            new ChangeShootModeJob().Schedule();
            UpdateButColor(NewUnitUIManager.Instance.ShootOnMoveButton);
        }

        if (uiData.shootOffBut)
        {
            new ChangeShootOffModeJob().Schedule();
            UpdateButColor(NewUnitUIManager.Instance.ShootOffButton);
        }

        if (uiData.autoPursuitBut)
        {
            new ChangeAutoTriggerModeJob().Schedule();
            Image but = NewUnitUIManager.Instance.AutoPursuitButton;
            if (but.color == Color.green)
            { but.color = Color.red; }
            else if (but.color == Color.red)
            { but.color = Color.green; }

            AutoPursuitButtonPush autoPursuitButtonPushScript = but.GetComponent<AutoPursuitButtonPush>();
            autoPursuitButtonPushScript.HidePursuitPanelPanel.SetActive(!autoPursuitButtonPushScript.HidePursuitPanelPanel.activeSelf);
            if (autoPursuitButtonPushScript.EnemyListPanel.activeSelf)
            {
                autoPursuitButtonPushScript.EnemyListPanel.SetActive(false);
                but.color = Color.red;
            }
        }

        if ((uint)uiData.newPursuiteUnitType != 0)
        {
            new ChangeAutoTriggerUnitsJob { inputUnits = (uint)uiData.newPursuiteUnitType }.Schedule();
        }

        #region Sliders
        if (uiData.newPursuitStartRadius != -1)
        {
            new ChangeAutoTriggerStartRadiusJob { newValue = uiData.newPursuitStartRadius }.Schedule();
        }
        if (uiData.newPursuitmaxHp != -1)
        {
            new ChangeAutoTriggerMaxHpJob { newValue = uiData.newPursuitmaxHp }.Schedule();
        }
        if (uiData.newPursuitEndRadius != -1)
        {
            new ChangeAutoTriggerEndRadiusJob { newValue = uiData.newPursuitEndRadius }.Schedule();
        }
        if (uiData.newPursuitMinAttackRadius != -1)
        {
            new ChangeAutoTriggerMinAttackDistJob { newValue = uiData.newPursuitMinAttackRadius }.Schedule();
        }
        #endregion
        if (uiData.newPursuitTimeForEnd != -1)
        {
            //Debug.Log(uiData.newPursuitTimeForEnd);
            new ChangeAutoTriggerTimeForEndJob { newValue = uiData.newPursuitTimeForEnd }.Schedule();
        }


        if (uiData.isNeededPrioritiesUpdate)
        {
            var newPriorities = PriorityDropdownScript.ReturnDropdownPriorityInfo();
            new UpdatePrioritiesJob { newPriorities = newPriorities }.Schedule();
        }
    }

    private void UpdateButColor(Image but)
    {
        if (but.color == Color.green)
            but.color = Color.red;
        else if (but.color == Color.red)
            but.color = Color.green;
    }
}

//[WithAll(typeof(SelectTag))]
[WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
public partial struct ChangeShootModeJob : IJobEntity
{
    public void Execute(ref BattleModeComponent battleModeSettings, ref ReloadComponent reloadComponent, ref MovementComponent movement, EnabledRefRO<SelectTag> selectTag)
    {
        if (!selectTag.ValueRO) return;
        battleModeSettings.shootingOnMove = !(battleModeSettings.shootingOnMove);
        reloadComponent.curDebaff += battleModeSettings.shootingOnMove ? reloadComponent.reload_SoM_Debaff : -reloadComponent.reload_SoM_Debaff;
        movement.curDebaff += battleModeSettings.shootingOnMove ? movement.movement_SoM_Debaff : -movement.movement_SoM_Debaff;
    }
}

//[WithAll(typeof(SelectTag))]
[WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
public partial struct ChangeShootOffModeJob : IJobEntity
{
    public void Execute(ref BattleModeComponent battleModeSettings, EnabledRefRO<SelectTag> selectTag)
    {
        if (!selectTag.ValueRO) return;
        battleModeSettings.shootingDisabled = !battleModeSettings.shootingDisabled;
    }
}

[WithAll(typeof(SelectTag))]
[WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
public partial struct ChangeAutoTriggerModeJob : IJobEntity
{
    public void Execute(ref BattleModeComponent battleModeSettings, EnabledRefRO<SelectTag> selectTag)
    {
        if (!selectTag.ValueRO) return;
        battleModeSettings.isAutoTrigger = !battleModeSettings.isAutoTrigger;
    }
}

//[WithAll(typeof(SelectTag))]
[WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
public partial struct ChangeAutoTriggerUnitsJob : IJobEntity
{
    public uint inputUnits;

    public void Execute(ref BattleModeComponent battleModeSettings, EnabledRefRO<SelectTag> selectTag)
    {
        if (!selectTag.ValueRO) return;

        if ((inputUnits & (uint)UnitTypes.BaseInfantry) != 0)
            if ((battleModeSettings.autoTriggerUnitTypes & (uint)UnitTypes.BaseInfantry) != 0)
                battleModeSettings.autoTriggerUnitTypes -= (uint)UnitTypes.BaseInfantry;
            else
                battleModeSettings.autoTriggerUnitTypes |= (uint)UnitTypes.BaseInfantry;

        if ((inputUnits & (uint)UnitTypes.MachineGunner) != 0)
            if ((battleModeSettings.autoTriggerUnitTypes & (uint)UnitTypes.MachineGunner) != 0)
                battleModeSettings.autoTriggerUnitTypes -= (uint)UnitTypes.MachineGunner;
            else
                battleModeSettings.autoTriggerUnitTypes |= (uint)UnitTypes.MachineGunner;

        if ((inputUnits & (uint)UnitTypes.AntyTank) != 0)
            if ((battleModeSettings.autoTriggerUnitTypes & (uint)UnitTypes.AntyTank) != 0)
                battleModeSettings.autoTriggerUnitTypes -= (uint)UnitTypes.AntyTank;
            else
                battleModeSettings.autoTriggerUnitTypes |= (uint)UnitTypes.AntyTank;

        //Debug.Log((inputUnits & (uint)UnitTypes.Tank) != 0);
        //Debug.Log((battleModeSettings.autoTriggerUnitTypes & (uint)UnitTypes.Tank) != 0);
        if ((inputUnits & (uint)UnitTypes.Tank) != 0)
            if ((battleModeSettings.autoTriggerUnitTypes & (uint)UnitTypes.Tank) != 0)
                battleModeSettings.autoTriggerUnitTypes -= (uint)UnitTypes.Tank;
            else
                battleModeSettings.autoTriggerUnitTypes |= (uint)UnitTypes.Tank;

        if ((inputUnits & (uint)UnitTypes.Artillery) != 0)
            if ((battleModeSettings.autoTriggerUnitTypes & (uint)UnitTypes.Artillery) != 0)
                battleModeSettings.autoTriggerUnitTypes -= (uint)UnitTypes.Artillery;
            else
                battleModeSettings.autoTriggerUnitTypes |= (uint)UnitTypes.Artillery;
    }
}

#region Sliders

[WithAll(typeof(SelectTag))]
[WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
public partial struct ChangeAutoTriggerStartRadiusJob : IJobEntity
{
    public float newValue;

    public void Execute(ref BattleModeComponent battleModeSettings, EnabledRefRO<SelectTag> selectTag)
    {
        if (!selectTag.ValueRO) return;

        battleModeSettings.autoTriggerRadiusSq = newValue * newValue;
    }
}

[WithAll(typeof(SelectTag))]
[WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
public partial struct ChangeAutoTriggerMaxHpJob : IJobEntity
{
    public float newValue;

    public void Execute(ref BattleModeComponent battleModeSettings, EnabledRefRO<SelectTag> selectTag)
    {
        if (!selectTag.ValueRO) return;

        battleModeSettings.autoTriggerMaxHpPercent = (int)newValue;
    }
}

[WithAll(typeof(SelectTag))]
[WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
public partial struct ChangeAutoTriggerEndRadiusJob : IJobEntity
{
    public float newValue;

    public void Execute(ref PursuingModeComponent pursueModeSettings, EnabledRefRO<SelectTag> selectTag)
    {
        if (!selectTag.ValueRO) return;

        pursueModeSettings.dropDistanceSq = newValue * newValue;
    }
}

[WithAll(typeof(SelectTag))]
[WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
public partial struct ChangeAutoTriggerMinAttackDistJob : IJobEntity
{
    public float newValue;

    public void Execute(ref PursuingModeComponent pursueModeSettings, EnabledRefRO<SelectTag> selectTag)
    {
        if (!selectTag.ValueRO) return;

        pursueModeSettings.maxShootDistanceSq = newValue * newValue;
    }
}

#endregion

[WithAll(typeof(SelectTag))]
[WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
public partial struct ChangeAutoTriggerTimeForEndJob : IJobEntity
{
    public float newValue;

    public void Execute(ref PursuingModeComponent pursuingModeSettings, EnabledRefRO<SelectTag> selectTag)
    {
        //Debug.Log(selectTag.ValueRO);
        if (!selectTag.ValueRO) return;

        pursuingModeSettings.dropTime = newValue;
    }
}



[WithAll(typeof(SelectTag))]
[WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
public partial struct UpdatePrioritiesJob : IJobEntity
{
    public PriorityInfo newPriorities;

    public void Execute(AttackPrioritiesAspect attackPriorities, EnabledRefRO<SelectTag> selectTag)
    {
        if (!selectTag.ValueRO) return;

        attackPriorities.DistanceModifier = (newPriorities.distancePriority == -1) ? 0 : 7 - newPriorities.distancePriority;
        attackPriorities.MinHpModifier = (newPriorities.minHpPriority == -1) ? 0 : 7 - newPriorities.minHpPriority;

        attackPriorities.UnitsPriority_Clear();
        foreach(var prior in newPriorities.unitsPriorities)
        {
            attackPriorities.UnitsPriority_Add((uint)prior.Item1, (7 - prior.Item2));
        }
    }
}


public struct ShootModeButChangeColorRqst : IComponentData
{
    public Color color;
}
    

