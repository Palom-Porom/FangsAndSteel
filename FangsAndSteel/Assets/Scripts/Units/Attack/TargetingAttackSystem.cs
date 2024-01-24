using AnimCooker;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

[UpdateInGroup(typeof(UnitsSystemGroup))]
[BurstCompile]
public partial struct TargetingAttackSystem : ISystem, ISystemStartStop
{
    ComponentLookup<HpComponent> hpLookup;
    ComponentLookup<LocalToWorld> localToWorldLookup;
    ComponentLookup<TeamComponent> teamLookup;
    ComponentLookup<FillFloatOverride> fillBarLookup;

    EntityQuery usualUnitsQuery;
    EntityQuery deployableUnitsQuery;

    EntityQuery potentialTargetsQuery;

    #region Animation vars
    ComponentLookup<AnimationCmdData> animCmdLookup;
    ComponentLookup<AnimationStateData> animStateLookup;
    NativeArray<AnimDbEntry> attackClips;
    NativeArray<AnimDbEntry> reloadClips;
    NativeArray<AnimDbEntry> moveClips;
    NativeArray<AnimDbEntry> deployClips;
    NativeArray<AnimDbEntry> undeployClips;
    NativeArray<AnimDbEntry> restClips;
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

        usualUnitsQuery = new EntityQueryBuilder (Allocator.TempJob).
            WithAll< AttackCharsComponent, AttackSettingsComponent, LocalTransform, TeamComponent, UnitsIconsComponent, ModelsBuffer, MovementComponent > ().
            WithNone<Deployable>().
            Build(state.EntityManager);

        deployableUnitsQuery = new EntityQueryBuilder (Allocator.TempJob).
            WithAll< AttackCharsComponent, AttackSettingsComponent, LocalTransform, TeamComponent, UnitsIconsComponent, ModelsBuffer, Deployable > ().
            WithAll<MovementComponent>().
            Build(state.EntityManager);

        potentialTargetsQuery = new EntityQueryBuilder(Allocator.TempJob).WithAll<HpComponent, LocalToWorld, TeamComponent>().Build(ref state);

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
        NativeArray<Entity> potentialTargetsArr = potentialTargetsQuery.ToEntityArray(Allocator.TempJob);

        var ecb =  SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        #endregion

        #region Usual Units Job
        if (!usualUnitsQuery.IsEmpty)
        {
            AttackTargetingJob attackTargetingJob = new AttackTargetingJob
            {
                hpLookup = hpLookup,
                localToWorldLookup = localToWorldLookup,
                teamLookup = teamLookup,
                fillBarLookup = fillBarLookup,

                potentialTargetsArr = potentialTargetsArr,

                ecb = ecb,
                deltaTime = Time.deltaTime,

                animCmdLookup = animCmdLookup,
                animStateLookup = animStateLookup,
                attackClips = attackClips,
                reloadClips = reloadClips,
                moveClips = moveClips,
                restClips = restClips
            };
            state.Dependency = attackTargetingJob.Schedule(usualUnitsQuery, state.Dependency);
        }
        #endregion

        #region Deployable Units Job
        if (!deployableUnitsQuery.IsEmpty)
        {
            AttackTargetingDeployableJob attackTargetingDeployableJob = new AttackTargetingDeployableJob
            {
                hpLookup = hpLookup,
                localToWorldLookup = localToWorldLookup,
                teamLookup = teamLookup,
                fillBarLookup = fillBarLookup,

                potentialTargetsArr = potentialTargetsArr,

                ecb = ecb,
                deltaTime = Time.deltaTime,

                animCmdLookup = animCmdLookup,
                animStateLookup = animStateLookup,
                attackClips = attackClips,
                reloadClips = reloadClips,
                moveClips = moveClips,
                deployClips = deployClips,
                undeployClips = undeployClips
            };
            state.Dependency = attackTargetingDeployableJob.Schedule(deployableUnitsQuery, state.Dependency);
        }
        #endregion

        ///TODO: Dispose of the potentialTargetsArr!!!!!!!!!!!!!!!!!!!!!!!!
    }
}

