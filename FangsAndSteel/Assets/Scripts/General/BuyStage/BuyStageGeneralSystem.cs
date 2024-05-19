using AnimCooker;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public partial class BuyStageGeneralSystem : SystemBase
{
    const int STARTER_PLAYER_MONEY = 1000;

    ComponentLookup<BuyStageCompletedTag> completedTagLookup;
    EntityQuery notCompletedTagQuery;
    EntityQuery notBoughtYetQuery;

    bool fstPlayerFinished;

    //float timeToClose_NewTurnPanel;

    protected override void OnCreate()
    {
        RequireForUpdate<GameTag>();
        RequireForUpdate<BuyStageNotCompletedTag>();

        completedTagLookup = SystemAPI.GetComponentLookup<BuyStageCompletedTag>();
        notCompletedTagQuery = new EntityQueryBuilder(Allocator.Persistent).WithAll<BuyStageNotCompletedTag>().Build(EntityManager);
        notBoughtYetQuery = new EntityQueryBuilder(Allocator.Persistent).WithAll<NotBoughtYetTag>().Build(EntityManager);
    }

    protected override void OnStartRunning()
    {
        completedTagLookup.Update(this);
        if (completedTagLookup.HasComponent(SystemHandle))
            EntityManager.RemoveComponent<BuyStageCompletedTag>(SystemHandle);

        // Show the buy panel
        StaticUIRefs.Instance.BuyPanel.SetActive(true);
        // Show the player buy zone
        StaticUIRefs.Instance.BuyBorders.SetActive(true);

        StaticUIRefs.Instance.BalanceText.text = STARTER_PLAYER_MONEY.ToString();

        //Set 1st as the current player
        StaticUIRefs.Instance.NewTurnText.text = "Этап закупки игрока 1";
        SystemAPI.GetSingletonRW<CurrentTeamComponent>().ValueRW.value = 1;
        fstPlayerFinished = false;
        EnableSystems(false);

        CameraControlSystem.lastCameraPos = Camera.main.transform.position;
        CameraControlSystem.lastPivotPos = Camera.main.transform.parent.position;
        CameraControlSystem.lastPivotRotation = Camera.main.transform.parent.rotation;
    }

    protected override void OnStopRunning()
    {
        // Hide the buy panel
        StaticUIRefs.Instance.BuyPanel.SetActive(false);
        // Hide the player buy zone
        StaticUIRefs.Instance.BuyBorders.SetActive(false);

        //CameraControlSystem.lastCameraPos = Camera.main.transform.position;
        //CameraControlSystem.lastPivotPos = Camera.main.transform.parent.position;
        //CameraControlSystem.lastPivotRotation = Camera.main.transform.parent.rotation;
        //Debug.Log(CameraControlSystem.lastCameraPos);
        //Debug.Log(CameraControlSystem.lastPivotPos);
        //Debug.Log(CameraControlSystem.lastPivotRotation);
    }

    protected override void OnUpdate()
    {
        StaticUIData uiData = SystemAPI.GetSingleton<StaticUIData>();

        if (uiData.newTurnStartBut)
        {
            //Set time for closing newTurnPanel
            //timeToClose_NewTurnPanel = TurnSystem.TIME_TO_CLOSE_NEW_TURN_PANEL;

            //Close the newTurnPanel
            StaticUIRefs.Instance.NewTurnPanel.SetActive(false);
        }
        //else if (timeToClose_NewTurnPanel > 0)
        //{
        //    timeToClose_NewTurnPanel -= SystemAPI.Time.DeltaTime;
        //    if (timeToClose_NewTurnPanel <= 0)
        //    {
        //        //Enable EngageMode (= Start Replay if the 2 player's turn) (if no ReplayStartCopies -> no engageMode as it is the first turn)
        //        if (!replayStartCopiesQuery.IsEmpty)
        //            EnableEngage();
        //        //else
        //        //Debug.Log("First turn - no Engage phase");
        //    }
        //}
        if (uiData.endTurnBut)
        {
            StaticUIRefs.Instance.NewTurnPanel.SetActive(true);
            CameraControlSystem.StepUpdateCameraPos();

            if (!fstPlayerFinished)
            {
                StaticUIRefs.Instance.NewTurnText.text = "Этап закупки игрока 2";
                SystemAPI.GetSingletonRW<CurrentTeamComponent>().ValueRW.value = 2;
                fstPlayerFinished = true;
                StaticUIRefs.Instance.BalanceText.text = STARTER_PLAYER_MONEY.ToString();
            }
            else
            {
                StaticUIRefs.Instance.NewTurnText.text = "Ход игрока 1";
                SystemAPI.GetSingletonRW<CurrentTeamComponent>().ValueRW.value = 1;
                EntityManager.AddComponent<BuyStageCompletedTag>(SystemHandle);
                EntityManager.RemoveComponent<BuyStageNotCompletedTag>(notCompletedTagQuery);
                EnableSystems(true);
            }

            EntityManager.DestroyEntity(notBoughtYetQuery);
        }

    }

    private void EnableSystems(bool enable)
    {
        World.Unmanaged.GetExistingSystemState<UnitsSystemGroup>().Enabled = enable;
        World.Unmanaged.GetExistingSystemState<AnimationSystem>().Enabled = enable;

        World.Unmanaged.GetExistingSystemState<TargetingMoveSystem>().Enabled = enable;
        World.Unmanaged.GetExistingSystemState<BasicButtonSystem>().Enabled = enable;
    }
}


public struct BuyStageCompletedTag : IComponentData { }
public struct BuyStageNotCompletedTag : IComponentData { }