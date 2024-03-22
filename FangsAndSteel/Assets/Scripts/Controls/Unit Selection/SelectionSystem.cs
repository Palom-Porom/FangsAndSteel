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
using Unity.Rendering;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(ControlsSystemGroup))]
public partial class SelectionSystem : SystemBase
{
    /// <summary>
    /// How much distance cursor need to move for dragging state
    /// </summary>
    private const float SQRD_DISTANCE_TO_DRAG = 0.25f;

    private float2 mouseStartPos;
    private bool isDragging;
    private bool wasClickedOnUI;

    private ComponentLookup<SelectTag> selectLookup;
    //private ComponentLookup<AttackSettingsComponent> attackSetsLookup;
    private ComponentLookup<TeamComponent> teamLookup;

    private EntityCommandBuffer ecb;

    /// <summary>
    /// All selected units
    /// </summary>
    private EntityQuery allSelected;
    /// <summary>
    /// All units that can be selected (and also have LocalTransform)
    /// </summary>
    private EntityQuery allSelectable;

    private Entity unitStatsRqstEntity;

    protected override void OnCreate()
    {
        RequireForUpdate<SelectTag>();

        selectLookup = GetComponentLookup<SelectTag>();
        //attackSetsLookup = GetComponentLookup<AttackSettingsComponent>();
        teamLookup = GetComponentLookup<TeamComponent>();
    }

    protected override void OnStartRunning()
    {
        selectLookup.Update(this);

        ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        allSelected = new EntityQueryBuilder(Allocator.TempJob).WithAll<SelectTag>().Build(this);
        allSelectable = new EntityQueryBuilder(Allocator.TempJob).WithAll<SelectTag, LocalTransform, TeamComponent>().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState).Build(this);

        if (!SystemAPI.TryGetSingletonEntity<UnitStatsRequestTag>(out unitStatsRqstEntity))
            unitStatsRqstEntity = Entity.Null;
        new DeselectAllUnitsJob 
        { 
            ecb = ecb.AsParallelWriter(),
            selectLookup = selectLookup,
            unitStatsRqstEntity = unitStatsRqstEntity
        }.Schedule(allSelected);
    }

    protected override void OnUpdate()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            wasClickedOnUI = EventSystem.current.IsPointerOverGameObject();
            if (!wasClickedOnUI)
                mouseStartPos = Mouse.current.position.value;
        }

        else if (Mouse.current.leftButton.isPressed)
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

        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            //If was clicked on UI, then nothing to do
            if (wasClickedOnUI)
            {
                wasClickedOnUI = false;
                return;
            }
            //Update containers
            selectLookup.Update(this);
            //attackSetsLookup.Update(this);
            teamLookup.Update(this);
            ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            if (!SystemAPI.TryGetSingletonEntity<UnitStatsRequestTag>(out unitStatsRqstEntity))
                unitStatsRqstEntity = Entity.Null;

            if (isDragging)
            {
                isDragging = false;
                GUI_Manager.Instance.isDragging = false;

                // Invert origin.y as in the GetRect it is inverted as well
                mouseStartPos.y = Screen.height - mouseStartPos.y;
                float2 curMousePosition = Mouse.current.position.value;
                curMousePosition.y = Screen.height - curMousePosition.y;

                //removing stats for single unit selection
                if (unitStatsRqstEntity != Entity.Null)
                    ecb.RemoveComponent<UnitStatsRequestTag>(unitStatsRqstEntity);

                //Change ShootMode color to the neutral
                Entity colorRqstEntity = ecb.CreateEntity();
                ecb.AddComponent(colorRqstEntity, new ShootModeButChangeColorRqst { color = Color.white });

                new MultipleSelectJob
                {
                    worldToScreen = WorldToScreen.Create(Camera.main),
                    rect = GUI_Utilities.GetScreenRect(mouseStartPos, curMousePosition),

                    selectTagLookup = selectLookup,
                    ecb = ecb.AsParallelWriter()
                }.Schedule(allSelectable);
            }

            else
            {
                new DeselectAllUnitsJob 
                { 
                    ecb = ecb.AsParallelWriter(),
                    selectLookup = selectLookup,
                    unitStatsRqstEntity = unitStatsRqstEntity
                }.Schedule(allSelected);

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
                    //attackSetsLookup = attackSetsLookup,
                    ecb = ecb,
                    teamLookup = teamLookup
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

    public Entity unitStatsRqstEntity;
    public void Execute(Entity entity, [ChunkIndexInQuery] int chunkIndexInQuery)
    {
        selectLookup.SetComponentEnabled(entity, false);
        ecb.AddComponent<DisableRendering>(chunkIndexInQuery, selectLookup[entity].selectionRing);

        //hiding stats for single unit selection
        if (unitStatsRqstEntity != Entity.Null)
            ecb.RemoveComponent<UnitStatsRequestTag>(chunkIndexInQuery, unitStatsRqstEntity);
    }
}