//If ComponentLookups are heavy indeed, then it is better to rewrite as 3 jobs connected with NativeReferences<info>
/// <summary>
/// Checks all current attack targets and searches for new ones [For Usual Units]
/// </summary>
//[BurstCompile]
public partial struct AttackTargetingJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<HpComponent> hpLookup;
    [ReadOnly] public ComponentLookup<LocalToWorld> localToWorldLookup;
    [ReadOnly] public ComponentLookup<TeamComponent> teamLookup;
    public ComponentLookup<FillFloatOverride> fillBarLookup;

    [ReadOnly] public NativeArray<Entity> potentialTargetsArr;

    public EntityCommandBuffer.ParallelWriter ecb;

    public float deltaTime;

    private float modHp;
    private float modDist;

    public ComponentLookup<AnimationCmdData> animCmdLookup;
    public ComponentLookup<AnimationStateData> animStateLookup;
    [ReadOnly] public NativeArray<AnimDbEntry> attackClips;
    [ReadOnly] public NativeArray<AnimDbEntry> reloadClips;
    [ReadOnly] public NativeArray<AnimDbEntry> moveClips;
    [ReadOnly] public NativeArray<AnimDbEntry> restClips;

    private const float ROT_TIME = 0.33f;

    public void Execute(ref AttackCharsComponent attack, ref AttackSettingsComponent attackSettings, ref LocalTransform localTransform, in TeamComponent team, ref MovementComponent movement,
        in UnitsIconsComponent unitsIcons, [ChunkIndexInQuery] int chunkIndexInQuery, in DynamicBuffer<ModelsBuffer> modelsBuf, Entity entity)
    {
        #region Reload
        //For now put the reloading here, but maybe then it is a good idea to put it in another Job with updating all units characteristics (MAYBE for example hp from healing)
        attack.curReload += deltaTime;
        if (attack.curReload - attack.reloadLen > float.Epsilon)
            attack.curReload = attack.reloadLen;
        fillBarLookup.GetRefRW(unitsIcons.reloadBarEntity).ValueRW.Value = attack.curReload / attack.reloadLen;
        #endregion

        #region Rotate to enemy
        if (!attackSettings.isAbleToMove && hpLookup.HasComponent(attack.target))
        {
            quaternion targetRot = quaternion.LookRotationSafe(localToWorldLookup[attack.target].Position - localTransform.Position, localTransform.Up());
            if (movement.initialRotation.Equals(targetRot))
            {
                if (movement.rotTimeElapsed < ROT_TIME)
                {
                    movement.rotTimeElapsed += deltaTime;
                    localTransform.Rotation = math.nlerp(localTransform.Rotation, targetRot, movement.rotTimeElapsed / ROT_TIME);
                }
            }
            else
            {
                movement.rotTimeElapsed = deltaTime;
                movement.initialRotation = targetRot;
                localTransform.Rotation = math.nlerp(localTransform.Rotation, targetRot, deltaTime / ROT_TIME);
            }
        }
        #endregion

        //If not reloaded yet then no need for search for target
        if (math.abs(attack.curReload - attack.reloadLen) > float.Epsilon)
            return;

        #region If has valid target -> create an attackRequest and return
        if (attack.target != Entity.Null
            && math.distancesq(localTransform.Position, localToWorldLookup[attack.target].Position) <= attack.radiusSq
            && hpLookup.HasComponent(attack.target))
        {
            attackSettings.isAbleToMove = attackSettings.shootingOnMoveMode;
            //when shootingOnMoveMode was already enabled
            if (attackSettings.isAbleToMove)
            {
                ecb.AddComponent(chunkIndexInQuery, entity, new NotAbleToMoveForTimeRqstComponent { passedTime = 0, targetTime = attack.timeToShoot });
                attackSettings.isAbleToMove = false;
            }


            CreateAttackRequest(chunkIndexInQuery, attack.target, attack.damage, localTransform.Position, modelsBuf);
            attack.curReload = 0;
            return;
        }
        #endregion

        #region Find new Target
        if (attackSettings.targettingMinHP)
        {
            modHp = 10000;
            modDist = 0.1f;
        }

        float bestScore = float.MinValue;
        Entity bestScoreEntity = Entity.Null;

        foreach (Entity potentialTarget in potentialTargetsArr)
        {
            //Check if they are in different teams
            if (teamLookup[potentialTarget].teamInd - team.teamInd == 0)
                continue;
            float curScore = 0;

            float distanceScore = attack.radiusSq - math.distancesq(localTransform.Position, localToWorldLookup[potentialTarget].Position);
            //Check if target is not in the attack radius
            if (distanceScore < 0)
                continue;
            curScore += distanceScore * modDist;

            float hpScore = -(hpLookup[potentialTarget].curHp);
            curScore += hpScore * modHp;
            ///TODO Other score affectors

            if (curScore > bestScore)
            {
                bestScore = curScore;
                bestScoreEntity = potentialTarget;
            }
        }

        attack.target = bestScoreEntity;
        #endregion

        #region Proccess the Result
        if (attack.target != Entity.Null)
        {
            if (attackSettings.shootingOnMoveMode)
                ecb.AddComponent(chunkIndexInQuery, entity, new NotAbleToMoveForTimeRqstComponent { passedTime = 0, targetTime = attack.timeToShoot });
            attackSettings.isAbleToMove = false;
            CreateAttackRequest(chunkIndexInQuery, attack.target, attack.damage, localTransform.Position, modelsBuf);
            attack.curReload = 0;
        }
        else
        {
            if (!attackSettings.isAbleToMove)
            { 
                if (movement.hasMoveTarget)
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
            }
            attackSettings.isAbleToMove = true;
        }
        #endregion

    }

    //private Entity FindBestTarget(float3 position, int radiusSq, int teamInd, float modDist, float modHp)
    //{
    //    float bestScore = float.MinValue;
    //    Entity bestScoreEntity = Entity.Null;

    //    foreach (Entity potentialTarget in potentialTargetsArr)
    //    {
    //        //Check if they are in different teams
    //        if (teamLookup[potentialTarget].teamInd - teamInd == 0)
    //            continue;

    //        float curScore = 0;

    //        float distanceScore = radiusSq - math.distancesq(position, localToWorldLookup[potentialTarget].Position)/* * distScoreMultiplier*/;
    //        //Check if target is not in the attack radius
    //        if (distanceScore < 0)
    //            continue;
    //        curScore += distanceScore * modDist;

    //        float hpScore = -(hpLookup[potentialTarget].curHp);
    //        curScore += hpScore * modHp;
    //        ///TODO Other score affectors

    //        if (curScore > bestScore)
    //        {
    //            bestScore = curScore;
    //            bestScoreEntity = potentialTarget;
    //        }
    //    }
    //    return bestScoreEntity;
    //}
    private void CreateAttackRequest(int chunkIndexInQuery, Entity target, float damage, float3 attackerPos, in DynamicBuffer<ModelsBuffer> modelsBuf)
    {
        Entity attackRequest = ecb.CreateEntity(chunkIndexInQuery);
        ecb.AddComponent(chunkIndexInQuery, attackRequest, new AttackRequestComponent { target = target, damage = damage, attackerPos = attackerPos });

        //Play Attack Anim
        foreach (var modelBufElem in modelsBuf)
        {
            RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
            byte reloadIdx = reloadClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
            animStateLookup.GetRefRW(modelBufElem.model).ValueRW.ForeverClipIndex = reloadIdx;
            animCmd.ValueRW.ClipIndex = attackClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
            animCmd.ValueRW.Cmd = AnimationCmd.PlayOnce;
        }
    }
}




