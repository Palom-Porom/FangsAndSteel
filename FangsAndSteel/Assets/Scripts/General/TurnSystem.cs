using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Entities.UI;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
public partial struct TurnSystem : ISystem, ISystemStartStop
{
    const float TURN_LEN = 10; 

    StaticUIData uIData;
    float timeToRun;
    bool orderPhase;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<StaticUIData>();

        //state.RequireForUpdate<MovementSystem>();
        //state.RequireForUpdate<AttackTargetingSystem>();
        //state.RequireForUpdate<AttackSystem>();

        timeToRun = 0;
        orderPhase = true;
    }

    public void OnStartRunning(ref SystemState state)
    {
        EnableEngageSystems(ref state, !orderPhase);
        //Debug.Log(state.WorldUnmanaged.GetExistingSystemState<MovementSystem>().Enabled);
    }

    public void OnStopRunning(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        //Debug.Log(state.WorldUnmanaged.GetExistingSystemState<MovementSystem>().Enabled);
        //If Order phase
        if (orderPhase)
        {
            uIData = SystemAPI.GetSingleton<StaticUIData>();
            if (uIData.endTurnBut)
            {
                //Start all Engage systems
                EnableEngageSystems(ref state, true);
                //Set Timer
                timeToRun = TURN_LEN;

                orderPhase = false;
            }
        }
        //If Engage phase
        else
        {
            timeToRun -= Time.deltaTime;
            if (timeToRun <= 0)
            {
                //Stop all Engage systems
                EnableEngageSystems(ref state, false);

                orderPhase = true;
            }
        }
    }

    private void EnableEngageSystems (ref SystemState systemState, bool enable)
    {
        systemState.WorldUnmanaged.GetExistingSystemState<MovementSystem>().Enabled = enable;
        systemState.WorldUnmanaged.GetExistingSystemState<TargetingAttackSystem>().Enabled = enable;
        systemState.WorldUnmanaged.GetExistingSystemState<AttackSystem>().Enabled = enable;
    }
}
