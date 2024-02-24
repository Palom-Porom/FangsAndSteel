using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class UnitTypeAuthoring : MonoBehaviour
{
    public UnitTypes typeOfUnit = UnitTypes.None;

    public class Baker : Baker<UnitTypeAuthoring>
    {
        public override void Bake(UnitTypeAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new UnitTypeComponent { value = authoring.typeOfUnit });
        }
    }
}


[Flags]
public enum UnitTypes
{
    None = 0,
    BaseInfantry = 1,
    MachineGunner = 2,
    AntiTankInf = 4,
    Tank = 8,
    Artillery = 16,
    Everything = 31
}

///<summary> Contains info about to what types of unit this one belongs to </summary>
public struct UnitTypeComponent : IComponentData
{
    public UnitTypes value;
}