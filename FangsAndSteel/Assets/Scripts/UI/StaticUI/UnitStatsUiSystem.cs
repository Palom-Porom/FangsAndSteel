using System;
using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[UpdateInGroup(typeof(NoJobsSystemGroup))]
public partial class UnitStatsUiSystem : SystemBase
{
    ComponentLookup<HpComponent> hpStats;
    ComponentLookup<AttackComponent> attackStats;
    ComponentLookup<MovementComponent> movementStats;
    ComponentLookup<VisionCharsComponent> visionStats;
    protected override void OnCreate()
    {
        RequireForUpdate<UnitStatsRequestTag>();
        hpStats = SystemAPI.GetComponentLookup<HpComponent>();
        attackStats = SystemAPI.GetComponentLookup<AttackComponent>();
        movementStats = SystemAPI.GetComponentLookup<MovementComponent>();
        visionStats = SystemAPI.GetComponentLookup<VisionCharsComponent>();
    }

    protected override void OnStartRunning()
    {
        StaticUIRefs.Instance.HpText.transform.parent.gameObject.SetActive(true);
    }

    protected override void OnUpdate()
    {
        hpStats.Update(this);
        attackStats.Update(this);
        movementStats.Update(this);
        visionStats.Update(this);
        Entity entity = SystemAPI.GetSingletonEntity<UnitStatsRequestTag>();
        StaticUIRefs.Instance.HpText.text = $"��������: {hpStats[entity].curHp} / {hpStats[entity].maxHp}";
        StaticUIRefs.Instance.AttackText.text = $"����: {attackStats[entity].damage}";
        StaticUIRefs.Instance.ReloadText.text = $"�����������: {attackStats[entity].curReload:f2} / {attackStats[entity].reloadLen}";
        StaticUIRefs.Instance.AttackRadiusText.text = $"������ �����: {math.sqrt(attackStats[entity].radiusSq)}";
        StaticUIRefs.Instance.MovementText.text = $"��������: {movementStats[entity].speed}";
        StaticUIRefs.Instance.VisionRadiusText.text = $"������ ������: {visionStats[entity].radius}";
    }

    protected override void OnStopRunning()
    {
        StaticUIRefs.Instance.HpText.transform.parent.gameObject.SetActive(false);
    }
}

public struct UnitStatsRequestTag : IComponentData
{
}
