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

    float topBorder;
    float bottomBorder;
    float leftBorder;
    float rightBorder;

    protected override void OnCreate()
    {
        RequireForUpdate<GameTag>();
        RequireForUpdate<BuyStageNotCompletedTag>();
        RequireForUpdate<NotBoughtYetTag>();

        notBoughtYetQuery = new EntityQueryBuilder(Allocator.Persistent).WithAll<NotBoughtYetTag>().Build(EntityManager);
    }

    protected override void OnStartRunning()
    {
        var temp = StaticUIRefs.Instance.BuyBorders.GetComponent<GetBuyBordersScript>().GetAllBorders(SystemAPI.GetSingleton<CurrentTeamComponent>().value);
        rightBorder = temp[0];
        topBorder = temp[1];
        bottomBorder = temp[2];
        leftBorder = temp[3];
        //Debug.Log(rightBorder);
        //Debug.Log(topBorder);
        //Debug.Log(bottomBorder);
        //Debug.Log(leftBorder);
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

        var updateNotBoughtYetUnitsPosJobHandle = new UpdateNotBoughtYetUnitsPosJob
        {
            raycastResult = raycastResult,

            rightBorder = rightBorder,
            topBorder = topBorder,
            bottomBorder = bottomBorder,
            leftBorder = leftBorder
        }.Schedule(targetRayCastJobHandle);

        Dependency = new PutAllNotBuyedOnTerrainJob
        {
            collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld
        }.Schedule(updateNotBoughtYetUnitsPosJobHandle);

    }
}

[WithAll(typeof(NotBoughtYetTag))]
public partial struct UpdateNotBoughtYetUnitsPosJob : IJobEntity 
{
    public NativeReference<RaycastResult> raycastResult;

    public float topBorder;
    public float bottomBorder;
    public float leftBorder;
    public float rightBorder;

    public void Execute(ref LocalTransform transform)
    {
        var result = raycastResult.Value;
        if (!result.hasHit)
            return;

        //Debug.Log(result.raycastHitInfo.Position);
        var newPos = new float3(
            math.clamp(result.raycastHitInfo.Position.x, leftBorder, rightBorder),
            result.raycastHitInfo.Position.y,
            math.clamp(result.raycastHitInfo.Position.z, bottomBorder, topBorder)
            );
        //Debug.Log(newPos);
        transform.Position = newPos;
        
    }
}

