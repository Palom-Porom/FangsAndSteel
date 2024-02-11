using AnimCooker;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Apple.ReplayKit;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;

[UpdateInGroup(typeof(UnitsSystemGroup))]
[BurstCompile]
public partial struct TargetingAttackSystem : ISystem, ISystemStartStop
{
    ComponentLookup<HpComponent> hpLookup;
    ComponentLookup<LocalToWorld> localToWorldLookup;
    ComponentLookup<TeamComponent> teamLookup;
    ComponentLookup<FillFloatOverride> fillBarLookup;


    ///<value> All units which can reload </value>
    EntityQuery reloaders;
    EntityQuery potentialTargetsQuery;
    ///<value> All units which are looking for target </value>
    EntityQuery targetSearchersQuery;
    ///<value> All units who can create attack request </value>
    ///<remarks> Without deployable </remarks>
    EntityQuery usualAttackers;
    ///<value> All units who can create attack request </value>
    ///<remarks> With deployable </remarks>
    EntityQuery deployableAttackers;
    ///<value> All units which are in a pursuing mode </value>
    EntityQuery pursuiers;

    ComponentTypeHandle<ReloadComponent> reloadTypeHandle;
    ComponentTypeHandle<ReloadComponent> reloadTypeHandleRO;
    ComponentTypeHandle<UnitIconsComponent> unitIconsTypeHandleRO;
    ComponentTypeHandle<MovementComponent> movementTypeHandle;
    ComponentTypeHandle<RotationComponent> rotationTypeHandle;
    ComponentTypeHandle<AttackCharsComponent> attackCharsTypeHandleRO;
    ComponentTypeHandle<PursuingModeComponent> pursuingModeTypeHandle;
    ComponentTypeHandle<PursuingModeComponent> pursuingModeTypeHandleRO;
    ComponentTypeHandle<BattleModeComponent> battleModeTypeHandle;
    ComponentTypeHandle<BattleModeComponent> battleModeTypeHandleRO;
    ComponentTypeHandle<LocalTransform> transformTypeHandleRO;
    ComponentTypeHandle<Deployable> deployableTypeHandle;
    EntityTypeHandle entityTypeHandle;
    BufferTypeHandle<ModelsBuffer> modelsBuffTypeHandle;
    BufferTypeHandle<AttackModelsBuffer> attackModelsBuffTypeHandle;

    #region Animation vars
    ComponentLookup<AnimationCmdData> animCmdLookup;
    ComponentLookup<AnimationStateData> animStateLookup;
    NativeArray<AnimDbEntry> attackClips;
    NativeArray<AnimDbEntry> reloadClips;
    NativeArray<AnimDbEntry> moveClips;
    NativeArray<AnimDbEntry> deployClips;
    NativeArray<AnimDbEntry> undeployClips;
    NativeArray<AnimDbEntry> restClips;
    NativeArray<AnimDbEntry> rest_deployedClips;
    #endregion

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AttackCharsComponent>();
        state.RequireForUpdate<HpComponent>();

        hpLookup = state.GetComponentLookup<HpComponent>(true);
        localToWorldLookup = state.GetComponentLookup<LocalToWorld>(true);
        teamLookup = state.GetComponentLookup<TeamComponent>(true);
        fillBarLookup = state.GetComponentLookup<FillFloatOverride>();


        reloaders = new EntityQueryBuilder(Allocator.Temp).
            WithAllRW<ReloadComponent>().
            WithAll<UnitIconsComponent>().
            WithAny<ModelsBuffer, AttackModelsBuffer>().
            Build(ref state);
        potentialTargetsQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<HpComponent, LocalToWorld, TeamComponent>().Build(ref state);
        targetSearchersQuery = new EntityQueryBuilder(Allocator.Temp).
            WithAllRW<AttackCharsComponent>().
            WithPresentRW<BattleModeComponent>().
            WithDisabledRW<PursuingModeComponent>().
            WithAll<LocalTransform, TeamComponent>().
            Build(ref state);

        usualAttackers = new EntityQueryBuilder(Allocator.Temp).
            WithAllRW<MovementComponent, ReloadComponent>().
            WithAllRW<RotationComponent>().
            WithAll<AttackCharsComponent, PursuingModeComponent, BattleModeComponent, LocalTransform>().
            WithAny<ModelsBuffer, AttackModelsBuffer>().
            WithNone<Deployable>().
            WithOptions(EntityQueryOptions.IgnoreComponentEnabledState).
            Build(ref state);
        deployableAttackers = new EntityQueryBuilder(Allocator.Temp).
            WithAllRW<MovementComponent, ReloadComponent>().
            WithAllRW<RotationComponent, Deployable>().
            WithAll<AttackCharsComponent, PursuingModeComponent, /*BattleModeComponent,*/ LocalTransform>().
            WithAny<ModelsBuffer, AttackModelsBuffer>().
            WithOptions(EntityQueryOptions.IgnoreComponentEnabledState).
            Build(ref state);
        pursuiers = new EntityQueryBuilder(Allocator.Temp).
            WithAllRW<PursuingModeComponent, MovementComponent>().
            WithAllRW<BattleModeComponent>().
            WithAll<LocalTransform, ReloadComponent>().
            Build(ref state);

        reloadTypeHandle = SystemAPI.GetComponentTypeHandle<ReloadComponent>();
        reloadTypeHandleRO = SystemAPI.GetComponentTypeHandle<ReloadComponent>(true);
        unitIconsTypeHandleRO = SystemAPI.GetComponentTypeHandle<UnitIconsComponent>(true);
        movementTypeHandle = SystemAPI.GetComponentTypeHandle<MovementComponent>();
        rotationTypeHandle = SystemAPI.GetComponentTypeHandle<RotationComponent>();
        attackCharsTypeHandleRO = SystemAPI.GetComponentTypeHandle<AttackCharsComponent>(true);
        pursuingModeTypeHandle = SystemAPI.GetComponentTypeHandle<PursuingModeComponent>();
        pursuingModeTypeHandleRO = SystemAPI.GetComponentTypeHandle<PursuingModeComponent>(true);
        battleModeTypeHandle = SystemAPI.GetComponentTypeHandle<BattleModeComponent>();
        battleModeTypeHandleRO = SystemAPI.GetComponentTypeHandle<BattleModeComponent>(true);
        transformTypeHandleRO = SystemAPI.GetComponentTypeHandle<LocalTransform>(true);
        deployableTypeHandle = SystemAPI.GetComponentTypeHandle<Deployable>();
        entityTypeHandle = SystemAPI.GetEntityTypeHandle();
        modelsBuffTypeHandle = SystemAPI.GetBufferTypeHandle<ModelsBuffer>(true);
        attackModelsBuffTypeHandle = SystemAPI.GetBufferTypeHandle<AttackModelsBuffer>(true);
        

