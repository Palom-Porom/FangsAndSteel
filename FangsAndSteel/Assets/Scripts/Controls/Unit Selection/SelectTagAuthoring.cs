using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class SelectTagAuthoring : MonoBehaviour
{
    public GameObject selectionRing;
    public class Baker : Baker<SelectTagAuthoring>
    {
        public override void Bake(SelectTagAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new SelectTag { selectionRing = GetEntity(authoring.selectionRing, TransformUsageFlags.None) });
        }
    }
}

public struct SelectTag : IComponentData, IEnableableComponent 
{
    public Entity selectionRing;
}
