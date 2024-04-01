using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class DeployableAuthoring : MonoBehaviour
{
    public float maxDeploy = 1;
    public float maxWaiting = 1;
    public class Baker : Baker<DeployableAuthoring>
    {
        public override void Bake(DeployableAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new Deployable
            {
                deployedState = false,
                deployTimeElapsed = 0,
                deployTime = authoring.maxDeploy,
                waitingTimeCur = authoring.maxWaiting,
                waitingTimeMax = authoring.maxWaiting
            });
        }
    }
}

[GhostComponent(PrefabType = GhostPrefabType.All)]
public struct Deployable : IComponentData
{
    /// <summary>
    /// Means if Unit is deployed/in proccess to become deployed OR undeployed/in proccess to become undeployed
    /// </summary>
    public bool deployedState;
    public float deployTimeElapsed;
    public float deployTime;
    public float waitingTimeMax;
    public float waitingTimeCur;
}
