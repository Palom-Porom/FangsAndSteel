using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using Unity.Collections;
using UnityEngine.InputSystem;
using RaycastHit = Unity.Physics.RaycastHit;
using Unity.Transforms;
using Unity.Rendering;

public partial class SelectionSystem : SystemBase
{
    private float2 mouseStartPos;
    private bool isDragging;
    private const float SQRD_DISTANCE_TO_DRAG = 0.25f;

    EntityCommandBuffer.ParallelWriter ecb;
    ComponentLookup<SelectTag> selectLookup;

    protected override void OnStartRunning()
    {
        ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged).AsParallelWriter();
        selectLookup = GetComponentLookup<SelectTag>();
        EntityQuery entityQuery = new EntityQueryBuilder(Allocator.TempJob).WithAll<SelectTag>().Build(this);
        new DeselectAllUnitsJob { ecb = ecb, selectLookup = selectLookup }.Schedule(entityQuery);
    }

    protected override void OnUpdate()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            mouseStartPos = Mouse.current.position.value;
        }

        if (Mouse.current.leftButton.isPressed)
        {
            if (!isDragging && math.distancesq(mouseStartPos, Mouse.current.position.value) > SQRD_DISTANCE_TO_DRAG)
            {
                isDragging = true;
                GUI_Manager.Instance.isDragging = true;
                GUI_Manager.Instance.mouseStartPos = mouseStartPos;
            }
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged).AsParallelWriter();
            selectLookup.Update(this);
            if (isDragging)
            {
                isDragging = false;
                GUI_Manager.Instance.isDragging = false;

                // Invert origin.y as in the GetRect it is inverted as well
                mouseStartPos.y = Screen.height - mouseStartPos.y;
                float2 curMousePosition = Mouse.current.position.value;
                curMousePosition.y = Screen.height - curMousePosition.y;

                EntityQuery entityQuery = new EntityQueryBuilder(Allocator.TempJob).WithAll<SelectTag, LocalTransform>()
                                                        .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState).Build(this);

                new MultipleSelectJob
                {
                    worldToScreen = WorldToScreen.Create(Camera.main),
                    rect = GUI_Utilities.GetScreenRect(mouseStartPos, curMousePosition),
                    ecb = ecb,
                    selectTagLookup = selectLookup,
                }.Schedule(entityQuery);
            }
            else
            {
                EntityQuery entityQuery = new EntityQueryBuilder(Allocator.TempJob).WithAll<SelectTag>().Build(this);

                new DeselectAllUnitsJob { ecb = ecb, selectLookup = selectLookup }.Schedule(entityQuery);

                var ray = Camera.main.ScreenPointToRay(Mouse.current.position.value);
                RaycastInput raycastInput = new RaycastInput
                {
                    Start = ray.origin,
                    End = ray.GetPoint(1000f),
                    Filter = new CollisionFilter
                    {
                        BelongsTo = (uint)layers.Everything,
                        CollidesWith = (uint)(layers.Selectable | layers.Terrain),
                        GroupIndex = 0
                    }
                };

                Dependency = new SingleSelectJob
                {
                    collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld,
                    raycastInput = raycastInput,
                    selectTagLookup = selectLookup,
                    ecb = ecb
                }.Schedule(Dependency);
            }
        }
    }

    
}


/// <summary>
/// Disable all SelectTag-s (as they are IEnableable-s)
/// </summary>
[BurstCompile]
public partial struct DeselectAllUnitsJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ecb;
    public ComponentLookup<SelectTag> selectLookup;
    public void Execute(Entity entity, [ChunkIndexInQuery] int chunkIndexInQuery)
    {
        ecb.AddComponent<DisableRendering>(chunkIndexInQuery, selectLookup[entity].selectionRing);
        selectLookup.SetComponentEnabled(entity, false);
    }
}

/// <summary>
/// Raycasts with the given RaycastInput and enables a SelectionTag if selectable is hit. 
/// Deselects all units (even if something unselectable was raycast-ed)
/// </summary>
[BurstCompile]
public partial struct SingleSelectJob : IJob
{

    [ReadOnly] public CollisionWorld collisionWorld;
    public RaycastInput raycastInput;
    public ComponentLookup<SelectTag> selectTagLookup;

    public EntityCommandBuffer.ParallelWriter ecb;

    public void Execute()
    {
        if (collisionWorld.CastRay(raycastInput, out RaycastHit raycastHit))
        {
            if (selectTagLookup.HasComponent(raycastHit.Entity))
            {
                selectTagLookup.SetComponentEnabled(raycastHit.Entity, true);
                ecb.RemoveComponent<DisableRendering>(0, selectTagLookup[raycastHit.Entity].selectionRing);
            }
        }
    }

}


[BurstCompile]
public partial struct MultipleSelectJob : IJobEntity
{
    public WorldToScreen worldToScreen;
    public Rect rect;

    public ComponentLookup<SelectTag> selectTagLookup;
    public EntityCommandBuffer.ParallelWriter ecb;

    public void Execute(Entity entity, in LocalTransform transform, [ChunkIndexInQuery] int chunkIndexInQuery)
    {
        if (rect.Contains(transform.Position.WorldToScreenCoordinatesNative(worldToScreen)))
        {
            selectTagLookup.SetComponentEnabled(entity, true);
            ecb.RemoveComponent<DisableRendering>(chunkIndexInQuery, selectTagLookup[entity].selectionRing);
        }
        else
        {
            ecb.AddComponent<DisableRendering>(chunkIndexInQuery, selectTagLookup[entity].selectionRing);
            selectTagLookup.SetComponentEnabled(entity, false);
        }
    }
}
