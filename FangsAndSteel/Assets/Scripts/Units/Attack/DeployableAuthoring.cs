using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
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
                deployTimeCur = 0,
                deployTimeMax = authoring.maxDeploy,
                waitingTimeCur = authoring.maxWaiting,
                waitingTimeMax = authoring.maxWaiting
            });
        }
    }
}

public struct Deployable : IComponentData
{
    /// <summary>
    /// Means if Unit is deployed/in proccess to become deployed OR undeployed/in proccess to become undeployed
    /// </summary>
    public bool deployedState;
    public float deployTimeCur;
    public float deployTimeMax;
    public float waitingTimeMax;
    public float waitingTimeCur;
}