        #region Get Animation Lookups
        animCmdLookup = state.GetComponentLookup<AnimationCmdData>();
        animStateLookup = state.GetComponentLookup<AnimationStateData>();
        #endregion
    }

    public void OnStartRunning(ref SystemState state)
    {
        #region Animation Clips Arrays
        attackClips = SystemAPI.GetSingleton<AnimDbRefData>().FindClips("Attack");
        reloadClips = SystemAPI.GetSingleton<AnimDbRefData>().FindClips("Recharge");
        moveClips = SystemAPI.GetSingleton<AnimDbRefData>().FindClips("Movement");
        deployClips = SystemAPI.GetSingleton<AnimDbRefData>().FindClips("Deploy");
        undeployClips = SystemAPI.GetSingleton<AnimDbRefData>().FindClips("Undeploy");
        restClips = SystemAPI.GetSingleton<AnimDbRefData>().FindClips("Rest");
        rest_deployedClips = SystemAPI.GetSingleton<AnimDbRefData>().FindClips("Rest_Deployed");
        #endregion
    }

    public void OnStopRunning(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        #region Update data
        hpLookup.Update(ref state);
        localToWorldLookup.Update(ref state);
        teamLookup.Update(ref state);
        fillBarLookup.Update(ref state);
        animCmdLookup.Update(ref state);
        animStateLookup.Update(ref state);

        reloadTypeHandle.Update(ref state);
        reloadTypeHandleRO.Update(ref state);
        unitIconsTypeHandleRO.Update(ref state);
        movementTypeHandle.Update(ref state);
        rotationTypeHandle.Update(ref state);
        attackCharsTypeHandleRO.Update(ref state);
        pursuingModeTypeHandle.Update(ref state);
        pursuingModeTypeHandleRO.Update(ref state);
        battleModeTypeHandle.Update(ref state);
        battleModeTypeHandleRO.Update(ref state);
        transformTypeHandleRO.Update(ref state);
        deployableTypeHandle.Update(ref state);
        entityTypeHandle.Update(ref state);
        modelsBuffTypeHandle.Update(ref state);
        attackModelsBuffTypeHandle.Update(ref state);

        NativeArray<Entity> potentialTargetsArr = potentialTargetsQuery.ToEntityArray(Allocator.TempJob);

        var ecb =  SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

        float deltaTime = SystemAPI.Time.DeltaTime;
        #endregion



        //JobHandle reloadJobHandle = new ReloadJob
        //{
        //    deltaTime = deltaTime,
        //    fillBarLookup = fillBarLookup
        //}.Schedule(state.Dependency);

        JobHandle reloadJobHandle = new _ReloadJob
        {
            deltaTime = deltaTime,
            fillBarLookup = fillBarLookup,

            animCmdLookup = animCmdLookup,
            animStateLookup = animStateLookup,
            reloadClips = reloadClips,

            reloadTypeHandle = reloadTypeHandle,
            unitsIconsTypeHandleRO = unitIconsTypeHandleRO,
            modelsBuffsTypeHandle = modelsBuffTypeHandle,
            attackModelsBuffsTypeHandle = attackModelsBuffTypeHandle
        }.Schedule(reloaders, state.Dependency);

        JobHandle deployJobHandle = new UpdateDeployJob
        {
            deltaTime = deltaTime,

            animCmdLookup = animCmdLookup,
            animStateLookup = animStateLookup,
            moveClips = moveClips,
            restClips = restClips
        }.Schedule(reloadJobHandle);

        JobHandle targetSearchJobHandle = new AttackTargetSearchJob
        {
            hpLookup = hpLookup,
            localToWorldLookup = localToWorldLookup,
            potentialTargetsArr = potentialTargetsArr,
            teamLookup = teamLookup
        }.Schedule(targetSearchersQuery, state.Dependency);

        //JobHandle usualCreateAttackRequestsJobHandle = new CreateUsualAttackRequestsJob
        //{
        //    localToWorldLookup = localToWorldLookup,
        //    ecb = ecb,
            
        //    animCmdLookup = animCmdLookup,
        //    animStateLookup = animStateLookup,
        //    attackClips = attackClips,
        //    reloadClips = reloadClips,
        //    moveClips = moveClips,
        //    restClips = restClips
        //}.Schedule(JobHandle.CombineDependencies(deployJobHandle, targetSearchJobHandle));

        JobHandle usualCreateAttackRequestsJobHandle = new _CreateUsualAttackRequestsJob
        {
            localToWorldLookup = localToWorldLookup,
            ecb = ecb,

            animCmdLookup = animCmdLookup,
            animStateLookup = animStateLookup,
            attackClips = attackClips,
            reloadClips = reloadClips,
            moveClips = moveClips,
            restClips = restClips,

            attackCharsTypeHandleRO = attackCharsTypeHandleRO,
            battleModeSetsTypeHandleRO = battleModeTypeHandleRO,
            pursuingModeSettsTypeHandleRO = pursuingModeTypeHandleRO,
            movementTypeHandle = movementTypeHandle,
            reloadTypeHandle = reloadTypeHandle,
            transformTypeHandleRO = transformTypeHandleRO,
            rotationTypeHandle = rotationTypeHandle,
            entityTypeHandle = entityTypeHandle,
            modelsBuffTypeHandle = modelsBuffTypeHandle,
            attackModelsBuffTypeHandle = attackModelsBuffTypeHandle

        }.Schedule(usualAttackers, JobHandle.CombineDependencies(deployJobHandle, targetSearchJobHandle));

        //state.Dependency = new CreateDeployableAttackRequestsJob
        //{
        //    localToWorldLookup = localToWorldLookup,
        //    ecb = ecb,
        //    deltaTime = deltaTime,

        //    animCmdLookup = animCmdLookup,
        //    animStateLookup = animStateLookup,
        //    attackClips = attackClips,
        //    reloadClips = reloadClips,
        //    deployClips = deployClips,
        //    undeployClips = undeployClips,
        //    rest_deployedClips = rest_deployedClips
        //}.Schedule(usualCreateAttackRequestsJobHandle);

        JobHandle deployableCreateAttackRequestsJobHandle = new _CreateDeployableAttackRequestsJob
        {
            localToWorldLookup = localToWorldLookup,
            ecb = ecb,
            deltaTime = deltaTime,

            animCmdLookup = animCmdLookup,
            animStateLookup = animStateLookup,
            attackClips = attackClips,
            reloadClips = reloadClips,
            deployClips = deployClips,
            undeployClips = undeployClips,
            rest_deployedClips = rest_deployedClips,

            attackCharsTypeHandleRO = attackCharsTypeHandleRO,
            pursuingModeSettsTypeHandleRO = pursuingModeTypeHandleRO,
            deployableTypeHandle = deployableTypeHandle,
            movementTypeHandle = movementTypeHandle,
            reloadTypeHandle = reloadTypeHandle,
            rotationTypeHandle = rotationTypeHandle,
            transformTypeHandleRO = transformTypeHandleRO,
            modelsBuffTypeHandle = modelsBuffTypeHandle,
            attackModelsBuffTypeHandle = attackModelsBuffTypeHandle
        }.Schedule(deployableAttackers, usualCreateAttackRequestsJobHandle);

        state.Dependency = new PursuingJob
        {
            deltaTime = deltaTime,
            localToWorldLookup = localToWorldLookup,

            pursuingModeSettsTypeHandle = pursuingModeTypeHandle,
            battleModeSetsTypeHandle = battleModeTypeHandle,
            reloadTypeHandleRO = reloadTypeHandleRO,
            movementTypeHandle = movementTypeHandle,
            transformTypeHandleRO = transformTypeHandleRO
        }.Schedule(pursuiers, deployableCreateAttackRequestsJobHandle);

        potentialTargetsArr.Dispose(targetSearchJobHandle);
    }
}




