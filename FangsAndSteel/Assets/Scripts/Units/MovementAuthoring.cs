using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class MovementAuthoring : MonoBehaviour
{
    public float speed;
    public float3 target;
    public bool isMoving = false;
    public class Baker : Baker<MovementAuthoring>
    {
        public override void Bake(MovementAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(this.GetEntity(TransformUsageFlags.Dynamic), new MovementComponent 
            { 
                speed = authoring.speed, 
                target = authoring.target, 
                isMoving = authoring.isMoving,

                rotTimePassed = 0,
                lastRotTarget = quaternion.identity
            });
            AddBuffer<MovementCommandsBuffer>(entity);
        }
    }
}


public struct MovementComponent : IComponentData
{
    public float speed;
    public float3 target;
    public bool isMoving;

    public float rotTimePassed;
    public quaternion lastRotTarget;
}

[InternalBufferCapacity(16)]
public struct MovementCommandsBuffer : IBufferElementData
{
    public float3 target;
}
