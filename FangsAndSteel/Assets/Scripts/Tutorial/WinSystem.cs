using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public partial class WinSystem : SystemBase
{

    EntityQuery enemyQuery;

    protected override void OnCreate()
    {
        RequireForUpdate<TutorialTag>();

        enemyQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<TutorialEnemyTag>().Build(this);
    }

    protected override void OnStartRunning()
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach(var(team, entity) in SystemAPI.Query<TeamComponent>().WithEntityAccess())
        {
            if (team.teamInd == 2)
                ecb.AddComponent<TutorialEnemyTag>(entity);
        }
        ecb.Playback(EntityManager);
    }

    protected override void OnUpdate()
    {
        if (enemyQuery.IsEmpty)
        {
            ShowEnemyZone.TutorialUiInstance.WinPanel.SetActive(true);
            NewUnitUIManager.Instance.gameObject.SetActive(false);
            Enabled = false;
        }
    }
}


public struct TutorialEnemyTag :IComponentData{ }