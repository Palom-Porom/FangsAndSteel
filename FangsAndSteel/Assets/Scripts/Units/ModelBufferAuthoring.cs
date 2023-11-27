using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ModelBufferAuthoring : MonoBehaviour
{
    public GameObject ModelsGameObject;
    public class Baker : Baker<ModelBufferAuthoring>
    {
        public override void Bake(ModelBufferAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            var buf = AddBuffer<ModelsBuffer>(entity);
            for (int i = 0; i < authoring.ModelsGameObject.transform.childCount; i++)
            {
                Entity modelEntity = GetEntity(authoring.ModelsGameObject.transform.GetChild(i), TransformUsageFlags.Dynamic);
                buf.Add(new ModelsBuffer { model = modelEntity });
            }    
        }
    }
}


[InternalBufferCapacity(5)]
public struct ModelsBuffer : IBufferElementData
{
    public Entity model;
}