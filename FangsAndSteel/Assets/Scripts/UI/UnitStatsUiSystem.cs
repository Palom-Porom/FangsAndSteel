using System;
using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public partial class UnitStatsUiSystem : SystemBase
{
    ComponentLookup<HpComponent> hpStats;
    ComponentLookup<AttackComponent> attackStats;
    ComponentLookup<MovementComponent> movementStats;
    protected override void OnCreate()
    {
        RequireForUpdate<UnitStatsRequestComponent>();
        hpStats = GetComponentLookup<HpComponent>();
        attackStats = GetComponentLookup<AttackComponent>();
        movementStats = GetComponentLookup<MovementComponent>();
    }

    protected override void OnUpdate()
    {
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
}

public struct UnitStatsRequestComponent : IComponentData
{
    public Entity entity;
}
