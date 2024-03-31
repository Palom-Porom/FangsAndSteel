using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(StaticUISystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class BasicButtonSystem : SystemBase
{

    ShootModeButChangeColorRqst shootButColorChangeRqst;
    EntityCommandBuffer ecb;

    StaticUIData uiData;

    bool isOrderPhase;
    float timeToRun;
    const int TURN_LEN = 10;

    protected override void OnCreate()
    {
        RequireForUpdate<StaticUIData>();
        isOrderPhase = true;
        timeToRun = 0;
    }

    protected override void OnUpdate()
    {
        //uiData = SystemAPI.GetSingleton<StaticUIData>();
        foreach (StaticUIData uiData in SystemAPI.Query<StaticUIData>().WithAll<GhostOwnerIsLocal>())
        {

            if (uiData.endTurnBut.IsSet)
            {
                EntityManager.CreateEntity(typeof(EndTurnRpc), typeof(SendRpcCommandRequest));
                StaticUIRefs.Instance.TurnIndicator.color = Color.green;
            }

            if (isOrderPhase)
            {
                foreach (var (request, requestEntity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>().WithAll<StartEngageModeRpc>().WithEntityAccess())
                {
                    EnableEngageSystems(true);
                    EntityManager.DestroyEntity(requestEntity);
                    timeToRun = TURN_LEN;
                    isOrderPhase = false;
                }
            }
            else
            {
                timeToRun -= SystemAPI.Time.DeltaTime;
                StaticUIRefs.Instance.TurnTimer.text = $"0:{(int)timeToRun:D2}";
                if (timeToRun <= 0)
                {
                    StaticUIRefs.Instance.TurnTimer.text = "0:00";
                    //Stop all Engage systems
                    EnableEngageSystems(false);

                    StaticUIRefs.Instance.TurnIndicator.color = Color.red;

                    isOrderPhase = true;
                }
            }

            //if (uiData.stopMoveBut) <- is done in the server
            //{
            //    foreach ((RefRW<MovementComponent> movementComponent, DynamicBuffer<MovementCommandsBuffer> moveComBuf, LocalTransform localTransform) 
            //        in SystemAPI.Query<RefRW<MovementComponent>, DynamicBuffer<MovementCommandsBuffer>, LocalTransform>().WithAll<SelectTag>())
            //    {
            //        moveComBuf.Clear();
            //        movementComponent.ValueRW.target = localTransform.Position;
            //        movementComponent.ValueRW.hasMoveTarget = false;
            //    }
            //}

            if (SystemAPI.TryGetSingleton<ShootModeButChangeColorRqst>(out shootButColorChangeRqst))
            {
                StaticUIRefs.Instance.ShootModeButton.color = shootButColorChangeRqst.color;
                ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
                ecb.DestroyEntity(SystemAPI.GetSingletonEntity<ShootModeButChangeColorRqst>());
            }

            if (uiData.shootModeBut.Count == 1)
            {
                //Debug.Log("1");
                //new ChangeShootModeJob().Schedule(); <-- is done in the server
                Color c = StaticUIRefs.Instance.ShootModeButton.color;
                if (c.r != c.g)
                {
                    //Debug.Log("2");
                    StaticUIRefs.Instance.ShootModeButton.color = new Color((c.r + 1) % 2, (c.g + 1) % 2, 0);
                }
                Debug.Log("shootModeBut Client func (changed color) is done");
            }
        }
    }

    private void EnableEngageSystems(bool enable)
    {
        //World.Unmanaged.GetExistingSystemState<UnitsSystemGroup>().Enabled = enable;
        //World.Unmanaged.GetExistingSystemState<VisionMapSystem>().Enabled = enable;
        //World.Unmanaged.GetExistingSystemState<AnimationSystem>().Enabled = enable;

        World.Unmanaged.GetExistingSystemState<TargetingMoveSystem>().Enabled = !enable;
        World.Unmanaged.GetExistingSystemState<BasicButtonSystem>().Enabled = !enable;
    }
}

[UpdateInGroup(typeof(StaticUISystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class BasicButtonSystemServer : SystemBase
{
    StaticUIData uiData;

    protected override void OnCreate()
    {
        RequireForUpdate<StaticUIData>();
    }
    protected override void OnUpdate()
    {
        foreach (StaticUIData uiData in SystemAPI.Query<StaticUIData>())
        {
            //uiData = SystemAPI.GetSingleton<StaticUIData>();

            if (uiData.stopMoveBut.IsSet)
            {
                foreach ((RefRW<MovementComponent> movementComponent, DynamicBuffer<MovementCommandsBuffer> moveComBuf, LocalTransform localTransform, TeamComponent team)
                    in SystemAPI.Query<RefRW<MovementComponent>, DynamicBuffer<MovementCommandsBuffer>, LocalTransform, TeamComponent>().WithAll<SelectTag>())
                {
                    if (uiData.teamInd != team.teamInd) continue;
                    moveComBuf.Clear();
                    movementComponent.ValueRW.target = localTransform.Position;
                    movementComponent.ValueRW.hasMoveTarget = false;
                }
            }

            if (uiData.shootModeBut.IsSet)
            {
                new ChangeShootModeJob { teamInd = uiData.teamInd }.Schedule();
                Debug.Log("shootModeBut Server func (changed mode and debuffes) is done");
            }
        }
    }
}


[WithAll(typeof(SelectTag))]
public partial struct ChangeShootModeJob : IJobEntity
{
    public int teamInd;

    public void Execute(ref BattleModeComponent battleModeSettings, ref ReloadComponent reloadComponent, ref MovementComponent movement , in TeamComponent team)
    {
        if (teamInd != team.teamInd) return;

        battleModeSettings.shootingOnMove = !(battleModeSettings.shootingOnMove);
        reloadComponent.curDebaff += battleModeSettings.shootingOnMove ? reloadComponent.reload_SoM_Debaff : -reloadComponent.reload_SoM_Debaff;
        movement.curDebaff += battleModeSettings.shootingOnMove ? movement.movement_SoM_Debaff : -movement.movement_SoM_Debaff;
    }
}

public struct ShootModeButChangeColorRqst : IComponentData
{
    public Color color;
}
    