///<summary> Update all units' reloadTime values and reload bars </summary>
[BurstCompile]
public partial struct ReloadJob : IJobEntity
{
    public float deltaTime;

    public ComponentLookup<FillFloatOverride> fillBarLookup;

    public void Execute(ref ReloadComponent reloadComponent, in UnitIconsComponent unitsIcons)
    {

        if (reloadComponent.curBullets == 0) // if drum is empty -> drum reload
        {
            reloadComponent.drumReloadElapsed += deltaTime;
            if (reloadComponent.drumReloadElapsed >= reloadComponent.drumReloadLen * (1 - reloadComponent.curDebaff))
            {
                reloadComponent.curBullets = reloadComponent.maxBullets;
                reloadComponent.bulletReloadElapsed = reloadComponent.bulletReloadLen;
                reloadComponent.drumReloadElapsed = 0f;
                fillBarLookup.GetRefRW(unitsIcons.reloadBarEntity).ValueRW.Value = 1f;
                return;
            }
            fillBarLookup.GetRefRW(unitsIcons.reloadBarEntity).ValueRW.Value = reloadComponent.drumReloadElapsed / reloadComponent.drumReloadLen;

        }

        else if (reloadComponent.bulletReloadElapsed < reloadComponent.bulletReloadLen) // reload of the bullet
        {
            if (reloadComponent.bulletReloadElapsed <= float.Epsilon) //If just shot -> update the ReloadBar
                fillBarLookup.GetRefRW(unitsIcons.reloadBarEntity).ValueRW.Value = (float)reloadComponent.curBullets / reloadComponent.maxBullets;
            reloadComponent.bulletReloadElapsed += deltaTime;
        }

    }
}


///<summary> Update all units' reloadTime values and reload bars </summary>
public partial struct _ReloadJob : IJobChunk
{
    public float deltaTime;

    public ComponentLookup<FillFloatOverride> fillBarLookup;

    public ComponentLookup<AnimationCmdData> animCmdLookup;
    [ReadOnly] public ComponentLookup<AnimationStateData> animStateLookup;
    [ReadOnly] public NativeArray<AnimDbEntry> reloadClips;

    public ComponentTypeHandle<ReloadComponent> reloadTypeHandle;
    [ReadOnly] public ComponentTypeHandle<UnitIconsComponent> unitsIconsTypeHandleRO;
    //ComponentTypeHandle<Deployable> deployableTypeHandle;
    [ReadOnly] public BufferTypeHandle<ModelsBuffer> modelsBuffsTypeHandle;
    [ReadOnly] public BufferTypeHandle<AttackModelsBuffer> attackModelsBuffsTypeHandle;

