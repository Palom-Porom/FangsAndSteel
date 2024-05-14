using AnimCooker;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public partial class BuyStageGeneralSystem : SystemBase
{
    ComponentLookup<BuyStageCompletedTag> completedTagLookup;
    EntityQuery notCompletedTagQuery;

    bool fstPlayerFinished;

    float timeToClose_NewTurnPanel;

    protected override void OnCreate()
    {
        RequireForUpdate<GameTag>();
        RequireForUpdate<BuyStageNotCompletedTag>();

        completedTagLookup = SystemAPI.GetComponentLookup<BuyStageCompletedTag>();
        notCompletedTagQuery = new EntityQueryBuilder(Allocator.Persistent).WithAll<BuyStageNotCompletedTag>().Build(EntityManager);
    }

    protected override void OnStartRunning()
    {
        completedTagLookup.Update(this);
        if (completedTagLookup.HasComponent(SystemHandle))
            EntityManager.RemoveComponent<BuyStageCompletedTag>(SystemHandle);

        ///TODO: Show the buy panel
        ///TODO: Show the player buy zone

        //Set 1st as the current player
        StaticUIRefs.Instance.NewTurnText.text = "Этап закупки игрока 1";
        SystemAPI.GetSingletonRW<CurrentTeamComponent>().ValueRW.value = 1;
        fstPlayerFinished = false;
        EnableSystems(false);
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

            if (!fstPlayerFinished)
            {
                StaticUIRefs.Instance.NewTurnText.text = "Этап закупки игрока 2";
                SystemAPI.GetSingletonRW<CurrentTeamComponent>().ValueRW.value = 2;
                fstPlayerFinished = true;
            }
            else
            {
                StaticUIRefs.Instance.NewTurnText.text = "Ход игрока 1";
                SystemAPI.GetSingletonRW<CurrentTeamComponent>().ValueRW.value = 1;
                EntityManager.AddComponent<BuyStageCompletedTag>(SystemHandle);
                EntityManager.RemoveComponent<BuyStageNotCompletedTag>(notCompletedTagQuery);
                EnableSystems(true);
            }
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