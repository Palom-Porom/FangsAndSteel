using System;
using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
public partial class UnitStatsUiSystem : SystemBase
{
    ComponentLookup<HpComponent> hpStats;
    ComponentLookup<AttackComponent> attackStats;
    ComponentLookup<MovementComponent> movementStats;
    protected override void OnCreate()
    {
        RequireForUpdate<UnitStatsRequestComponent>();
        //RequireForUpdate<HpComponent>();
        //RequireForUpdate<AttackComponent>();
        //RequireForUpdate<MovementComponent>();
        hpStats = SystemAPI.GetComponentLookup<HpComponent>();
        attackStats = SystemAPI.GetComponentLookup<AttackComponent>();
        movementStats = SystemAPI.GetComponentLookup<MovementComponent>();
    }

    protected override void OnStartRunning()
    {
        StaticUIRefs.Instance.HpText.transform.parent.gameObject.SetActive(true);
    }

    protected override void OnUpdate()
    {
        Debug.Log(Dependency.ToString());
        hpStats.Update(this);
        attackStats.Update(this);
        movementStats.Update(this);
        Entity entity = SystemAPI.GetSingleton<UnitStatsRequestComponent>().entity;
        StaticUIRefs.Instance.HpText.text = $"HP: {hpStats[entity].curHp} / {hpStats[entity].maxHp}";
        StaticUIRefs.Instance.AttackText.text = $"DMG: {attackStats[entity].damage}";
        StaticUIRefs.Instance.ReloadText.text = $"ReloadTime: {attackStats[entity].curReload} / {attackStats[entity].reloadLen}";
        StaticUIRefs.Instance.AttackRadiusText.text = $"AttackRadius: {math.sqrt(attackStats[entity].radiusSq)}";
        StaticUIRefs.Instance.MovementText.text = $"Speed: {movementStats[entity].speed}";
    }

    protected override void OnStopRunning()
    {
        StaticUIRefs.Instance.HpText.transform.parent.gameObject.SetActive(false);
    }
}

public struct UnitStatsRequestComponent : IComponentData
{
    public Entity entity;
}
