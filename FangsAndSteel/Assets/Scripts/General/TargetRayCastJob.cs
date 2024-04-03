using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Physics;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;


/// <summary>
/// Raycasts with given RaycastInput and puts the result in the NativeReference<RaycastResult>
/// </summary>
[BurstCompile]
public partial struct TargetRayCastJob : IJob
{
    [ReadOnly]
    public CollisionWorld collisionWorld;
    public RaycastInput raycastInput;
    public NativeReference<RaycastResult> raycastResult;

    //[BurstCompile] - for jobs` execution method that is not needed, as this method is automatically bursted if Job is bursted
    public void Execute()
    {
        bool hasHitTemp = collisionWorld.CastRay(raycastInput, out RaycastHit raycastHit);
        raycastResult.Value = new RaycastResult
        {
            hasHit = hasHitTemp,
            raycastHitInfo = raycastHit
        };
    }
}

/// <summary>
/// Contains info about result of Raycast
/// </summary>
public struct RaycastResult
{
    public bool hasHit;
    public RaycastHit raycastHitInfo;
}
