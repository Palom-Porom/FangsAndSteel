using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

public class AttackAuthoring : MonoBehaviour
{
    public float damage = 0;
    public int attackRadius = 0;
    public float timeToShoot = 1;

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
                target = Entity.Null, 

                timeToShoot = authoring.timeToShoot
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
                shootAnimLen = authoring.timeToShoot
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
        }
    }
}
public struct AttackCharsComponent : IComponentData
{
    public float damage;
    public int radiusSq;
    public Entity target;

    public float timeToShoot;
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

    public float shootAnimElapsed;
    public float shootAnimLen;

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
    //TODO: *list of enemies to auto-trigger* (?enum + [flags]?)
    ///<value> Time of pursuing without a shot until dropping the auto-triggered target </value>
    //public float autoTriggerDropTime;
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


/// <summary>
/// Used to put <c>isAbleToMove = false</c> for some time
/// </summary>
public struct NotAbleToMoveForTimeRqstComponent : IComponentData
{
    public float targetTime;
    public float passedTime;
}
