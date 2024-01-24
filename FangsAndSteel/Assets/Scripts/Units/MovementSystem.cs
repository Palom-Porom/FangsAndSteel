using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;
using AnimCooker;
using System.Net.Security;
using System;
using Unity.Jobs;
using static AnimDb;

[UpdateInGroup(typeof(UnitsSystemGroup))]
[BurstCompile]
public partial struct MovementSystem : ISystem, ISystemStartStop
{
    ComponentLookup<LocalTransform> transformLookup;
    
    ComponentLookup<AnimationCmdData> animCmdLookup;
    ComponentLookup<AnimationStateData> animStateLookup;
    NativeArray<AnimDbEntry> restClips;


    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
        state.RequireForUpdate<MovementComponent>();
        state.RequireForUpdate<AnimDbRefData>();

        transformLookup = state.GetComponentLookup<LocalTransform>();

        animCmdLookup = state.GetComponentLookup<AnimationCmdData>();
        animStateLookup = state.GetComponentLookup<AnimationStateData>();
    }

    public void OnStartRunning(ref SystemState state)
    {
        restClips = SystemAPI.GetSingleton<AnimDbRefData>().FindClips("Rest");        
    }

    public void OnStopRunning(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) 
    {
        transformLookup.Update(ref state);

        animCmdLookup.Update(ref state);
        animStateLookup.Update(ref state);


        #region MovementJob
        var moveJob = new MovementJob
        {
            deltaTime = SystemAPI.Time.DeltaTime,
            collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld,

            animCmdLookup = animCmdLookup,
            animStateLookup = animStateLookup,
            restClips = restClips
        };
        JobHandle movementJobHandle = moveJob.Schedule(state.Dependency);
        #endregion

        #region RotationJobs
        JobHandle rotationJobHandle = new RotationToTargetJob
        {
            deltaTime = SystemAPI.Time.DeltaTime
        }.Schedule(movementJobHandle);

        JobHandle attackRotationJobHandle = new AttackRotationToTargetJob
        {
            deltaTime = SystemAPI.Time.DeltaTime,
            transformLookup = transformLookup
        }.Schedule(movementJobHandle);

        state.Dependency = JobHandle.CombineDependencies(attackRotationJobHandle, rotationJobHandle);
        #endregion
    }


}

/// <summary>
/// Moves all entities with MovementComponent to their target over the terrain
/// </summary>
[BurstCompile]
public partial struct MovementJob : IJobEntity
{
    public float deltaTime;
    [ReadOnly] public CollisionWorld collisionWorld;

    public ComponentLookup<AnimationCmdData> animCmdLookup;
    [ReadOnly] public ComponentLookup<AnimationStateData> animStateLookup;
    [ReadOnly] public NativeArray<AnimDbEntry> restClips;

    public void Execute(ref LocalTransform transform, ref MovementComponent movementComponent, ref RotationToTargetComponent rotation,
        DynamicBuffer<MovementCommandsBuffer> movementCommandsBuffer, in DynamicBuffer<ModelsBuffer> modelsBuf, in AttackSettingsComponent attackSettings)
    {
        if (!attackSettings.isAbleToMove)
            return;

        if (!movementComponent.hasMoveTarget)
            return;

        if (math.distancesq(movementComponent.target, transform.Position) < (deltaTime * movementComponent.speed) / 2)
        {
            //Has next target to move?
            if (movementCommandsBuffer.Length != 0)
            {
                movementComponent.target = movementCommandsBuffer[0].target;
                movementCommandsBuffer.RemoveAt(0);
            }
            else
            {
                movementComponent.hasMoveTarget = false;
                foreach (var modelBufElem in modelsBuf)
                {
                    RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
                    animCmd.ValueRW.ClipIndex = restClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                    animCmd.ValueRW.Cmd = AnimationCmd.SetPlayForever;
                }
                return;
            } 
        }

        float3 tempDir = movementComponent.target - transform.Position;
        tempDir.y = 0;
        tempDir = math.normalize(tempDir);
        float speed = deltaTime * movementComponent.speed * (1 - movementComponent.curDebaff);
        if (speed < 0)
            Debug.Log("ERROR: Movement debuff is higher than 1!");

        CollisionFilter filter = new CollisionFilter
        {
            BelongsTo = (uint)layers.Everything,
            CollidesWith = (uint)layers.Terrain,
            GroupIndex = 0
        };
        float3 pointDistancePos = transform.Position + tempDir * speed;
        var pointDistanceInput = new PointDistanceInput { Filter = filter, Position = pointDistancePos, MaxDistance = 1.5f };
        if (collisionWorld.CalculateDistance(pointDistanceInput, out DistanceHit closestHit))
        {
            if (closestHit.SurfaceNormal.y < 0f)
                closestHit.SurfaceNormal = -closestHit.SurfaceNormal;

            rotation.newRotTarget = quaternion.LookRotationSafe(closestHit.Position - transform.Position, closestHit.SurfaceNormal);
            //if (UtilityFuncs.Nearly_Equals_quaternion(newTargetRot, movementComponent.curRotTarget))
            //{
            //    if (movementComponent.rotTimeElapsed < ROT_TIME)
            //    {
            //        movementComponent.rotTimeElapsed += deltaTime;
            //        transform.Rotation = math.nlerp(movementComponent.initialRotation, newTargetRot, movementComponent.rotTimeElapsed / ROT_TIME);
            //        //if (movementComponent.rotTimePassed / ROT_TIME > 0.5)
            //        Debug.Log(movementComponent.rotTimeElapsed / ROT_TIME);
            //    }
            //}
            //else
            //{
            //    movementComponent.rotTimeElapsed = deltaTime;
            //    movementComponent.initialRotation = transform.Rotation;
            //    movementComponent.curRotTarget = newTargetRot;
            //    transform.Rotation = math.nlerp(transform.Rotation, newTargetRot, deltaTime / ROT_TIME);
            //    //if (movementComponent.rotTimePassed / ROT_TIME > 0.5)
            //    //Debug.Log(movementComponent.rotTimePassed / ROT_TIME);
            //    //Debug.Log(movementComponent.lastRotTarget);
            //    //Debug.Log(movementComponent.lastRotTarget.value - targetRot.value);
            //    //Debug.Log(targetRot);
            //    //movementComponent.lastRotTarget = targetRot;
            //}

            ////{
            ////    transform.Rotation = math.nlerp(transform.Rotation, newTargetRot, deltaTime / ROT_TIME);
            ////}


            transform.Position = closestHit.Position;
        }
        else
        {
            Debug.Log($"ERROR: Terrain was not found near unit!");
        }
    }
}