    public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
    {
        unsafe
        {
            ReloadComponent* reloads = chunk.GetComponentDataPtrRW(ref reloadTypeHandle);
            UnitIconsComponent* unitIcons = chunk.GetComponentDataPtrRO(ref unitsIconsTypeHandleRO);
            BufferAccessor<ModelsBuffer> modelsBuffs = chunk.GetBufferAccessor(ref modelsBuffsTypeHandle);

            BufferAccessor<AttackModelsBuffer> attackModelsBuffs = new BufferAccessor<AttackModelsBuffer>();
            bool hasAttackModels = chunk.Has(ref attackModelsBuffsTypeHandle);
            if (hasAttackModels)
                attackModelsBuffs = chunk.GetBufferAccessor(ref attackModelsBuffsTypeHandle);

            Assert.IsFalse(useEnabledMask);

            for (int i = 0; i < chunk.Count; i++)
            {
                if (reloads[i].curBullets == 0) // if drum is empty -> drum reload
                {
                    if (reloads[i].shootAnimElapsed < reloads[i].shootAnimLen)
                    {
                        fillBarLookup.GetRefRW(unitIcons[i].reloadBarEntity).ValueRW.Value = 0f;
                        reloads[i].shootAnimElapsed += deltaTime;
                        continue;
                    }

                    if (reloads[i].drumReloadElapsed == 0f)//if just started reloading -> reload anim
                    {
                        if (!hasAttackModels)
                        {
                            foreach (var modelBufElem in modelsBuffs[i])
                            {
                                RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
                                animCmd.ValueRW.ClipIndex = reloadClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                                animCmd.ValueRW.Cmd = AnimationCmd.PlayOnce;
                            }
                        }
                        else
                        {
                            foreach (var modelBufElem in attackModelsBuffs[i])
                            {
                                RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
                                animCmd.ValueRW.ClipIndex = reloadClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                                animCmd.ValueRW.Cmd = AnimationCmd.PlayOnce;
                            }
                        }
                    }

                    reloads[i].drumReloadElapsed += deltaTime;
                    if (reloads[i].drumReloadElapsed > reloads[i].drumReloadLen * (1 - reloads[i].curDebaff))
                    {
                        reloads[i].curBullets = reloads[i].maxBullets;
                        reloads[i].bulletReloadElapsed = reloads[i].bulletReloadLen;
                        reloads[i].drumReloadElapsed = 0f;
                        fillBarLookup.GetRefRW(unitIcons[i].reloadBarEntity).ValueRW.Value = 1f;
                        reloads[i].shootAnimElapsed = 0f;
                        continue;
                    }
                    fillBarLookup.GetRefRW(unitIcons[i].reloadBarEntity).ValueRW.Value = reloads[i].drumReloadElapsed / reloads[i].drumReloadLen;

                }

                else if (reloads[i].bulletReloadElapsed <= reloads[i].bulletReloadLen) // reload of the bullet
                {
                    if (reloads[i].bulletReloadElapsed <= float.Epsilon) //If just shot -> update the ReloadBar
                        fillBarLookup.GetRefRW(unitIcons[i].reloadBarEntity).ValueRW.Value = (float)reloads[i].curBullets / reloads[i].maxBullets;
                    reloads[i].bulletReloadElapsed += deltaTime;
                }
            }
        }
    }
}


///<summary> Update deploying/undeploying values of units </summary>
[BurstCompile]
public partial struct UpdateDeployJob : IJobEntity
{
    public float deltaTime;

    public ComponentLookup<AnimationCmdData> animCmdLookup;
    [ReadOnly] public ComponentLookup<AnimationStateData> animStateLookup;
    [ReadOnly] public NativeArray<AnimDbEntry> moveClips;
    [ReadOnly] public NativeArray<AnimDbEntry> restClips;

    public void Execute(ref Deployable deployable, ref MovementComponent movementComponent, in DynamicBuffer<ModelsBuffer> modelsBuf, in ReloadComponent reload)
    {
        //Undeploying
        if (!deployable.deployedState)
        {
            if (!movementComponent.isAbleToMove)
            {
                if (!reload.isReloaded()) //not undeploy until full reloaded
                    return;

                if (deployable.deployTimeElapsed <= 0)
                {
                    movementComponent.isAbleToMove = true;

                    if (movementComponent.hasMoveTarget) //Set move animation PlayForever
                    {
                        foreach (var modelBufElem in modelsBuf)
                        {
                            RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
                            animCmd.ValueRW.ClipIndex = moveClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                            animCmd.ValueRW.Cmd = AnimationCmd.SetPlayForever;
                        }
                    }
                    else //Set rest animation PlayForever
                    {
                        foreach (var modelBufElem in modelsBuf)
                        {
                            RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
                            animCmd.ValueRW.ClipIndex = restClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                            animCmd.ValueRW.Cmd = AnimationCmd.SetPlayForever;
                        }
                    }
                }
                else
                    deployable.deployTimeElapsed -= deltaTime;
            }
        }
        //Deploying
        else if (deployable.deployTimeElapsed < deployable.deployTime)
        {
            deployable.deployTimeElapsed += deltaTime;
        }
    }
}


///<summary> Searching for the most valuable target at the moment for ALL units </summary>
//[BurstCompile]
//[WithPresent(typeof(BattleModeComponent))]
//[WithDisabled(typeof(PursuingModeComponent))]
public partial struct AttackTargetSearchJob : IJobEntity
{
    /// <summary> In other words all units that can be attacked </summary>
    [ReadOnly] public NativeArray<Entity> potentialTargetsArr;

    [ReadOnly] public ComponentLookup<LocalToWorld> localToWorldLookup;
    [ReadOnly] public ComponentLookup<TeamComponent> teamLookup;
    [ReadOnly] public ComponentLookup<HpComponent> hpLookup;

    public void Execute(ref AttackCharsComponent attackChars, ref BattleModeComponent modeSettings, ref PursuingModeComponent pursuingModeComponent, in LocalTransform localTransform, in TeamComponent team,
        EnabledRefRW<PursuingModeComponent> pursuingModeEnabledRefRW, EnabledRefRW<BattleModeComponent> battleModeEnabledRefRW)
    {
        float bestScore = float.MinValue;
        Entity bestScoreEntity = Entity.Null;

        foreach (Entity potentialTarget in potentialTargetsArr)
        {
            //Check if they are in different teams
            if (teamLookup[potentialTarget].teamInd - team.teamInd == 0)
                continue;

            float curScore = 0;

            float distanceToPotTargetSq = math.distancesq(localTransform.Position, localToWorldLookup[potentialTarget].Position);

            //auto-trigger check
            if ((modeSettings.autoTriggerMoving /*|| (modeSettings.autoTriggerStatic && (!movement.hasMoveTarget || !movement.isAbleToMove))*/) // <-- suppose autoTriggerStatic is not so useful option for player
                &&
                distanceToPotTargetSq < modeSettings.autoTriggerRadiusSq
                &&
                (hpLookup[potentialTarget].curHp / hpLookup[potentialTarget].maxHp) <= modeSettings.autoTriggerMaxHpPercent
                /*&& *check the type of unit* */)
            {
                curScore += 10000; // such targets has higher priority than other (auto-trigger has more priority than usual attack)
                battleModeEnabledRefRW.ValueRW = false;
                pursuingModeEnabledRefRW.ValueRW = true;
                pursuingModeComponent.Target = attackChars.target;
            }
            else if (distanceToPotTargetSq > attackChars.radiusSq)
                continue; // if pot target is not in any of the radiuses - going to next potential target

            curScore -= distanceToPotTargetSq;
            ///TODO Other score affectors

            if (curScore > bestScore)
            {
                bestScore = curScore;
                bestScoreEntity = potentialTarget;
            }
        }

        attackChars.target = bestScoreEntity;
    }
}


