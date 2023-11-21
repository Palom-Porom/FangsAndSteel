using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class HpAuthoring : MonoBehaviour
{
    public int maxHp = 0;
    public int curHp = 0;
    public class Baker : Baker<HpAuthoring>
    {
        public override void Bake(HpAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new HpComponent { maxHp = authoring.maxHp, curHp = authoring.curHp });
        }
    }
}
public struct HpComponent : IComponentData
{
    public int maxHp;
    public int curHp;
}

