using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class UnitsIconsAuthoring : MonoBehaviour
{
    public GameObject infoQuadsEntity;
    public class Baker : Baker<UnitsIconsAuthoring>
    {
        public override void Bake(UnitsIconsAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new UnitsIconsComponent { infoQuadsEntity = GetEntity(authoring.infoQuadsEntity, TransformUsageFlags.Dynamic) });
        }
    }
}
            
            
        
    

public struct UnitsIconsComponent: IComponentData
{
    public Entity infoQuadsEntity;
}
