using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class NotRotateAttackModelAuthoring : MonoBehaviour
{
    public class Baker : Baker<NotRotateAttackModelAuthoring>
    {
        public override void Bake(NotRotateAttackModelAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent<NotRotateAttackModelTag>(entity);
        }
    }
}

public struct NotRotateAttackModelTag : IComponentData { }