/// <summary> Creates attack requests if needed and do connected to this things (animation, etc.) </summary>
/// <remarks> That Job is for NON Deployable units! </remarks>
[WithNone(typeof(Deployable))]
[BurstCompile]
public partial struct CreateUsualAttackRequestsJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<LocalToWorld> localToWorldLookup;

    public EntityCommandBuffer.ParallelWriter ecb;

    [NativeDisableContainerSafetyRestriction]
    public ComponentLookup<AnimationCmdData> animCmdLookup;
    [NativeDisableContainerSafetyRestriction]
    public ComponentLookup<AnimationStateData> animStateLookup;
    [ReadOnly] public NativeArray<AnimDbEntry> attackClips;
    [ReadOnly] public NativeArray<AnimDbEntry> reloadClips;
    [ReadOnly] public NativeArray<AnimDbEntry> moveClips;
    [ReadOnly] public NativeArray<AnimDbEntry> restClips;

    public void Execute(in AttackCharsComponent attackChars, in BattleModeComponent modeSettings, ref MovementComponent movementComponent, Entity entity,
        in DynamicBuffer<ModelsBuffer> modelsBuf, in LocalTransform localTransform, ref ReloadComponent reloadComponent, [ChunkIndexInQuery] int chunkIndexInQuery,
        ref RotationComponent rotation)
    {
        //if has some target -> shoot
        if (attackChars.target != Entity.Null) 
        {
            if (!(reloadComponent.curBullets > 0 && reloadComponent.bulletReloadElapsed >= reloadComponent.bulletReloadLen)) // if not reloaded -> return
                return;
            if (modeSettings.shootingOnMove)
                ecb.AddComponent(chunkIndexInQuery, entity, new NotAbleToMoveForTimeRqstComponent { passedTime = 0, targetTime = attackChars.timeToShoot });
            movementComponent.isAbleToMove = false;
            //Rotate to target
            rotation.newRotTarget = quaternion.LookRotationSafe(localToWorldLookup[attackChars.target].Position - localTransform.Position, localTransform.Up());

            //Create Attack Request
            Entity attackRequest = ecb.CreateEntity(chunkIndexInQuery);
            ecb.AddComponent(chunkIndexInQuery, attackRequest, new AttackRequestComponent 
            { 
                target = attackChars.target, 
                damage = attackChars.damage, 
                attackerPos = localTransform.Position
            });

            //Play Attack Anim
            foreach (var modelBufElem in modelsBuf)
            {
                RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
                byte reloadIdx = reloadClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                animStateLookup.GetRefRW(modelBufElem.model).ValueRW.ForeverClipIndex = reloadIdx;
                animCmd.ValueRW.ClipIndex = attackClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                animCmd.ValueRW.Cmd = AnimationCmd.PlayOnce;
            }
            
            reloadComponent.curBullets--;
            reloadComponent.bulletReloadElapsed = 0;
        }
        else // this means unit doesn't have a target to shoot
        {
            if (!movementComponent.isAbleToMove && // if not able to move
                reloadComponent.isReloaded()) // and reloaded
            {// enable to move and adjust the anims
                if (movementComponent.hasMoveTarget)
                    foreach (var modelBufElem in modelsBuf)
                    {
                        RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
                        animCmd.ValueRW.ClipIndex = moveClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                        animCmd.ValueRW.Cmd = AnimationCmd.SetPlayForever;
                    }
                else
                    foreach (var modelBufElem in modelsBuf)
                    {
                        RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
                        animCmd.ValueRW.ClipIndex = restClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                        animCmd.ValueRW.Cmd = AnimationCmd.SetPlayForever;
                    }
                movementComponent.isAbleToMove = true;
            }
        }
    }
}


/// <summary> Creates attack requests if needed and do connected to this things (animation, etc.) </summary>
/// <remarks> That Job is for NON Deployable units! </remarks>
public partial struct _CreateUsualAttackRequestsJob : IJobChunk
{
    [ReadOnly] public ComponentLookup<LocalToWorld> localToWorldLookup;

    public EntityCommandBuffer.ParallelWriter ecb;

    [NativeDisableContainerSafetyRestriction]
    public ComponentLookup<AnimationCmdData> animCmdLookup;
    [NativeDisableContainerSafetyRestriction]
    public ComponentLookup<AnimationStateData> animStateLookup;
    [ReadOnly] public NativeArray<AnimDbEntry> attackClips;
    [ReadOnly] public NativeArray<AnimDbEntry> reloadClips;
    [ReadOnly] public NativeArray<AnimDbEntry> moveClips;
    [ReadOnly] public NativeArray<AnimDbEntry> restClips;

    [ReadOnly] public ComponentTypeHandle<AttackCharsComponent> attackCharsTypeHandleRO;
    [ReadOnly] public ComponentTypeHandle<PursuingModeComponent> pursuingModeSettsTypeHandleRO;
    [ReadOnly] public ComponentTypeHandle<BattleModeComponent> battleModeSetsTypeHandleRO;
    public ComponentTypeHandle<MovementComponent> movementTypeHandle;
    [ReadOnly] public ComponentTypeHandle<LocalTransform> transformTypeHandleRO;
    public ComponentTypeHandle<ReloadComponent> reloadTypeHandle;
    public ComponentTypeHandle<RotationComponent> rotationTypeHandle;
    public EntityTypeHandle entityTypeHandle;
    [ReadOnly] public BufferTypeHandle<ModelsBuffer> modelsBuffTypeHandle;
    [ReadOnly] public BufferTypeHandle<AttackModelsBuffer> attackModelsBuffTypeHandle;