/// <summary>
/// Raycasts with the given RaycastInput and enables a SelectionTag if Selectable is hit
/// </summary>
[BurstCompile]
public partial struct SingleSelectJob : IJob
{

    [ReadOnly] public CollisionWorld collisionWorld;
    public RaycastInput raycastInput;
    public ComponentLookup<SelectTag> selectTagLookup;
    //public ComponentLookup<BattleModeComponent> attackSetsLookup;

    public EntityCommandBuffer ecb;
    public ComponentLookup<TeamComponent> teamLookup;

    public void Execute()
    {
        if (collisionWorld.CastRay(raycastInput, out RaycastHit raycastHit))
        {
            if (selectTagLookup.HasComponent(raycastHit.Entity) && teamLookup[raycastHit.Entity].teamInd == 1)
            {
                selectTagLookup.SetComponentEnabled(raycastHit.Entity, true);
                ecb.RemoveComponent<DisableRendering>(selectTagLookup[raycastHit.Entity].selectionRing);

                //showing stats for single unit selection
                //Entity unitStatsRqstEntity = ecb.CreateEntity();
                ecb.AddComponent(raycastHit.Entity, new UnitStatsRequestTag());

                //Change ShootMode color depending on the state of that variable
                //Entity entity = ecb.CreateEntity();
                //if (attackSetsLookup[raycastHit.Entity].shootingOnMoveMode)
                //    ecb.AddComponent(entity, new ShootModeButChangeColorRqst { color = Color.green});
                //else
                //    ecb.AddComponent(entity, new ShootModeButChangeColorRqst { color = Color.red});

            }
            else
            {
                //Change ShootMode color to the neutral
                Entity colorRqstEntity = ecb.CreateEntity();
                ecb.AddComponent(colorRqstEntity, new ShootModeButChangeColorRqst { color = Color.white });
            }
        }
    }

}


/// <summary>
/// Selects all units, which are inside of the rect (which is in the screen coordinates)
/// All other units are deselected
/// </summary>
[BurstCompile]
public partial struct MultipleSelectJob : IJobEntity
{
    public WorldToScreen worldToScreen;
    public Rect rect;

    public ComponentLookup<SelectTag> selectTagLookup;
    public EntityCommandBuffer.ParallelWriter ecb;

    public void Execute(Entity entity, in LocalTransform transform, [ChunkIndexInQuery] int chunkIndexInQuery, in TeamComponent team)
    {
        if (rect.Contains(transform.Position.WorldToScreenCoordinatesNative(worldToScreen)) && team.teamInd == 1)
        {
            selectTagLookup.SetComponentEnabled(entity, true);
            ecb.RemoveComponent<DisableRendering>(chunkIndexInQuery, selectTagLookup[entity].selectionRing);
        }
        else
        {
            selectTagLookup.SetComponentEnabled(entity, false);
            ecb.AddComponent<DisableRendering>(chunkIndexInQuery, selectTagLookup[entity].selectionRing);
        }
    }
}