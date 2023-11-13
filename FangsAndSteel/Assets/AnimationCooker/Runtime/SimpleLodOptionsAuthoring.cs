// Put this on a singleton in the scene to edit LOD options.

using Unity.Entities;
using UnityEngine;

namespace AnimCooker
{
    public struct SimpleLodOptsData : IComponentData
    {
        public float TimerInterval;
    }

    public class SimpleLodOptionsAuthoring : MonoBehaviour
    {
        [Tooltip("The interval at which to run the LOD System. Use 0 for 'every frame'.")]
        public float TimerInterval = 0.25f;
    }

    public class SimpleLodOptionsBaker : Baker<SimpleLodOptionsAuthoring>
    {
        public override void Bake(SimpleLodOptionsAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent(entity, new SimpleLodOptsData { TimerInterval = authoring.TimerInterval });
        }
    }
} // namespace