    public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
    {
        unsafe
        {
            AttackCharsComponent* attackChars = chunk.GetComponentDataPtrRO(ref attackCharsTypeHandleRO);
            PursuingModeComponent* pursuingModes = chunk.GetComponentDataPtrRO(ref pursuingModeSettsTypeHandleRO);
            BattleModeComponent* battleModeSetts = chunk.GetComponentDataPtrRO(ref battleModeSetsTypeHandleRO);
            MovementComponent* movements = chunk.GetComponentDataPtrRW(ref movementTypeHandle);
            LocalTransform* transforms = chunk.GetComponentDataPtrRO(ref transformTypeHandleRO);
            ReloadComponent* reloads = chunk.GetComponentDataPtrRW(ref reloadTypeHandle);
            RotationComponent* rotations = chunk.GetComponentDataPtrRW(ref rotationTypeHandle);
            BufferAccessor<ModelsBuffer> modelsBufs = chunk.GetBufferAccessor(ref modelsBuffTypeHandle);
            Entity* entities = chunk.GetEntityDataPtrRO(entityTypeHandle);

            BufferAccessor<AttackModelsBuffer> attackModelsBufs = new BufferAccessor<AttackModelsBuffer>();
            bool hasSeparateAttackModels = chunk.Has(ref attackModelsBuffTypeHandle);
            if (hasSeparateAttackModels)
                attackModelsBufs = chunk.GetBufferAccessor(ref attackModelsBuffTypeHandle);

            

            for (int i = 0; i < chunk.Count; i++)
            {
                //if has some target -> shoot
                if (attackChars[i].target != Entity.Null)
                {
                    if (!reloads[i].isReloaded()) // if not reloaded -> return
                        continue;

                    bool isPursuingEnabled = chunk.IsComponentEnabled(ref pursuingModeSettsTypeHandleRO, i);
                    if (isPursuingEnabled &&
                        pursuingModes[i].maxShootDistanceSq > math.distancesq(localToWorldLookup[pursuingModes[i].Target].Position, transforms[i].Position))
                    {//if pursuing and not close to target enough -> not shoot
                        continue;
                    }

                    if (battleModeSetts[i].shootingOnMove || hasSeparateAttackModels || isPursuingEnabled) //if can move while reload -> temp component added
                        ecb.AddComponent(unfilteredChunkIndex, entities[i], new NotAbleToMoveForTimeRqstComponent 
                        { 
                            passedTime = 0, 
                            targetTime = attackChars[i].timeToShoot 
                        });

                    if (!hasSeparateAttackModels)//if no turret -> stop and turn to the enemy
                    {
                        movements[i].isAbleToMove = false;
                        //Rotate to target
                        rotations[i].newRotTarget =
                            quaternion.LookRotationSafe(localToWorldLookup[attackChars[i].target].Position - transforms[i].Position, transforms[i].Up());
                    }

                    //Create Attack Request
                    Entity attackRequest = ecb.CreateEntity(unfilteredChunkIndex);
                    ecb.AddComponent(unfilteredChunkIndex, attackRequest, new AttackRequestComponent
                    {
                        target = attackChars[i].target,
                        damage = attackChars[i].damage,
                        attackerPos = transforms[i].Position
                    });

                    //Play Attack Anim
                    if (!hasSeparateAttackModels) //if has no turret -> anim whole body
                    {
                        foreach (var modelBufElem in modelsBufs[i])
                        {
                            RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
                            byte restIdx = restClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                            animStateLookup.GetRefRW(modelBufElem.model).ValueRW.ForeverClipIndex = restIdx;
                            animCmd.ValueRW.ClipIndex = attackClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                            animCmd.ValueRW.Cmd = AnimationCmd.PlayOnce;
                        }
                    }
                    else //if has a turret -> anim only turret
                    {
                        foreach (var modelBufElem in attackModelsBufs[i])
                        {
                            RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
                            //byte reloadIdx = reloadClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                            //animStateLookup.GetRefRW(modelBufElem.model).ValueRW.ForeverClipIndex = reloadIdx;
                            animCmd.ValueRW.ClipIndex = attackClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                            animCmd.ValueRW.Cmd = AnimationCmd.PlayOnce;
                        }
                    }

                    reloads[i].curBullets--;
                    reloads[i].bulletReloadElapsed = 0;

                    if (isPursuingEnabled)
                        pursuingModes[i].dropTimeElapsed = 0;
                }
                else // this means unit doesn't have a target to shoot
                {
                    if (!movements[i].isAbleToMove && // if not able to move
                        reloads[i].isReloaded()) // and reloaded
                    {// enable to move and adjust the anims
                        if (movements[i].hasMoveTarget)
                            foreach (var modelBufElem in modelsBufs[i])
                            {
                                RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
                                animCmd.ValueRW.ClipIndex = moveClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                                animCmd.ValueRW.Cmd = AnimationCmd.SetPlayForever;
                            }
                        else
                            foreach (var modelBufElem in modelsBufs[i])
                            {
                                RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
                                animCmd.ValueRW.ClipIndex = restClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                                animCmd.ValueRW.Cmd = AnimationCmd.SetPlayForever;
                            }
                        movements[i].isAbleToMove = true;
                    }
                }
            }
        }
    }
}


