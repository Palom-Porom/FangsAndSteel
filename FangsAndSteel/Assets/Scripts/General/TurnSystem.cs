using AnimCooker;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Entities.UI;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.UI;

public struct EndTurnRpc : IRpcCommand { }

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class _TurnSystem : SystemBase
{

    const uint NUM_OF_PLAYERS = 2;
    List<int> readyPlayersInds;

    const float TURN_LEN = 10;
    //StaticUIData uIData;
    public static float timeToRun;
    bool orderPhase;

    protected override void OnCreate()
    {
        //RequireForUpdate(*playerEntity*)
        readyPlayersInds = new List<int>();

        timeToRun = 0;
        orderPhase = true;
    }

    protected override void OnStartRunning()
    {
        EnableEngageSystems(!orderPhase);
    }

    public void OnStopRunning(ref SystemState state)
    {
        orderPhase = !orderPhase; //Forgot for what was that?
    }

    protected override void OnUpdate()
    {
        if (orderPhase)
        {
            foreach (var request in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>().WithAll<EndTurnRpc>())
            {
                readyPlayersInds.Add(request.ValueRO.SourceConnection.Index);
                if (readyPlayersInds.Count == NUM_OF_PLAYERS)
                {
                    StartEngageMode();
                }
            }
        }
        else
        {
            timeToRun -= SystemAPI.Time.DeltaTime;
            //StaticUIRefs.Instance.TurnTimer.text = $"0:{(int)timeToRun:D2}"; <-- will be done on client
            if (timeToRun <= 0)
            {
                //StaticUIRefs.Instance.TurnTimer.text = "0:00"; <-- will be done on client
                StartOrderMode();
            }
        }
    }

    private void StartEngageMode()
    {
        timeToRun = TURN_LEN;
        EnableEngageSystems(true);
        orderPhase = false;
    }

    private void StartOrderMode()
    {
        EnableEngageSystems(false);
        orderPhase = true;
        //StaticUIRefs.Instance.TurnIndicator.color = Color.red;  <-- will be done on client
    }

    private void EnableEngageSystems(bool enable)
    {
        World.Unmanaged.GetExistingSystemState<UnitsSystemGroup>().Enabled = enable;
        //World.Unmanaged.GetExistingSystemState<VisionMapSystem>().Enabled = enable;
        World.Unmanaged.GetExistingSystemState<AnimationSystem>().Enabled = enable;

        //Below will be done on the client after clicking EndTurnBut
        //World.Unmanaged.GetExistingSystemState<TargetingMoveSystem>().Enabled = !enable;
        //World.Unmanaged.GetExistingSystemState<BasicButtonSystem>().Enabled = !enable;
    }
}


//[UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
//public partial class TurnSystem : SystemBase
//{

//    const float TURN_LEN = 10; 
//    StaticUIData uIData;
//    public static float timeToRun;
//    bool orderPhase;

//    protected override void OnCreate()
//    {
//        RequireForUpdate<GameTag>();
//        RequireForUpdate<StaticUIData>();
//        timeToRun = 0;
//        orderPhase = true;
//    }

//    protected override void OnStartRunning()
//    {
//        EnableEngageSystems(!orderPhase);
//    }

//    public void OnStopRunning(ref SystemState state) 
//    {
//        orderPhase = !orderPhase;
//    }

    
//    protected override void OnUpdate()
//    {
//        //If Order phase
//        if (orderPhase)
//        {
//            uIData = SystemAPI.GetSingleton<StaticUIData>();
//            if (uIData.endTurnBut.IsSet)
//            {
//                //Start all Engage systems
//                EnableEngageSystems(true);
//                //Set Timer
//                timeToRun = TURN_LEN;

//                StaticUIRefs.Instance.TurnIndicator.color = Color.green;

//                orderPhase = false;
//            }
//        }
//        //If Engage phase
//        else
//        {
//            timeToRun -= SystemAPI.Time.DeltaTime;
//            StaticUIRefs.Instance.TurnTimer.text = $"0:{(int)timeToRun:D2}";
//            if (timeToRun <= 0)
//            {
//                StaticUIRefs.Instance.TurnTimer.text = "0:00";
//                //Stop all Engage systems
//                EnableEngageSystems(false);

//                StaticUIRefs.Instance.TurnIndicator.color = Color.red;

//                orderPhase = true;
//            }
//        }
//    }

//    private void EnableEngageSystems (bool enable)
//    {
//        World.Unmanaged.GetExistingSystemState<UnitsSystemGroup>().Enabled = enable;
//        //World.Unmanaged.GetExistingSystemState<VisionMapSystem>().Enabled = enable;
//        World.Unmanaged.GetExistingSystemState<AnimationSystem>().Enabled = enable;

//        World.Unmanaged.GetExistingSystemState<TargetingMoveSystem>().Enabled = !enable;
//        World.Unmanaged.GetExistingSystemState<BasicButtonSystem>().Enabled = !enable;
//    }
//}
