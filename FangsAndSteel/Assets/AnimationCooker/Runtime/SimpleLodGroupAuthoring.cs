// This authoring tool is used to author a simple-lod group
// It requires that the game-object also has a LODGroup component on it.

using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace AnimCooker
{

    [RequireComponent(typeof(LODGroup))]
    public class SimpleLodGroupAuthoring : MonoBehaviour
    {
        public Mesh Mesh0 = null;
        public Mesh Mesh1 = null;
        public Mesh Mesh2 = null;
        public Material Mat0 = null;
        public Material Mat1 = null;
        public Material Mat2 = null;
    }

    [System.Serializable]
    public class SimpleLodResourceData : IComponentData
    {
        public Mesh Mesh0;
        public Mesh Mesh1;
        public Mesh Mesh2;
        public Material Mat0;
        public Material Mat1;
        public Material Mat2;
    }

    public struct SimpleLodData : IComponentData
    {
        public BatchMeshID MeshId0;
        public BatchMeshID MeshId1;
        public BatchMeshID MeshId2;
        public BatchMaterialID MatId0;
        public BatchMaterialID MatId1;
        public BatchMaterialID MatId2;
    }

    public struct SimpleLodInfoData : IComponentData
    {
        public float WorldSpaceSize;
        public float ScreenRelativeTransitionHeight0;
        public float ScreenRelativeTransitionHeight1;
        public float ScreenRelativeTransitionHeight2;
    }

    // DISTANCE MODE
    //public struct LodDistanceData : IComponentData
    //{
    //    public float Dist0Sq;
    //    public float Dist1Sq;
    //    public float Dist2Sq;
    //}

    public class SimpleLodGroupBaker : Baker<SimpleLodGroupAuthoring>
    {
        public override void Bake(SimpleLodGroupAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

            // LOD resource data
            SimpleLodResourceData lodRes = new SimpleLodResourceData();
            lodRes.Mesh0 = authoring.Mesh0;
            lodRes.Mesh1 = authoring.Mesh1;
            lodRes.Mesh2 = authoring.Mesh2;
            lodRes.Mat0 = authoring.Mat0;
            lodRes.Mat1 = authoring.Mat1;
            lodRes.Mat2 = authoring.Mat2;
            AddComponentObject(entity, lodRes);

            // DISTANCE MODE
            //// add lod distance data - the initialization system will fill it.
            //AddComponent<LodDistanceData>(entity);

            // LOD info data
            SimpleLodInfoData lodInf = new SimpleLodInfoData();
            LODGroup group = GetComponent<LODGroup>();
            group.enabled = false;
            LOD[] lods = group.GetLODs();
            if (lods.Length >= 3) {
                lodInf.WorldSpaceSize = LODGroupExtensions.GetWorldSpaceSize(group);
                lodInf.ScreenRelativeTransitionHeight0 = lods[0].screenRelativeTransitionHeight;
                lodInf.ScreenRelativeTransitionHeight1 = lods[1].screenRelativeTransitionHeight;
                lodInf.ScreenRelativeTransitionHeight2 = lods[2].screenRelativeTransitionHeight;

                //float dist0 = LODGroupExtensions.CalculateLODSwitchDistance(authoring.FieldOfView, group, 0);
            }
            AddComponent(entity, lodInf);
        }
    }
} // namespace