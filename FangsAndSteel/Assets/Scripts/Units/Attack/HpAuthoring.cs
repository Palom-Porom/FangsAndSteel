using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class HpAuthoring : MonoBehaviour
{
    public float maxHp = 0;
    public float curHp = 0;
    public float timeToDie = 0;
    public class Baker : Baker<HpAuthoring>
    {
        public override void Bake(HpAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new HpComponent { maxHp = authoring.maxHp, curHp = authoring.curHp, timeToDie = authoring.timeToDie });
        }
    }
}

[GhostComponent(PrefabType = GhostPrefabType.All)]
public struct HpComponent : IComponentData
{
    public float maxHp;
    public float curHp;
    public float timeToDie;
}

