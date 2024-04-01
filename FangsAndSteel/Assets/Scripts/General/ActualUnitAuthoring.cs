using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ActualUnitAuthoring : MonoBehaviour
{
    public class Baker : Baker<ActualUnitAuthoring>
    {
        public override void Bake(ActualUnitAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<ActualEntityTag>(entity);
        }
    }
}

public struct ReplayStartCopyTag : IComponentData { }
public struct ReplayCopyTag : IComponentData { }
public struct ActualEntityTag : IComponentData { }
public struct WasDisabledTag : IComponentData { }
