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

    public float rotationTime = 0.33f;
    public class Baker : Baker<MovementAuthoring>
    {
        public override void Bake(MovementAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new MovementComponent 
            { 
                speed = authoring.speed, 
                target = authoring.target, 
                hasMoveTarget = authoring.isMoving,

                rotTimeElapsed = 0,
                initialRotation = quaternion.identity
            });
            AddComponent(entity, new RotationToTargetComponent
            {
                rotTime = authoring.rotationTime,
                rotTimeElapsed = 0f,
                initialRotation = quaternion.identity,
                curRotTarget = Quaternion.identity,
                newRotTarget = Quaternion.identity
            });
            AddBuffer<MovementCommandsBuffer>(entity);
        }
    }
}


public struct MovementComponent : IComponentData
{
    public float speed;
    public float3 target;
    public bool hasMoveTarget;
    public bool isAbleToMove;

    ///<value> Percentage of movement speed debbaf when ShootingOnMove mode is enabled </value>
    ///<remarks> Value is 0 to 1 </remarks>
    public readonly float movement_SoM_Debaff;
    ///<value> Current summary movement speed debaff </value>
    ///<remarks> Value is 0 to 1 </remarks>
    public float curDebaff;

    public float rotTimeElapsed;
    public quaternion initialRotation;
    public quaternion curRotTarget;
}



///<summary> Component used for smooth rotating to some target </summary>
public struct RotationToTargetComponent : IComponentData
{
    ///<value> Time elapsed from setting current rotation target </value>
    public float rotTimeElapsed;
    ///<value> Time in which rotation to target will be done </value>
    public float rotTime;
    /// <value> Value of Rotation in the start of Rotating process </value>
    public quaternion initialRotation;
    ///<value> Target to which was rotating in the last frame </value>
    public quaternion curRotTarget;
    ///<value> Rotating-target of the current frame </value>
    public quaternion newRotTarget;
}



///<summary> Info about rotation on target for attack </summary>
///<remarks> Used only for those Entities which have AttackModelsBuffers </remarks>
public struct AttackRotationToTargetComponent : IComponentData 
{
    ///<value> Time elapsed from setting current rotation target </value>
    public float rotTimeElapsed;
    ///<value> Time in which rotation to target will be done </value>
    public float rotTime;
    /// <value> Value of Rotation in the start of Rotating process </value>
    public quaternion initialRotation;
    ///<value> Target to which was rotating in the last frame </value>
    public quaternion curRotTarget;
    ///<value> Rotating-target of the current frame </value>
    public quaternion newRotTarget;

    ///<value> Time elapsed since last rotation </value>
    public float noRotTimeElapsed;
    ///<value> If there was no rotation for this time -> returns to default state rotation </value>
    public float timeToReturnRot;
    ///<value> Is rotating to default state of rotation at the moment? </value>
    public bool isRotatingToDefault;
    ///<value> If true then all attackModels are in default state of rotation </value>
    public bool isInDefaultState;
}



[InternalBufferCapacity(10)]
public struct MovementCommandsBuffer : IBufferElementData
{
    public float3 target;

    //AttackSettingsComponent
    public bool targettingMinHP;
    public bool shootingOnMoveMode;
}
