using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Entities.UI;
using UnityEngine;
using UnityEngine.UI;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
public partial class TurnSystem : SystemBase
{

    const float TURN_LEN = 10; 
    StaticUIData uIData;
    float timeToRun;
    bool orderPhase;

    protected override void OnCreate()
    {
        RequireForUpdate<StaticUIData>();
        timeToRun = 0;
        orderPhase = true;
    }

    protected override void OnStartRunning()
    {
        EnableEngageSystems(!orderPhase);
    }

    public void OnStopRunning(ref SystemState state) { }

    
    protected override void OnUpdate()
    {
        //If Order phase
        if (orderPhase)
        {
            uIData = SystemAPI.GetSingleton<StaticUIData>();
            if (uIData.endTurnBut)
            {
                //Start all Engage systems
                EnableEngageSystems(true);
                //Set Timer
                timeToRun = TURN_LEN;

                StaticUIRefs.Instance.TurnIndicator.color = Color.green;

                orderPhase = false;
            }
        }
        //If Engage phase
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

                orderPhase = true;
            }
        }
    }

    private void EnableEngageSystems (bool enable)
    {
        World.Unmanaged.GetExistingSystemState<MovementSystem>().Enabled = enable;
        World.Unmanaged.GetExistingSystemState<TargetingAttackSystem>().Enabled = enable;
        World.Unmanaged.GetExistingSystemState<AttackSystem>().Enabled = enable;
    }
}
