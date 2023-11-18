using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using Unity.Entities;
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

    bool initialFillDone;
    StaticUIData uIData;
    protected override void OnUpdate()
    {
        hpStats.Update(this);
        attackStats.Update(this);
        movementStats.Update(this);
        Entity entity = SystemAPI.GetSingleton<UnitStatsRequestComponent>().entity;
        if ( !initialFillDone )
        {
            StaticUIRefs.Instance.hptext
        }
    }
}

public struct UnitStatsRequestComponent : IComponentData
{
    public Entity entity;
}