using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class PrefubsAuthoring : MonoBehaviour
{
    public GameObject flag1;
    public GameObject flag2;
    public GameObject flag3;
    public GameObject flag4;
    public GameObject flag5;
    public GameObject flag6;
    public GameObject flag7;
    public GameObject flag8;
    public GameObject flag9;

    public GameObject FirstRobot_Red;
    public GameObject FirstRobot_Blue;
    public GameObject MachineGunner_Red;
    public GameObject MachineGunner_Blue;
    public GameObject AntyTank_Red;
    public GameObject AntyTank_Blue;
    public GameObject Tank_Red;
    public GameObject Tank_Blue;

    public class Baker : Baker<PrefubsAuthoring>
    {
        public override void Bake(PrefubsAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new PrefubsComponent
            {
                flag1 = GetEntity(authoring.flag1, TransformUsageFlags.Dynamic),
                flag2 = GetEntity(authoring.flag2, TransformUsageFlags.Dynamic),
                flag3 = GetEntity(authoring.flag3, TransformUsageFlags.Dynamic),
                flag4 = GetEntity(authoring.flag4, TransformUsageFlags.Dynamic),
                flag5 = GetEntity(authoring.flag5, TransformUsageFlags.Dynamic),
                flag6 = GetEntity(authoring.flag6, TransformUsageFlags.Dynamic),
                flag7 = GetEntity(authoring.flag7, TransformUsageFlags.Dynamic),
                flag8 = GetEntity(authoring.flag8, TransformUsageFlags.Dynamic),
                flag9 = GetEntity(authoring.flag9, TransformUsageFlags.Dynamic),

                FirstRobot_Red = GetEntity(authoring.FirstRobot_Red, TransformUsageFlags.Dynamic),
                FirstRobot_Blue = GetEntity(authoring.FirstRobot_Blue, TransformUsageFlags.Dynamic),
                MachineGunner_Red = GetEntity(authoring.MachineGunner_Red, TransformUsageFlags.Dynamic),
                MachineGunner_Blue = GetEntity(authoring.MachineGunner_Blue, TransformUsageFlags.Dynamic),
                AntyTank_Red = GetEntity(authoring.AntyTank_Red, TransformUsageFlags.Dynamic),
                AntyTank_Blue = GetEntity(authoring.AntyTank_Blue, TransformUsageFlags.Dynamic),
                Tank_Red = GetEntity(authoring.Tank_Red, TransformUsageFlags.Dynamic),
                Tank_Blue = GetEntity(authoring.Tank_Blue, TransformUsageFlags.Dynamic),
            });
            //AddComponent(entity, new SelectTag { selectionRing = GetEntity(authoring.selectionRing, TransformUsageFlags.None) });
        }
    }
}

public struct PrefubsComponent : IComponentData
{
    public Entity flag1;
    public Entity flag2;
    public Entity flag3;
    public Entity flag4;
    public Entity flag5;
    public Entity flag6;
    public Entity flag7;
    public Entity flag8;
    public Entity flag9;

    public Entity FirstRobot_Red;
    public Entity FirstRobot_Blue;
    public Entity MachineGunner_Red;
    public Entity MachineGunner_Blue;
    public Entity AntyTank_Red;
    public Entity AntyTank_Blue;
    public Entity Tank_Red;
    public Entity Tank_Blue;
}

public struct FlagTag : IComponentData { }