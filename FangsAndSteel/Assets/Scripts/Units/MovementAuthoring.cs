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
            AddComponent(this.GetEntity(TransformUsageFlags.Dynamic), new MovementComponent { speed = authoring.speed, target = authoring.target, isMoving = authoring.isMoving });
        }
    }
}


public struct MovementComponent : IComponentData
{
    public float speed;
    public float3 target;
    public bool isMoving;
}