///<summary> Creates attack requests if needed and do connected to this things (animation, etc.) </summary>
/// <remarks> That Job is for Deployable units! </remarks>
[BurstCompile]
public partial struct CreateDeployableAttackRequestsJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<LocalToWorld> localToWorldLookup;

    public EntityCommandBuffer.ParallelWriter ecb;

    public float deltaTime;

    [NativeDisableContainerSafetyRestriction]
    public ComponentLookup<AnimationCmdData> animCmdLookup;
    [NativeDisableContainerSafetyRestriction]
    public ComponentLookup<AnimationStateData> animStateLookup;
    [ReadOnly] public NativeArray<AnimDbEntry> attackClips;
    [ReadOnly] public NativeArray<AnimDbEntry> reloadClips;
    [ReadOnly] public NativeArray<AnimDbEntry> deployClips;
    [ReadOnly] public NativeArray<AnimDbEntry> undeployClips;
    [ReadOnly] public NativeArray<AnimDbEntry> rest_deployedClips;

    public void Execute(ref Deployable deployable, in AttackCharsComponent attackChars, ref MovementComponent movementComponent, in DynamicBuffer<ModelsBuffer> modelsBuf,
        ref ReloadComponent reloadComponent, [ChunkIndexInQuery] int chunkIndexInQuery, in LocalTransform localTransform, ref RotationComponent rotation)
    {
        if (attackChars.target != Entity.Null)
        {
            deployable.waitingTimeCur = 0; // remove to other place

            //if Undeployed, than start Deploying
            if (!deployable.deployedState)
            {
                deployable.deployedState = true;
                movementComponent.isAbleToMove = false;

                //Set Deploy anim PlayOnce
                foreach (var modelBufElem in modelsBuf)
                {
                    RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
                    //animCmd.ValueRW.ClipIndex = rest_deployedClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                    //animCmd.ValueRW.Cmd = AnimationCmd.SetPlayForever;
                    //byte rest_deployed_idx = rest_deployedClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                    //animStateLookup.GetRefRW(modelBufElem.model).ValueRW.ForeverClipIndex = rest_deployed_idx;
                    animCmd.ValueRW.ClipIndex = deployClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                    animCmd.ValueRW.Cmd = AnimationCmd.PlayOnce;
                }
            }
            //If fully Deployed and Reloaded -> create Attack Rqst
            else if (deployable.deployTimeElapsed >= deployable.deployTime && reloadComponent.curBullets > 0 && reloadComponent.bulletReloadElapsed >= reloadComponent.bulletReloadLen)
            {
                //Rotate to target
                rotation.newRotTarget = quaternion.LookRotationSafe(localToWorldLookup[attackChars.target].Position - localTransform.Position, localTransform.Up());

                //Create Attack Request
                Entity attackRequest = ecb.CreateEntity(chunkIndexInQuery);
                ecb.AddComponent(chunkIndexInQuery, attackRequest, new AttackRequestComponent 
                { 
                    target = attackChars.target, 
                    damage = attackChars.damage, 
                    attackerPos =  localTransform.Position
                });

                //Play Attack Anim
                foreach (var modelBufElem in modelsBuf)
                {
                    RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
                    ///TODO: Reload when the anim on model appear
                    animCmd.ValueRW.ClipIndex = attackClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                    animCmd.ValueRW.Cmd = AnimationCmd.PlayOnce;
                }

                reloadComponent.curBullets--;
                reloadComponent.bulletReloadElapsed = 0;
            }
        }
        //If has no target and another point to Move -> undeploy and move after waitingTime elapsed
        else if (movementComponent.hasMoveTarget) 
        {
            //Update waiting time
            if (deployable.waitingTimeCur < deployable.waitingTimeMax)
                deployable.waitingTimeCur += deltaTime;
            //if waiting time elapsed -> undeploy and move
            if (deployable.deployedState && deployable.waitingTimeCur >= deployable.waitingTimeMax && // if deployed and waited enough time
                reloadComponent.isReloaded()) // and reloaded
            {// then undelpoy
                deployable.deployedState = false;

                //Set Undeploy anim PlayOnce
                foreach (var modelBufElem in modelsBuf)
                {
                    RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
                    animCmd.ValueRW.ClipIndex = undeployClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                    animCmd.ValueRW.Cmd = AnimationCmd.PlayOnce;
                }
            }
        }
    }
}


///<summary> Creates attack requests if needed and do connected to this things (animation, etc.) </summary>
/// <remarks> That Job is for Deployable units! </remarks>
public partial struct _CreateDeployableAttackRequestsJob : IJobChunk
{
    [ReadOnly] public ComponentLookup<LocalToWorld> localToWorldLookup;

    public EntityCommandBuffer.ParallelWriter ecb;

    public float deltaTime;

    [NativeDisableContainerSafetyRestriction]
    public ComponentLookup<AnimationCmdData> animCmdLookup;
    [NativeDisableContainerSafetyRestriction]
    public ComponentLookup<AnimationStateData> animStateLookup;
    [ReadOnly] public NativeArray<AnimDbEntry> attackClips;
    [ReadOnly] public NativeArray<AnimDbEntry> reloadClips;
    [ReadOnly] public NativeArray<AnimDbEntry> deployClips;
    [ReadOnly] public NativeArray<AnimDbEntry> undeployClips;
    [ReadOnly] public NativeArray<AnimDbEntry> rest_deployedClips;

    public ComponentTypeHandle<Deployable> deployableTypeHandle;
    [ReadOnly] public ComponentTypeHandle<AttackCharsComponent> attackCharsTypeHandleRO;
    [ReadOnly] public ComponentTypeHandle<PursuingModeComponent> pursuingModeSettsTypeHandleRO;
    public ComponentTypeHandle<MovementComponent> movementTypeHandle;
    [ReadOnly] public ComponentTypeHandle<LocalTransform> transformTypeHandleRO;
    public ComponentTypeHandle<ReloadComponent> reloadTypeHandle;
    public ComponentTypeHandle<RotationComponent> rotationTypeHandle;
    [ReadOnly] public BufferTypeHandle<ModelsBuffer> modelsBuffTypeHandle;
    [ReadOnly] public BufferTypeHandle<AttackModelsBuffer> attackModelsBuffTypeHandle;


