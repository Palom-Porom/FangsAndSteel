// The purpose of this system is to run once for every entity to
// initialize it with a SimpleLodData component.
// It registers meshes & materials, adds a component, and removes unnecessary components.
//
// It runs in the main thread without burst because it uses entitymanager
// to add a a component and it also accesses a managed system.

using Unity.Entities;
using Unity.Rendering;

namespace AnimCooker
{

    [RequireMatchingQueriesForUpdate]
    public partial class SimpleLodInitSystem : SystemBase
    {
        EntitiesGraphicsSystem m_hybridRenderer;

        protected override void OnCreate()
        {
            m_hybridRenderer = World.GetExistingSystemManaged<EntitiesGraphicsSystem>();
        }

        protected override void OnUpdate()
        {
            Entities.WithNone<SimpleLodData>().ForEach((Entity entity, SimpleLodResourceData resource) =>
            {
                // Register the meshes and add them to a component on the entity.
                // Note - registering meshes does not put them in the rendermesh-array.
                // They get stored in some other place... i dunno where.
                SimpleLodData lod = new SimpleLodData();
                lod.MatId0 = m_hybridRenderer.RegisterMaterial(resource.Mat0);
                lod.MatId1 = m_hybridRenderer.RegisterMaterial(resource.Mat1);
                lod.MatId2 = m_hybridRenderer.RegisterMaterial(resource.Mat2);
                lod.MeshId0 = m_hybridRenderer.RegisterMesh(resource.Mesh0);
                lod.MeshId1 = m_hybridRenderer.RegisterMesh(resource.Mesh1);
                lod.MeshId2 = m_hybridRenderer.RegisterMesh(resource.Mesh2);

                // ensure this never runs again for this entity
                EntityManager.AddComponentData(entity, lod);

                // these components are no longer needed.
                EntityManager.RemoveComponent<SimpleLodResourceData>(entity);
                if (EntityManager.HasComponent<MeshLODGroupComponent>(entity)) {
                    EntityManager.RemoveComponent<MeshLODGroupComponent>(entity);
                }
                // can't remove this because it's internal, but i want to remove it.
                //EntityManager.RemoveComponent<LODGroupWorldReferencePoint>(entity);
            }).WithStructuralChanges().WithoutBurst().Run();
        }


        // DISTANCE MODE
        //protected override void OnUpdate()
        //{
        //    Entities.WithNone<SimpleLodData>().ForEach((Entity entity, SimpleLodResourceData resource, ref LodDistanceData dist, in MeshLODGroupComponent group) =>
        //    {
        //        // Register the meshes and add them to a component on the entity.
        //        // Note - registering meshes does not put them in the rendermesh-array.
        //        // They get stored in some other place... i dunno where.
        //        SimpleLodData lod = new SimpleLodData();
        //        lod.MatId0 = m_hybridRenderer.RegisterMaterial(resource.Mat0);
        //        lod.MatId1 = m_hybridRenderer.RegisterMaterial(resource.Mat1);
        //        lod.MatId2 = m_hybridRenderer.RegisterMaterial(resource.Mat2);
        //        lod.MeshId0 = m_hybridRenderer.RegisterMesh(resource.Mesh0);
        //        lod.MeshId1 = m_hybridRenderer.RegisterMesh(resource.Mesh1);
        //        lod.MeshId2 = m_hybridRenderer.RegisterMesh(resource.Mesh2);

        //        dist.Dist0Sq = group.LODDistances0.x * group.LODDistances0.x;
        //        dist.Dist1Sq = group.LODDistances0.y * group.LODDistances0.y;
        //        dist.Dist2Sq = group.LODDistances0.z * group.LODDistances0.z;

        //        // ensure this never runs again for this entity
        //        EntityManager.AddComponentData(entity, lod);
        //        // thes components are no longer needed.
        //        EntityManager.RemoveComponent<SimpleLodResourceData>(entity);
        //        EntityManager.RemoveComponent<MeshLODGroupComponent>(entity);
        //        //EntityManager.RemoveComponent<LODGroupWorldReferencePoint>(entity);
        //    }).WithStructuralChanges().WithoutBurst().Run();
        //}

        //static float GetWorldSpaceScale(Transform t)
        //{
        //    var scale = t.lossyScale;
        //    float largestAxis = Mathf.Abs(scale.x);
        //    largestAxis = Mathf.Max(largestAxis, Mathf.Abs(scale.y));
        //    largestAxis = Mathf.Max(largestAxis, Mathf.Abs(scale.z));
        //    return largestAxis;
        //}
    }
} // namespace