///<summary> Update all units' reloadTime values and reload bars </summary>
public partial struct ReloadJob : IJobEntity
{
    public float deltaTime;

    public ComponentLookup<FillFloatOverride> fillBarLookup;

    public void Execute(ref ReloadComponent reloadComponent, in UnitsIconsComponent unitsIcons)
    {

        if (reloadComponent.curBullets == 0) // if drum is empty -> drum reload
        {
            reloadComponent.drumReloadElapsed += deltaTime;
            if (reloadComponent.drumReloadElapsed >= reloadComponent.drumReloadLen * reloadComponent.curDebaff)
            {
                reloadComponent.curBullets = reloadComponent.maxBullets;
                reloadComponent.bulletReloadElapsed = reloadComponent.bulletReloadLen;
            }
            fillBarLookup.GetRefRW(unitsIcons.reloadBarEntity).ValueRW.Value = reloadComponent.drumReloadElapsed / reloadComponent.drumReloadLen;
        }

        else if (reloadComponent.bulletReloadElapsed >= reloadComponent.bulletReloadLen) // reload of the bullet
        {
            if (reloadComponent.bulletReloadElapsed <= float.Epsilon) //If just shot -> update the ReloadBar
                fillBarLookup.GetRefRW(unitsIcons.reloadBarEntity).ValueRW.Value = reloadComponent.curBullets / reloadComponent.maxBullets;
            reloadComponent.bulletReloadElapsed += deltaTime;
        }

    }
}


