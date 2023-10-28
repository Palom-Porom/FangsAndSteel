using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


public class TeamComponentAuthoring : MonoBehaviour
{
    public int teamInd = 0;
    public class Baker : Baker<TeamComponentAuthoring>
    {
        public override void Bake(TeamComponentAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new TeamComponent { teamInd = authoring.teamInd});
        }
    }
}
public struct TeamComponent : IComponentData 
{
    public int teamInd;
}
