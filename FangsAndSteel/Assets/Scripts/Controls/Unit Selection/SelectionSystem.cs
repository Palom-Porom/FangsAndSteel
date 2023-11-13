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
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public partial class SelectionSystem : SystemBase
{
    private float2 mouseStartPos;
    private bool isDragging;
    private bool wasClickedOnUI;
    private const float SQRD_DISTANCE_TO_DRAG = 0.25f;

    protected override void OnStartRunning()
    {
        new DeselectAllUnitsJob().Schedule();
    }

    protected override void OnUpdate()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            wasClickedOnUI = EventSystem.current.IsPointerOverGameObject();
            if (!wasClickedOnUI)
                mouseStartPos = Mouse.current.position.value;
        }

        if (Mouse.current.leftButton.isPressed)
        {
            if (!isDragging &&
                math.distancesq(mouseStartPos, Mouse.current.position.value) > SQRD_DISTANCE_TO_DRAG &&
                !wasClickedOnUI)
            {
                isDragging = true;
                GUI_Manager.Instance.isDragging = true;
                GUI_Manager.Instance.mouseStartPos = mouseStartPos;
            }
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            //If was clicked on UI, then nothing to do
            if (wasClickedOnUI)
            {
                wasClickedOnUI = false;
                return;
            }

            if (isDragging)
            {
                isDragging = false;
                GUI_Manager.Instance.isDragging = false;

                // Invert origin.y as in the GetRect it is inverted as well
                mouseStartPos.y = Screen.height - mouseStartPos.y;
                float2 curMousePosition = Mouse.current.position.value;
                curMousePosition.y = Screen.height - curMousePosition.y;

                EntityQuery entityQuery = GetEntityQuery(new EntityQueryBuilder(Allocator.TempJob).WithAll<SelectTag, LocalTransform>()
                                                        .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState));

                new MultipleSelectJob
                {
                    worldToScreen = WorldToScreen.Create(Camera.main),
                    rect = GUI_Utilities.GetScreenRect(mouseStartPos, curMousePosition)
                }.Schedule(entityQuery);
            }
            else
            {
                new DeselectAllUnitsJob().Schedule();

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
                    selectTagLookup = SystemAPI.GetComponentLookup<SelectTag>()
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
    public void Execute(EnabledRefRW<SelectTag> selectTag)
    {
        selectTag.ValueRW = false;
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

    public void Execute()
    {
        if (collisionWorld.CastRay(raycastInput, out RaycastHit raycastHit))
        {
            if (selectTagLookup.HasComponent(raycastHit.Entity))
            {
                selectTagLookup.SetComponentEnabled(raycastHit.Entity, true);
            }
        }
    }

}


[BurstCompile]
public partial struct MultipleSelectJob : IJobEntity
{
    public WorldToScreen worldToScreen;
    public Rect rect;

    public void Execute(EnabledRefRW<SelectTag> selectTag, in LocalTransform transform)
    {
        if (rect.Contains(transform.Position.WorldToScreenCoordinatesNative(worldToScreen)))
        {
            selectTag.ValueRW = true;
        }
        else
        {
            selectTag.ValueRW = false;
        }
    }
}
