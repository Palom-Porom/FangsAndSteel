using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


public class CurrentTeamAuthoring : MonoBehaviour
{
    public int currentTeam;
  public class Baker : Baker<CurrentTeamAuthoring>
    {
        public override void Bake(CurrentTeamAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new CurrentTeamComponent { currentTeam = authoring.currentTeam });
        }

    }
}

public struct CurrentTeamComponent : IComponentData
{
    public int currentTeam;
}
