// This system has one simple function - every frame it copies data from the camera
// into a singleton that can be used by other systems to fetch the current camera position in jobs.

using Unity.Entities;
using Unity.Mathematics;

namespace AnimCooker
{

    public struct CamData : IComponentData
    {
        public float3 Pos;
        public float FovRad;
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class CameraDataSystem : SystemBase
    {
        UnityEngine.Camera m_cam = UnityEngine.Camera.main;

        // DISTANCE MODE
        //float m_oldFov = 0f;

        protected override void OnCreate()
        {
            EntityManager.CreateEntity(typeof(CamData));
        }

        protected override void OnUpdate()
        {
            if (m_cam == null) { m_cam = UnityEngine.Camera.main; }
            if (m_cam == null) { return; }
            CamData camData = new CamData { Pos = m_cam.transform.position, FovRad = math.radians(m_cam.fieldOfView) };
            SystemAPI.SetSingleton(camData);

            // DISTANCE MODE
            //if (m_cam.fieldOfView != m_oldFov) {
            //    m_oldFov = m_cam.fieldOfView;
            //    UpdateDistancesJob job = new UpdateDistancesJob();
            //    job.Cam = camData;
            //    Dependency = job.ScheduleParallel(Dependency);
            //}
        }
    }

    // DISTANCE MODE
    //// This only runs whenever the FOV is changing.
    //[BurstCompile]
    //public partial struct UpdateDistancesJob : IJobEntity
    //{
    //    public CamData Cam;

    //    [BurstCompile]
    //    public void Execute()
    //    {
    // todo - update distances for all entities
    //    }
    //}

} // namespace