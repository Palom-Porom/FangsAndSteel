using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using UnityEngine;

public class ModelBufferAuthoring : MonoBehaviour
{
    [Tooltip("Parent GO of all unit`s models")]
    public GameObject modelsGameObject;

    [Tooltip("Models for separate rotation when unit is attacking\nFor example turret of tank")]
    public GameObject[] attackModels;

    [Tooltip("Needed only if attackModels array is not empty!")]
    public float attackRotationTime = 0.33f;
    [Tooltip("Needed only if attackModels array is not empty!")]
    public float timeToReturnRot = 1f;


    public class Baker : Baker<ModelBufferAuthoring>
    {
        public override void Bake(ModelBufferAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            var buf = AddBuffer<ModelsBuffer>(entity);
            for (int i = 0; i < authoring.modelsGameObject.transform.childCount; i++)
            {
                Entity modelEntity = GetEntity(authoring.modelsGameObject.transform.GetChild(i), TransformUsageFlags.Dynamic);
                buf.Add(new ModelsBuffer { model = modelEntity });
            }
            
            //if has attack models -> add buffer and component for separate attack-targeting
            if (authoring.attackModels.Length != 0)
            {
                AddComponent(entity, new AttackRotationToTargetComponent
                {
                    rotTime = authoring.attackRotationTime,
                    rotTimeElapsed = 0f,
                    initialRotation = quaternion.identity,
                    curRotTarget = Quaternion.identity,
                    newRotTarget = Quaternion.identity,

                    noRotTimeElapsed = 0f,
                    timeToReturnRot = authoring.timeToReturnRot,
                    isInDefaultState = true,
                    isRotatingToDefault = false
                });

                var attackModelsBuf = AddBuffer<AttackModelsBuffer>(entity);
                foreach(var go in authoring.attackModels)
                {
                    Entity modelEntity = GetEntity(go, TransformUsageFlags.Dynamic);
                    attackModelsBuf.Add(new AttackModelsBuffer { model = modelEntity });
                }
            }
        }
    }
}


[InternalBufferCapacity(5)]
public struct ModelsBuffer : IBufferElementData
{
    public Entity model;

    public static implicit operator ModelsBuffer(Entity model)
    {
        return new ModelsBuffer { model = model };
    }

    public static implicit operator Entity (ModelsBuffer element)
    {
        return element.model;
    }
}


[InternalBufferCapacity(5)]
public struct AttackModelsBuffer : IBufferElementData
{
    public Entity model;

    public static implicit operator AttackModelsBuffer(Entity model)
    {
        return new AttackModelsBuffer { model = model };
    }

    public static implicit operator Entity(AttackModelsBuffer element)
    {
        return element.model;
    }
}