using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class SelectTagAuthoring : MonoBehaviour
{
    public class Baker : Baker<SelectTagAuthoring>
    {
        public override void Bake(SelectTagAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent<SelectTag>(entity);
        }
    }
}

public struct SelectTag : IComponentData, IEnableableComponent { }