///<summary> Update deploying/undeploying values of units </summary>
public partial struct UpdateDeployJob : IJobEntity
{
    public float deltaTime;


    public ComponentLookup<AnimationCmdData> animCmdLookup;
    [ReadOnly] public ComponentLookup<AnimationStateData> animStateLookup;
    [ReadOnly] public NativeArray<AnimDbEntry> moveClips;

    public void Execute(ref Deployable deployable, ref MovementComponent movementComponent, in DynamicBuffer<ModelsBuffer> modelsBuf)
    {
        //Undeploying
        if (!deployable.deployedState)
        {
            if (!movementComponent.isAbleToMove)
            {
                if (deployable.deployTimeElapsed <= 0)
                {
                    //Set move animation PlayForever
                    foreach (var modelBufElem in modelsBuf)
                    {
                        RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
                        animCmd.ValueRW.ClipIndex = moveClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                        animCmd.ValueRW.Cmd = AnimationCmd.SetPlayForever;
                    }
                    movementComponent.isAbleToMove = true;
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
public partial struct AttackTargetSearchJob : IJobEntity
{
    /// <summary> In other words all units that can be attacked </summary>
    [ReadOnly] public NativeArray<Entity> potentialTargetsArr;

    [ReadOnly] public ComponentLookup<LocalToWorld> localToWorldLookup;
    [ReadOnly] public ComponentLookup<TeamComponent> teamLookup;
    [ReadOnly] public ComponentLookup<HpComponent> hpLookup;

    public void Execute(ref AttackCharsComponent attackChars, in BattleModeComponent modeSettings, in LocalTransform localTransform, in TeamComponent team, in MovementComponent movement, Entity entity)
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
            if ((modeSettings.autoTriggerMoving || (modeSettings.autoTriggerStatic && (!movement.hasMoveTarget || movement.isAbleToMove)))
                &&
                distanceToPotTargetSq < modeSettings.autoTriggerRadiusSq
                &&
                (hpLookup[potentialTarget].curHp / hpLookup[potentialTarget].maxHp) <= modeSettings.autoTriggerMaxHpPercent
                /*&& *check the type of unit* */)
            {
                curScore += 10000; // such targets has higher priority than other (auto-trigger has more priority than usual attack)
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
public partial struct CreateUsualAttackRequestsJob : IJobEntity
{
    [ReadOnly] ComponentLookup<LocalTransform> transformLookup;

    public EntityCommandBuffer.ParallelWriter ecb;

    public ComponentLookup<AnimationCmdData> animCmdLookup;
    public ComponentLookup<AnimationStateData> animStateLookup;
    [ReadOnly] public NativeArray<AnimDbEntry> attackClips;
    [ReadOnly] public NativeArray<AnimDbEntry> reloadClips;
    [ReadOnly] public NativeArray<AnimDbEntry> moveClips;
    [ReadOnly] public NativeArray<AnimDbEntry> restClips;

    public void Execute(in AttackCharsComponent attackChars, in BattleModeComponent modeSettings, ref MovementComponent movementComponent, Entity entity,
        in DynamicBuffer<ModelsBuffer> modelsBuf, in LocalTransform localTransform, ref ReloadComponent reloadComponent, [ChunkIndexInQuery] int chunkIndexInQuery,
        ref RotationToTargetComponent rotation)
    {
        //if has some target and reloaded -> shoot
        if (attackChars.target != Entity.Null && reloadComponent.curBullets > 0 && reloadComponent.bulletReloadElapsed >= reloadComponent.bulletReloadLen) 
        {
            if (modeSettings.shootingOnMove)
                ecb.AddComponent(chunkIndexInQuery, entity, new NotAbleToMoveForTimeRqstComponent { passedTime = 0, targetTime = attackChars.timeToShoot });
            movementComponent.isAbleToMove = false;
            //Rotate to target
            rotation.newRotTarget = quaternion.LookRotationSafe(transformLookup[attackChars.target].Position - localTransform.Position, localTransform.Up());

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
        //if cannot shoot -> can move + anim change (move or just stand)
        else
        {
            if (!movementComponent.isAbleToMove)
            {
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


///<summary> Creates attack requests if needed and do connected to this things (animation, etc.) </summary>
/// <remarks> That Job is for Deployable units! </remarks>
public partial struct CreateDeployableAttackRequestsJob : IJobEntity
{
    [ReadOnly] ComponentLookup<LocalTransform> transformLookup;

    public EntityCommandBuffer.ParallelWriter ecb;

    public float deltaTime;

    public ComponentLookup<AnimationCmdData> animCmdLookup;
    [ReadOnly] public ComponentLookup<AnimationStateData> animStateLookup;
    [ReadOnly] public NativeArray<AnimDbEntry> attackClips;
    [ReadOnly] public NativeArray<AnimDbEntry> reloadClips;
    [ReadOnly] public NativeArray<AnimDbEntry> moveClips;
    [ReadOnly] public NativeArray<AnimDbEntry> deployClips;
    [ReadOnly] public NativeArray<AnimDbEntry> undeployClips;

    public void Execute(ref Deployable deployable, in AttackCharsComponent attackChars, ref MovementComponent movementComponent, in DynamicBuffer<ModelsBuffer> modelsBuf,
        ref ReloadComponent reloadComponent, [ChunkIndexInQuery] int chunkIndexInQuery, in LocalTransform localTransform, ref RotationToTargetComponent rotation)
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
                    animCmd.ValueRW.ClipIndex = deployClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                    animCmd.ValueRW.Cmd = AnimationCmd.PlayOnce;
                }
            }
            //If fully Deployed and Reloaded -> create Attack Rqst
            else if (deployable.deployTimeElapsed >= deployable.deployTime && reloadComponent.curBullets > 0 && reloadComponent.bulletReloadElapsed >= reloadComponent.bulletReloadLen)
            {
                //Rotate to target
                rotation.newRotTarget = quaternion.LookRotationSafe(transformLookup[attackChars.target].Position - localTransform.Position, localTransform.Up());

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
            if (deployable.deployedState && deployable.waitingTimeCur >= deployable.waitingTimeMax)
            {
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



//public partial struct RotationToTargetJob : IJobEntity
//{

//    public float deltaTime;

//    //[ReadOnly] public ComponentLookup<HpComponent> hpLookup;
//    [ReadOnly] public ComponentLookup<Deployable> deployLookup;
//    [ReadOnly] public ComponentLookup<LocalToWorld> localToWorldLookup;

//    Deployable deployable;

//    private const float ROT_TIME = 0.33f;

//    public void Execute(ref LocalTransform localTransform,in AttackCharsComponent attackChars, ref MovementComponent movementComponent, Entity entity)
//    {
//        if (attackChars.target != Entity.Null && !movementComponent.isAbleToMove /*&& hpLookup.HasComponent(attackChars.target)*/) // <- I suppose there will be no such occasions when target is without hp component anymore
//        {
//            if (deployLookup.TryGetComponent(entity, out deployable))
//            {
//                if (!deployable.deployedState || (deployable.deployTimeElapsed < deployable.deployTime))
//                    return;
//            }
//            quaternion targetRot = quaternion.LookRotationSafe(localToWorldLookup[attackChars.target].Position - localTransform.Position, localTransform.Up());
//            if (movementComponent.initialRotation.Equals(targetRot))
//            {
//                if (movementComponent.rotTimeElapsed < ROT_TIME)
//                {
//                    movementComponent.rotTimeElapsed += deltaTime;
//                    localTransform.Rotation = math.nlerp(localTransform.Rotation, targetRot, movementComponent.rotTimeElapsed / ROT_TIME);
//                }
//            }
//            else
//            {
//                movementComponent.rotTimeElapsed = deltaTime;
//                movementComponent.initialRotation = targetRot;
//                localTransform.Rotation = math.nlerp(localTransform.Rotation, targetRot, deltaTime / ROT_TIME);
//            }
//        }
//    }
//}



//Turning Job

//Pursuing Job



/// <summary>
/// Checks all current attack targets and searches for new ones [For Deployable Units]
/// </summary>
[BurstCompile]
public partial struct AttackTargetingDeployableJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<HpComponent> hpLookup;
    [ReadOnly] public ComponentLookup<LocalToWorld> localToWorldLookup;
    [ReadOnly] public ComponentLookup<TeamComponent> teamLookup;
    public ComponentLookup<FillFloatOverride> fillBarLookup;

    [ReadOnly] public NativeArray<Entity> potentialTargetsArr;

    public EntityCommandBuffer.ParallelWriter ecb;

    public float deltaTime;

    private float modHp;
    private float modDist;

    public ComponentLookup<AnimationCmdData> animCmdLookup;
    [ReadOnly] public ComponentLookup<AnimationStateData> animStateLookup;
    [ReadOnly] public NativeArray<AnimDbEntry> attackClips;
    [ReadOnly] public NativeArray<AnimDbEntry> reloadClips;
    [ReadOnly] public NativeArray<AnimDbEntry> moveClips;
    [ReadOnly] public NativeArray<AnimDbEntry> deployClips;
    [ReadOnly] public NativeArray<AnimDbEntry> undeployClips;

    private const float ROT_TIME = 0.33f;

    public void Execute(ref AttackCharsComponent attack, ref AttackSettingsComponent attackSettings, ref LocalTransform localTransform, in TeamComponent team, ref MovementComponent movement,
        in UnitsIconsComponent unitsIcons, [ChunkIndexInQuery] int chunkIndexInQuery, [ReadOnly] DynamicBuffer<ModelsBuffer> modelsBuf, ref Deployable deployable)
    {
        

        #region Deploy Handle
        //Undeploying
        if (!deployable.deployedState)
        {
            if (deployable.deployTimeElapsed > 0)
                deployable.deployTimeElapsed -= deltaTime;
            if (!attackSettings.isAbleToMove && deployable.deployTimeElapsed <= 0)
            {
                //Set move animation PlayForever
                foreach (var modelBufElem in modelsBuf)
                {
                    RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
                    animCmd.ValueRW.ClipIndex = moveClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                    animCmd.ValueRW.Cmd = AnimationCmd.SetPlayForever;
                }
                attackSettings.isAbleToMove = true;
            }
        }
        //Deploying
        else if (deployable.deployTimeElapsed < deployable.deployTime)
        {
            deployable.deployTimeElapsed += deltaTime;
            return;
        }
        #endregion

        bool fullyDeployed = deployable.deployTimeElapsed >= deployable.deployTime;

        #region Rotate to enemy
        if (!attackSettings.isAbleToMove && fullyDeployed && hpLookup.HasComponent(attack.target))
        {
            quaternion targetRot = quaternion.LookRotationSafe(localToWorldLookup[attack.target].Position - localTransform.Position, localTransform.Up());
            if (movement.initialRotation.Equals(targetRot))
            {
                if (movement.rotTimeElapsed < ROT_TIME)
                {
                    movement.rotTimeElapsed += deltaTime;
                    localTransform.Rotation = math.nlerp(localTransform.Rotation, targetRot, movement.rotTimeElapsed / ROT_TIME);
                }
            }
            else
            {
                movement.rotTimeElapsed = deltaTime;
                movement.initialRotation = targetRot;
                localTransform.Rotation = math.nlerp(localTransform.Rotation, targetRot, deltaTime / ROT_TIME);
            }
        }
        #endregion

        if (fullyDeployed)
        {
            #region Reload
            //For now put the reloading here, but maybe then it is a good idea to put it in another Job with updating all units characteristics (MAYBE for example hp from healing)
            attack.curReload += deltaTime;
            if (attack.curReload - attack.reloadLen > float.Epsilon)
                attack.curReload = attack.reloadLen;
            fillBarLookup.GetRefRW(unitsIcons.reloadBarEntity).ValueRW.Value = attack.curReload / attack.reloadLen;
            #endregion

            #region If has valid target -> create an attackRequest and return
            if (attack.target != Entity.Null
                && math.abs(attack.curReload - attack.reloadLen) <= float.Epsilon
                && math.distancesq(localTransform.Position, localToWorldLookup[attack.target].Position) <= attack.radiusSq
                && hpLookup.HasComponent(attack.target))
            {
                CreateAttackRequest(chunkIndexInQuery, attack.target, attack.damage, localTransform.Position, modelsBuf);
                attack.curReload = 0;
                return;
            }
            #endregion
        }

        #region Find new Target
        if (attackSettings.targettingMinHP)
        {
            modHp = 10000;
            modDist = 0.1f;
        }

        float bestScore = float.MinValue;
        Entity bestScoreEntity = Entity.Null;

        foreach (Entity potentialTarget in potentialTargetsArr)
        {
            //Check if they are in different teams
            if (teamLookup[potentialTarget].teamInd - team.teamInd == 0)
                continue;
            float curScore = 0;

            float distanceScore = attack.radiusSq - math.distancesq(localTransform.Position, localToWorldLookup[potentialTarget].Position);
            //Check if target is not in the attack radius
            if (distanceScore < 0)
                continue;
            curScore += distanceScore * modDist;

            float hpScore = -(hpLookup[potentialTarget].curHp);
            curScore += hpScore * modHp;
            ///TODO Other score affectors

            if (curScore > bestScore)
            {
                bestScore = curScore;
                bestScoreEntity = potentialTarget;
            }
        }

        attack.target = bestScoreEntity;
        #endregion

        #region Proccess the Result
        if (attack.target != Entity.Null)
        {
            deployable.waitingTimeCur = 0;

            //if Undeployed, than start Deploying
            if (!deployable.deployedState)
            {
                deployable.deployedState = true;
                attackSettings.isAbleToMove = false;

                //Set Deploy anim PlayOnce
                foreach (var modelBufElem in modelsBuf)
                {
                    RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
                    animCmd.ValueRW.ClipIndex = deployClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
                    animCmd.ValueRW.Cmd = AnimationCmd.PlayOnce;
                }
            }
            //If fully Deployed and Reloaded -> create Attack Rqst
            else if (fullyDeployed && math.abs(attack.curReload - attack.reloadLen) <= float.Epsilon)
            {
                CreateAttackRequest(chunkIndexInQuery, attack.target, attack.damage, localTransform.Position, modelsBuf);
                attack.curReload = 0;
            }
        }
        else if (movement.hasMoveTarget) //If has another point to Move, than no need to Undeploy
        {
            if (deployable.waitingTimeCur < deployable.waitingTimeMax)
                deployable.waitingTimeCur += deltaTime;
            //if waiting too long for the new target -> undeploy
            if (deployable.deployedState && deployable.waitingTimeCur >= deployable.waitingTimeMax)
            {
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
        #endregion
    }

    //private Entity FindBestTarget(float3 position, int radiusSq, int teamInd, float modDist, float modHp)
    //{
    //    float bestScore = float.MinValue;
    //    Entity bestScoreEntity = Entity.Null;

    //    foreach (Entity potentialTarget in potentialTargetsArr)
    //    {
    //        //Check if they are in different teams
    //        if (teamLookup[potentialTarget].teamInd - teamInd == 0)
    //            continue;

    //        float curScore = 0;

    //        float distanceScore = radiusSq - math.distancesq(position, localToWorldLookup[potentialTarget].Position)/* * distScoreMultiplier*/;
    //        //Check if target is not in the attack radius
    //        if (distanceScore < 0)
    //            continue;
    //        curScore += distanceScore * modDist;

    //        float hpScore = -(hpLookup[potentialTarget].curHp);
    //        curScore += hpScore * modHp;
    //        ///TODO Other score affectors

    //        if (curScore > bestScore)
    //        {
    //            bestScore = curScore;
    //            bestScoreEntity = potentialTarget;
    //        }
    //    }
    //    return bestScoreEntity;
    //}
    private void CreateAttackRequest(int chunkIndexInQuery, Entity target, float damage, float3 attackerPos, in DynamicBuffer<ModelsBuffer> modelsBuf)
    {
        Entity attackRequest = ecb.CreateEntity(chunkIndexInQuery);
        ecb.AddComponent(chunkIndexInQuery, attackRequest, new AttackRequestComponent { target = target, damage = damage, attackerPos = attackerPos });

        //Play Attack Anim
        foreach (var modelBufElem in modelsBuf)
        {
            RefRW<AnimationCmdData> animCmd = animCmdLookup.GetRefRW(modelBufElem.model);
            ///TODO: Reload when the anim on model appear
            animCmd.ValueRW.ClipIndex = attackClips[animStateLookup[modelBufElem.model].ModelIndex].ClipIndex;
            animCmd.ValueRW.Cmd = AnimationCmd.PlayOnce;
        }
    }
}


public struct AttackRequestComponent : IComponentData
{
    public Entity target;
    public float damage;
    public float3 attackerPos;
}
