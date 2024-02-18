using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class AttackAuthoring : MonoBehaviour
{
    public float damage = 0;
    public int attackRadius = 0;
    ///TODO: automatically get the value below if it is possible
    public float shootingAnimationLen = 1;

    public int maxBullets = 1;
    public float bulletReload = 0.5f;
    public float drumReload = 3f;
    public float reload_SoM_Debuff = 0.5f;

    public float dropTime = 5f;
    public float dropDistance = 20f;
    
    public class Baker : Baker<AttackAuthoring>
    {
        public override void Bake(AttackAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new AttackCharsComponent 
            { 
                damage = authoring.damage,
                radiusSq = authoring.attackRadius * authoring.attackRadius,
                target = Entity.Null
            });

            AddComponent(entity, new ReloadComponent
            {
                maxBullets = authoring.maxBullets,
                curBullets = authoring.maxBullets,

                bulletReloadElapsed = authoring.bulletReload,
                bulletReloadLen = authoring.bulletReload,

                drumReloadElapsed = 0f,
                drumReloadLen = authoring.drumReload,

                reload_SoM_Debaff = authoring.reload_SoM_Debuff,
                curDebaff = 0,

                shootAnimElapsed = 0,
                shootAnimLen = authoring.shootingAnimationLen
            });

            AddComponent(entity, new BattleModeComponent
            {
                shootingDisabled = false,
                shootingOnMove = false,
                
                autoTriggerMoving = false,
                autoTriggerRadiusSq = authoring.attackRadius,
                autoTriggerMaxHpPercent = 100
            });

            AddComponent(entity, new PursuingModeComponent
            {
                Target = Entity.Null,
                
                dropTime = authoring.dropTime,
                dropTimeElapsed = 0,
                
                maxShootDistanceSq = (authoring.attackRadius - 5f) * (authoring.attackRadius - 5f),
                dropDistanceSq = authoring.dropDistance * authoring.dropDistance
            });
            SetComponentEnabled<PursuingModeComponent>(entity, false);

            AddComponent(entity, new SimpleAttackPrioritiesComponent
            {
                distanceModifier = 1,
                minHpModifier = 0
            });
            AddBuffer<UnitsPrioritiesBuffer>(entity);
            AddBuffer<ZonesPrioritiesBuffer>(entity);
        }
    }
}


///<summary> General attacker characteristics </summary>
public struct AttackCharsComponent : IComponentData
{
    ///<value> Damage which is done to target during one attack </value>
    public float damage;
    ///<value> Square of radius of attack distance of unit </value>
    ///<remarks> Squared for more efficiency </remarks>
    public int radiusSq;
    ///<value> Current target of attack. If unit can he will do an attack on him </value>
    public Entity target;
}


///<summary> Info about "bullets" and reload status of unit </summary>>
[BurstCompile]
public struct ReloadComponent : IComponentData
{
    /// <value> Max "bullets" amount in a Drum </value>
    public int maxBullets;
    /// <value> Current "bullets" amount </value>
    public int curBullets;

    /// <value> Current state of reload between "bullets" </value>
    public float bulletReloadElapsed;
    /// <value> Overall reload time between "bullets" </value>
    public float bulletReloadLen;

    /// <value> Current state of reload of the "Drum" </value>
    public float drumReloadElapsed;
    /// <value> Overall reload time of the "Drum" </value>
    public float drumReloadLen;


    ///<value> Percentage of reload speed debbaf when ShootingOnMove mode is enabled </value>
    ///<remarks> !Value 0 to 1! </remarks>
    public float reload_SoM_Debaff;
    ///<value> Percentage of current reload speed debbaf </value>
    public float curDebaff;

    ///<value> Time elapsed after last attack was made </value>
    public float shootAnimElapsed;
    ///<value> Time needed to pass after last attack was made to start reloading process and play reload animation </value>
    ///<remarks> Basically equals to the length of the attack animation </remarks>
    public float shootAnimLen; ///TODO: automatically get that value if it is possible

    ///<value> If true then unit is ready to fire </value>
    public bool isReloaded() { return curBullets > 0 && bulletReloadElapsed >= bulletReloadLen; }
}


///<summary> One of units ActionModes </summary>
///<remarks> Usual mode of unit </remarks>
public struct BattleModeComponent : IComponentData, IEnableableComponent
{
    ///<value> No shooting, but searching for targets still enabled </value>
    public bool shootingDisabled;

