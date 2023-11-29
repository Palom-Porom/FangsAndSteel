using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class AttackAuthoring : MonoBehaviour
{
    public int damage = 0;
    public float realodLen = 1;
    public int attackRadius = 0;
    public float timeToShoot = 1;
    public class Baker : Baker<AttackAuthoring>
    {
        public override void Bake(AttackAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new AttackComponent 
            { 
                damage = authoring.damage,
                reloadLen = authoring.realodLen,
                curReload = 0,
                radiusSq = authoring.attackRadius * authoring.attackRadius,
                target = Entity.Null, 

                timeToShoot = authoring.timeToShoot
            });

            AddComponent(entity, new AttackSettingsComponent 
            {
                isAbleToMove = true,

                targettingMinHP = false,
                shootingOnMoveMode = false
            });
        }
    }
}
public struct AttackComponent : IComponentData
{
    public int damage;
    public float reloadLen;
    public float curReload;
    public int radiusSq;
    public Entity target;

    public float timeToShoot;
}

public struct AttackSettingsComponent : IComponentData
{
    public bool isAbleToMove;

    public bool targettingMinHP;
    public bool shootingOnMoveMode;
}


/// <summary>
/// Used to put isAbleToMove = false for some time
/// </summary>
public struct NotAbleToMoveForTimeRqstComponent : IComponentData
{
    public float targetTime;
    public float passedTime;
}