    public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
    {
        unsafe
        {
            Deployable* deployables = chunk.GetComponentDataPtrRW(ref deployableTypeHandle);
            AttackCharsComponent* attackChars = chunk.GetComponentDataPtrRO(ref attackCharsTypeHandleRO);
            PursuingModeComponent* pursuingModes = chunk.GetComponentDataPtrRO(ref pursuingModeSettsTypeHandleRO);
            MovementComponent* movements = chunk.GetComponentDataPtrRW(ref movementTypeHandle);
            LocalTransform* transforms = chunk.GetComponentDataPtrRO(ref transformTypeHandleRO);
            ReloadComponent* reloads = chunk.GetComponentDataPtrRW(ref reloadTypeHandle);
            RotationComponent* rotations = chunk.GetComponentDataPtrRW(ref rotationTypeHandle);
            BufferAccessor<ModelsBuffer> modelsBufs = chunk.GetBufferAccessor(ref modelsBuffTypeHandle);

            BufferAccessor<AttackModelsBuffer> attackModelsBufs = new BufferAccessor<AttackModelsBuffer>();
            bool hasSeparateAttackModels = chunk.Has(ref attackModelsBuffTypeHandle);
            if (hasSeparateAttackModels)
                attackModelsBufs = chunk.GetBufferAccessor(ref attackModelsBuffTypeHandle);



            for (int i = 0; i < chunk.Count; i++)
            {
                if (attackChars[i].target != Entity.Null)
                {
                    deployables[i].waitingTimeCur = 0; // remove to other place (is it possible to null this value in some if - not every frame when unit has a target?)

                    bool isPursuingEnabled = chunk.IsComponentEnabled(ref pursuingModeSettsTypeHandleRO, i);
                    if (isPursuingEnabled &&
                        pursuingModes[i].maxShootDistanceSq > math.distancesq(localToWorldLookup[pursuingModes[i].Target].Position, transforms[i].Position))
                    {//if pursuing and not close to target enough -> not shoot
                        continue;
                    }

                    //if Undeployed, than start Deploying
                    if (!deployables[i].deployedState)
                    {
                        deployables[i].deployedState = true;
                        movements[i].isAbleToMove = false;

                        //Set Deploy anim PlayOnce
                        foreach (var modelBufElem in modelsBufs[i])
                        {
                            RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
                            byte rest_deployed_idx = rest_deployedClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                            animStateLookup.GetRefRW(modelBufElem.model).ValueRW.ForeverClipIndex = rest_deployed_idx;
                            animCmd.ValueRW.ClipIndex = deployClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                            animCmd.ValueRW.Cmd = AnimationCmd.PlayOnce;
                        }
                    }
                    //If fully Deployed and Reloaded -> create Attack Rqst
                    else if (deployables[i].deployTimeElapsed >= deployables[i].deployTime && reloads[i].isReloaded())
                    {
                        if (!hasSeparateAttackModels)
                            //Rotate to target
                            rotations[i].newRotTarget = quaternion.LookRotationSafe(localToWorldLookup[attackChars[i].target].Position - transforms[i].Position, transforms[i].Up());

                        //Create Attack Request
                        Entity attackRequest = ecb.CreateEntity(unfilteredChunkIndex);
                        ecb.AddComponent(unfilteredChunkIndex, attackRequest, new AttackRequestComponent
                        {
                            target = attackChars[i].target,
                            damage = attackChars[i].damage,
                            attackerPos = transforms[i].Position
                        });

                        //Play Attack Anim
                        if (!hasSeparateAttackModels)
                        {
                            foreach (var modelBufElem in modelsBufs[i])
                            {
                                RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
                                animCmd.ValueRW.ClipIndex = attackClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                                animCmd.ValueRW.Cmd = AnimationCmd.PlayOnce;
                            }
                        }
                        else
                        {
                            foreach (var modelBufElem in attackModelsBufs[i])
                            {
                                RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
                                animCmd.ValueRW.ClipIndex = attackClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                                animCmd.ValueRW.Cmd = AnimationCmd.PlayOnce;
                            }
                        }

                        reloads[i].curBullets--;
                        reloads[i].bulletReloadElapsed = 0;

                        if (isPursuingEnabled)
                            pursuingModes[i].dropTimeElapsed = 0;
                    }
                }
                //If has no target and another point to Move -> undeploy and move after waitingTime elapsed
                else if (movements[i].hasMoveTarget)
                {
                    //Update waiting time
                    if (deployables[i].waitingTimeCur < deployables[i].waitingTimeMax)
                        deployables[i].waitingTimeCur += deltaTime;
                    //if waiting time elapsed -> undeploy and move
                    if (deployables[i].deployedState && deployables[i].waitingTimeCur >= deployables[i].waitingTimeMax && // if deployed and waited enough time
                        reloads[i].isReloaded()) // and reloaded
                    {// then undelpoy
                        deployables[i].deployedState = false;

                        //Set Undeploy anim PlayOnce
                        foreach (var modelBufElem in modelsBufs[i])
                        {
                            RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
                            animCmd.ValueRW.ClipIndex = undeployClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                            animCmd.ValueRW.Cmd = AnimationCmd.PlayOnce;
                        }
                    }
                }
            }
        }
    }
}


public partial struct PursuingJob : IJobChunk
{
    public float deltaTime;
    
    [ReadOnly] public ComponentLookup<LocalToWorld> localToWorldLookup;

    public ComponentTypeHandle<PursuingModeComponent> pursuingModeSettsTypeHandle;
    public ComponentTypeHandle<BattleModeComponent> battleModeSetsTypeHandle;
    public ComponentTypeHandle<MovementComponent> movementTypeHandle;
    [ReadOnly] public ComponentTypeHandle<LocalTransform> transformTypeHandleRO;
    [ReadOnly] public ComponentTypeHandle<ReloadComponent> reloadTypeHandleRO;

    public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
    {
        unsafe
        {
            PursuingModeComponent* pursuingSetts = chunk.GetComponentDataPtrRW(ref pursuingModeSettsTypeHandle);
            MovementComponent* movements = chunk.GetComponentDataPtrRW(ref movementTypeHandle);
            LocalTransform* transforms = chunk.GetComponentDataPtrRO(ref transformTypeHandleRO);
            ReloadComponent* reloads = chunk.GetComponentDataPtrRO(ref reloadTypeHandleRO);

            ChunkEntityEnumerator chunkEnum = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
            while (chunkEnum.NextEntityIndex(out int i))
            {
                float3 targetPos = localToWorldLookup[pursuingSetts[i].Target].Position;
                float distToTargetSq = math.distancesq(transforms[i].Position, targetPos);
                if (reloads[i].isReloaded())
                    pursuingSetts[i].dropTimeElapsed += deltaTime;
                //Check if the distance to target is too big
                if (distToTargetSq > pursuingSetts[i].dropDistanceSq ||
                    pursuingSetts[i].dropTimeElapsed > pursuingSetts[i].dropTime)
                {// Turn off pursuing mode and return to BattleMode
                    chunk.SetComponentEnabled(ref battleModeSetsTypeHandle, i, true);
                    chunk.SetComponentEnabled(ref pursuingModeSettsTypeHandle, i, false);
                    continue;
                }

                //Update target of moving
                movements[i].target = targetPos;
            }
        }
    }
}




public struct AttackRequestComponent : IComponentData
{
    public Entity target;
    public float damage;
    public float3 attackerPos;
}
