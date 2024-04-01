using AnimCooker;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


//public partial class ReplaySystem : SystemBase
//{
//    StaticUIData uiData;
//    const float TURN_LEN = 10;
//    float timeElapsed;

//    bool started;

//    protected override void OnCreate()
//    {
//        RequireForUpdate<Entity>();
//    }

//    protected override void OnStartRunning()
//    {
//        EntityManager.CreateEntity(typeof(StaticUIData));
//        started = false;
//    }

//    protected override void OnStopRunning()
//    {
//        World.DefaultGameObjectInjectionWorld.EntityManager.DestroyEntity(new EntityQueryBuilder().WithAll<IsReplayActive>().Build(this));
//    }

//    protected override void OnUpdate()
//    {
//        if (started)
//        {
//            timeElapsed -= SystemAPI.Time.DeltaTime;
//            StaticUIRefs.Instance.TurnTimer.text = $"0:{(int)timeElapsed:D2}";
//            if (timeElapsed <= 0)
//            {
//                StaticUIRefs.Instance.TurnTimer.text = "0:00";
//                //Stop all Engage systems
//                EnableEngageSystems(false);

//                //StaticUIRefs.Instance.TurnIndicator.color = Color.red;

//                EntityManager.DestroyAndResetAllEntities();
//            }
//        }
//        else
//        {
//            uiData = SystemAPI.GetSingleton<StaticUIData>();
//            if (uiData.newTurnStartBut)
//            {
//                StaticUIRefs.Instance.NewTurnPanel.SetActive(false);
//                started = true;
//            }
//        }
//    }

//    private void EnableEngageSystems(bool enable)
//    {
//        World.Unmanaged.GetExistingSystemState<UnitsSystemGroup>().Enabled = enable;
//        //World.Unmanaged.GetExistingSystemState<VisionMapSystem>().Enabled = enable;
//        World.Unmanaged.GetExistingSystemState<AnimationSystem>().Enabled = enable;

//        World.Unmanaged.GetExistingSystemState<TargetingMoveSystem>().Enabled = !enable;
//        World.Unmanaged.GetExistingSystemState<BasicButtonSystem>().Enabled = !enable;
//    }
//}