    ///<value> Can reload on move, but slower reload + slower movement </value>
    public bool shootingOnMove;

    ///<value> Is auto-trigger enabled when unit is not moving </value>
    //public bool autoTriggerStatic; <-- suppose it is not so useful option for player 
    ///<value> Is auto-trigger enabled when unit is moving </value>
    public bool autoTriggerMoving;
    ///<value> Radius in which auto-trigger will work </value>
    public float autoTriggerRadiusSq;
    ///<value> Max percentage (0 to 1) of Hp which target can have for auto-trigger to work </value>
    public int autoTriggerMaxHpPercent;
    ///<value> Unit types for which auto-trigger will work </value>
    public uint autoTriggerUnitTypes;
}


///<summary> One of units ActionModes </summary>
///<remarks> Represents the state of pusuing some particular target </remarks>
public struct PursuingModeComponent : IComponentData, IEnableableComponent
{
    /// <summary> Target which is pursued </summary>
    public Entity Target;

    /// <summary> Time after which pursuing will be dropped if no shot was made </summary>
    public float dropTime;
    /// <summary> Elapsed time wihtout a shot </summary>
    public float dropTimeElapsed;

    ///<summary> Order to shoot no further than this distance (squared)</summary>
    public float maxShootDistanceSq;
    /// <summary> If distance (squared) to target is bigger than this - pursuing will be dropped </summary>
    public float dropDistanceSq;
}


public struct SimpleAttackPrioritiesComponent : IComponentData
{
    public float distanceModifier;
    public float minHpModifier;
}

[InternalBufferCapacity(7)]
public struct UnitsPrioritiesBuffer : IBufferElementData
{
    public uint types;
    public float modifier;
}

[InternalBufferCapacity(10)]
public struct ZonesPrioritiesBuffer : IBufferElementData
{
    public float2 leftCorner;
    public float2 rightCorner;
    public float modifier;
}

public readonly partial struct AttackPrioritiesAspect : IAspect
{
    public readonly RefRW<SimpleAttackPrioritiesComponent> simplePriorities;
    public readonly DynamicBuffer<UnitsPrioritiesBuffer> unitsPriorities;
    public readonly DynamicBuffer<ZonesPrioritiesBuffer> zonesPriorities;

    public float DistanceModifier { get => simplePriorities.ValueRO.distanceModifier; set => simplePriorities.ValueRW.distanceModifier = value; }
    public float MinHpModifier { get => simplePriorities.ValueRO.minHpModifier; set => simplePriorities.ValueRW.minHpModifier = value; }

    public void UnitsPriority_Add(uint types, float modifier) => unitsPriorities.Add(new UnitsPrioritiesBuffer { modifier = modifier, types = types });
    public void UnitsPriority_Delete(uint types)
    {
        int curLen = unitsPriorities.Length;
        for (int i = 0; i < curLen; i++)
        {
            unitsPriorities.ElementAt(i).types &= ~types;
            if (unitsPriorities.ElementAt(i).types == 0)
            {
                unitsPriorities.RemoveAtSwapBack(i);
                curLen--;
                i--;
            }
        }
    }
    public void UnitsPriority_ChangeModifier(uint types, float newModifier)
    {
        for (int i = 0; i < unitsPriorities.Length; i++)
        {
            if (unitsPriorities.ElementAt(i).types == types)
            {
                unitsPriorities.ElementAt(i).modifier = newModifier;
                return;
            }
        }
        Debug.Log("ERROR: Changed units modifier of unknown cell!");
    }
    public void UnitsPriority_ExchangeTypes(uint types1, uint types2)
    {
        int idx1 = -1, idx2 = -1;
        for(int i = 0; i < unitsPriorities.Length; i++)
        {
            if (idx1 == -1 && unitsPriorities.ElementAt(i).types == types1)
                idx1 = i;
            if (idx2 == -1 && unitsPriorities.ElementAt(i).types == types2)
                idx2 = i;
        }
        var temp = unitsPriorities.ElementAt(idx1).types;
        unitsPriorities.ElementAt(idx1).types = unitsPriorities.ElementAt(idx2).types;
        unitsPriorities.ElementAt(idx2).types = temp;
    }

    ///TODO: zones priorities methods
}


/// <summary>
/// Used to put <c>isAbleToMove = false</c> for some time
/// </summary>
public struct NotAbleToMoveForTimeRqstComponent : IComponentData
{
    public float targetTime;
    public float passedTime;
}
