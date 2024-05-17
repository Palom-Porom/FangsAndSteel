using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class PutOnPlaceBuyingUnitsSystem : SystemBase
{
    EntityQuery notBoughtYetQuery;

    float3 cameraPosition;
    float3 mouseTargetingPoint;

    protected override void OnCreate()
    {
        RequireForUpdate<GameTag>();
        RequireForUpdate<BuyStageNotCompletedTag>();
        RequireForUpdate<NotBoughtYetTag>();

        notBoughtYetQuery = new EntityQueryBuilder(Allocator.Persistent).WithAll<NotBoughtYetTag>().Build(EntityManager);
    }

    protected override void OnUpdate()
    {
        cameraPosition = Camera.main.transform.position;
        var ray = Camera.main.ScreenPointToRay(Mouse.current.position.value);
        mouseTargetingPoint = ray.GetPoint(1000f);

        RaycastInput raycastInput = new RaycastInput
        {
            Start = cameraPosition,
            End = mouseTargetingPoint,
            Filter = new CollisionFilter
            {
                BelongsTo = (uint)layers.Everything,
                CollidesWith = (uint)layers.Terrain,
                GroupIndex = 0
            }
        };

        NativeReference<RaycastResult> raycastResult = new NativeReference<RaycastResult>(Allocator.TempJob);

        JobHandle targetRayCastJobHandle = new TargetRayCastJob
        {
            collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld,
            raycastInput = raycastInput,
            raycastResult = raycastResult
        }.Schedule(Dependency);

        Dependency = new UpdateNotBoughtYetUnitsPosJob
        {
            raycastResult = raycastResult
        }.Schedule(targetRayCastJobHandle);

    }
}

[WithAll(typeof(NotBoughtYetTag))]
public partial struct UpdateNotBoughtYetUnitsPosJob : IJobEntity 
{
    public NativeReference<RaycastResult> raycastResult;

    public void Execute(ref LocalTransform transform)
    {
        var result = raycastResult.Value;
        if (!result.hasHit)
            return;

        //Debug.Log(result.raycastHitInfo.Position);
        transform.Position = result.raycastHitInfo.Position;
    }
}