public partial struct RotationToTargetJob : IJobEntity
{
    public float deltaTime;

    public void Execute(ref RotationToTargetComponent rotation, ref LocalTransform localTransform)
    {
        if (UtilityFuncs.Nearly_Equals_quaternion(rotation.newRotTarget, rotation.curRotTarget))
        {
            if (rotation.rotTimeElapsed < rotation.rotTime)
            {
                rotation.rotTimeElapsed += deltaTime;
                localTransform.Rotation = math.nlerp(rotation.initialRotation, rotation.newRotTarget, rotation.rotTimeElapsed / rotation.rotTime);
            }
        }
        else
        {
            rotation.rotTimeElapsed = deltaTime;
            rotation.initialRotation = localTransform.Rotation;
            rotation.curRotTarget = rotation.newRotTarget;
            localTransform.Rotation = math.nlerp(localTransform.Rotation, rotation.newRotTarget, deltaTime / rotation.rotTime);
        }
    }
}



public partial struct AttackRotationToTargetJob : IJobEntity
{
    public float deltaTime;
    public ComponentLookup<LocalTransform> transformLookup;

    public void Execute(ref AttackRotationToTargetComponent rotation, in DynamicBuffer<AttackModelsBuffer> attackModelsBuf, in AttackCharsComponent attackChars, in LocalTransform localTransform)
    {
        #region Update target and handle automatic return to default rotation
        if (attackChars.target != Entity.Null)
        {
            rotation.newRotTarget = quaternion.LookRotationSafe(transformLookup[attackChars.target].Position - localTransform.Position, localTransform.Up());
            rotation.isInDefaultState = false;
            rotation.isRotatingToDefault = false;
            rotation.noRotTimeElapsed = 0;
        }
        else
        {
            if (rotation.isInDefaultState)
                return;
            if (!rotation.isRotatingToDefault)
            {
                rotation.noRotTimeElapsed += deltaTime;
                if (rotation.noRotTimeElapsed >= rotation.timeToReturnRot)
                    rotation.isRotatingToDefault = true;
            }
        }
        #endregion

        #region Usual rotation logic, but for attackModels
        if (UtilityFuncs.Nearly_Equals_quaternion(rotation.newRotTarget, rotation.curRotTarget))
        {
            if (rotation.rotTimeElapsed < rotation.rotTime)
            {
                rotation.rotTimeElapsed += deltaTime;
                quaternion resultRotation = math.nlerp(rotation.initialRotation, rotation.newRotTarget, deltaTime / rotation.rotTime);
                foreach (var model in attackModelsBuf)
                    transformLookup.GetRefRW(model).ValueRW.Rotation = resultRotation;
            }
            else if (rotation.isRotatingToDefault)
            {
                rotation.isRotatingToDefault = false;
                rotation.isInDefaultState = true;
            }
        }
        else
        {
            rotation.rotTimeElapsed = deltaTime;
            rotation.initialRotation = transformLookup[attackModelsBuf[0]].Rotation;
            rotation.curRotTarget = rotation.newRotTarget;
            quaternion resultRotation = math.nlerp(rotation.initialRotation, rotation.newRotTarget, deltaTime / rotation.rotTime);
            foreach (var model in attackModelsBuf)
                transformLookup.GetRefRW(model).ValueRW.Rotation = resultRotation;
        }
        #endregion
    }
}

