using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class BuyStageAuthoring : MonoBehaviour
{
    public class Baker : Baker<BuyStageAuthoring>
    {
        public override void Bake(BuyStageAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent<BuyStageNotCompletedTag>(entity);
        }

    }
}
