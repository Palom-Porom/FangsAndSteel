using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[HelpURL("dsjaklf")]
public class MovementAuthoring : MonoBehaviour
{
    [Header("Movement Stats")]
    public float speed;
    public float3 target;
    public bool isMoving = false;

    [Space(15)]

    [Header("Rotation Stats")]
    public float rotationTime = 0.33f;

    [Space(15)]

    [Header("Vehicle Stats")]
    public bool isVehicle = false;
    [Space(5)]
    public float minMovementMultiplier = 0.1f;
    [Space(5)]
    public float variationUsualSpeed = 0.01f;
    public float variationExtremeSpeed = 0.05f;
    [Space(5)]
    public float degreesPerSec = 45;
    public float extremeVariationRotationDelta = 90;
    public float minRotationDeltaForEffect = 5;


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
                isAbleToMove = true
            });

            AddComponent(entity, new RotationToTargetComponent
            {
                rotTime = authoring.rotationTime,
                rotTimeElapsed = 0f,
                initialRotation = quaternion.identity,
                curRotTarget = Quaternion.identity,
                newRotTarget = Quaternion.identity
            });

            if (authoring.isVehicle)
            {
                AddComponent(entity, new VehicleMovementComponent
                {
                    minMovementMultiplier = authoring.minMovementMultiplier,
                    curMovementSpeedMultiplier = 1,

                    variationUsualSpeed = authoring.variationUsualSpeed,
                    variationExtremeSpeed = authoring.variationExtremeSpeed,

                    degreesPerSecond = authoring.degreesPerSec,
                    rotTimeElapsed = 0,
                    rotationDeltaY = 0,
                    extremeVariationRotationDelta = authoring.extremeVariationRotationDelta,
                    minRotationDeltaForEffect = authoring.minRotationDeltaForEffect,
                    
                    curTargetDir = new float3(0, 0, 1)
                });
            }

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


[BurstCompile]
public struct VehicleMovementComponent: IComponentData
{
    public float minMovementMultiplier;
    public float curMovementSpeedMultiplier;

    public float variationUsualSpeed;
    public float variationExtremeSpeed;

    public float degreesPerSecond;
    ///<value> Time elapsed from setting current rotation target </value>
    public float rotTimeElapsed;
    ///<value> Time in which rotation to target will be done </value>
    public float rotTime;
    /// <value> Value of Rotation in the start of Rotating process </value>
    public quaternion initialRotation;
    ///<value> Target to which was rotating in the last frame </value>
    public quaternion curRotTarget;
    public float3 curTargetDir;
    public float3 newTargetDir;
    public quaternion rotationDiff;
    public float rotationDeltaY;
    public float extremeVariationRotationDelta;
    public float minRotationDeltaForEffect;

    public float3 temporaryTarget;

    private float VariationSpeed { get { return rotationDeltaY < extremeVariationRotationDelta ? variationUsualSpeed : variationExtremeSpeed; } }
    private float CurrentMovementMultiplierTarget { get { return math.max((1 - rotationDeltaY / extremeVariationRotationDelta), minMovementMultiplier); } }

    public void IncreaseMovementMultiplier()
    {
        curMovementSpeedMultiplier += VariationSpeed;
        if (curMovementSpeedMultiplier >= 1)
        {
            curMovementSpeedMultiplier = 1;
            rotationDeltaY = 0;
        }
    }

    public void DecreaseMovementMultiplier()
    {
        if (curMovementSpeedMultiplier > CurrentMovementMultiplierTarget)
        {
            curMovementSpeedMultiplier -= VariationSpeed;
        }
        else
        {
            IncreaseMovementMultiplier();
        }
    }

    public void UpdateMovementMultiplier()
    {
        var curTarget = CurrentMovementMultiplierTarget;
        if (math.abs(curTarget - curMovementSpeedMultiplier) < float.Epsilon)
            return;
        if (math.abs(curTarget - curMovementSpeedMultiplier) < VariationSpeed)
            curMovementSpeedMultiplier = curTarget;
        else
            curMovementSpeedMultiplier += math.sign(curTarget - curMovementSpeedMultiplier) * VariationSpeed;
    }
}



[InternalBufferCapacity(10)]
public struct MovementCommandsBuffer : IBufferElementData
{
    public float3 target;

    //AttackSettingsComponent
    //public bool targettingMinHP;
    //public bool shootingOnMoveMode;
}
