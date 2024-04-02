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
using UnityEngine.UI;
using TMPro;

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

    static public bool needUpdateUIPanelInfo;

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
        Dependency = new DeselectAllUnitsJob 
        { 
            ecb = ecb.AsParallelWriter(),
            selectLookup = selectLookup,
            unitStatsRqstEntity = unitStatsRqstEntity
        }.Schedule(allSelected, Dependency);
        Dependency.Complete();

        needUpdateUIPanelInfo = true;
    }

    protected override void OnUpdate()
    {
        allSelected.CompleteDependency();
        EntityManager.CompleteDependencyBeforeRO<SelectTag>();
        if (needUpdateUIPanelInfo) { UpdateUIPanelInfo(); }


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

            int curTeam = SystemAPI.GetSingleton<CurrentTeamComponent>().value;

            needUpdateUIPanelInfo = true;

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
                //ecb.AddComponent(colorRqstEntity, new ShootModeButChangeColorRqst { color = Color.white });

                new MultipleSelectJob
                {
                    worldToScreen = WorldToScreen.Create(Camera.main),
                    rect = GUI_Utilities.GetScreenRect(mouseStartPos, curMousePosition),

                    selectTagLookup = selectLookup,
                    ecb = ecb.AsParallelWriter(),
                    curTeam = curTeam
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
                    teamLookup = teamLookup,
                    curTeam = curTeam
                }.Schedule(Dependency);
            }
        }
    }

    private void UpdateUIPanelInfo()
    {
        needUpdateUIPanelInfo = false;
        StaticUIRefs.Instance.UnitsUI.SetActive(!allSelected.IsEmpty);
        if (allSelected.IsEmpty) return; 

        bool isMultipleSelected = (allSelected.ToEntityArray(Allocator.Temp).Length > 1);

        //List<(HashSet<bool>, Image)> diffButs = new List<(HashSet<bool>, Image)>
        //{
        //    (new HashSet<bool>(), NewUnitUIManager.Instance.ShootOnMoveButton),
        //    (new HashSet<bool>(), NewUnitUIManager.Instance.ShootOffButton)
        //};
        HashSet<bool> diffVals_shootOnMove = new HashSet<bool>();
        HashSet<bool> diffVals_shootOff = new HashSet<bool>();
        HashSet<bool> diffVals_autoTrigger = new HashSet<bool>();

        HashSet<bool> diffVals_baseInfBut = new HashSet<bool>();
        HashSet<bool> diffVals_machineGunnerBut = new HashSet<bool>();
        HashSet<bool> diffVals_antyTankBut = new HashSet<bool>();
        HashSet<bool> diffVals_tankBut = new HashSet<bool>();
        HashSet<bool> diffVals_artilleryBut = new HashSet<bool>();

        HashSet<float> diffVals_pursuiteStartRadius = new HashSet<float>();
        HashSet<float> diffVals_pursuiteMaxHp = new HashSet<float>();
        HashSet<float> diffVals_pursuiteEndRadius = new HashSet<float>();
        HashSet<float> diffVals_pursuiteMaxAttackDist = new HashSet<float>();
        HashSet<float> diffVals_pursuiteTimeForEnd = new HashSet<float>();

        //Debug.Log(new EntityQueryBuilder(Allocator.Temp).WithAll<BattleModeComponent, PursuingModeComponent>().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState).Build(this).ToEntityArray(Allocator.Temp).Length);
        //Debug.Log(new EntityQueryBuilder(Allocator.Temp).WithAll<BattleModeComponent, PursuingModeComponent, SelectTag>().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState).Build(this).ToEntityArray(Allocator.Temp).Length);
        //Debug.Log(new EntityQueryBuilder(Allocator.Temp).WithAll<BattleModeComponent, SelectTag>().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState).Build(this).ToEntityArray(Allocator.Temp).Length);
        ////Debug.Log(new EntityQueryBuilder(Allocator.Temp).WithAll<BattleModeComponent, SelectTag>().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState).Build(this).ToEntityArray(Allocator.Temp).Length);
        //Debug.Log(new EntityQueryBuilder(Allocator.Temp).WithAll<BattleModeComponent, SelectTag>().Build(this).ToEntityArray(Allocator.Temp).Length);
        //Debug.Log(new EntityQueryBuilder(Allocator.Temp).WithAll<BattleModeComponent, PursuingModeComponent, SelectTag>().Build(this).ToEntityArray(Allocator.Temp).Length);
        //Debug.Log(new EntityQueryBuilder(Allocator.Temp).WithAll<PursuingModeComponent, SelectTag>().Build(this).ToEntityArray(Allocator.Temp).Length);

        

        foreach (var (battleModeSets, pursueModeSets, selectTag_EnabledRO) 
            in SystemAPI.Query<BattleModeComponent, PursuingModeComponent, EnabledRefRO<SelectTag>>().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
        {
            //Debug.Log($"Entered foreach; SelectEnabled = {selectTag_EnabledRO.ValueRO}");
            if (!selectTag_EnabledRO.ValueRO) continue;

            NewUnitUIManager.Instance.ShootOnMoveButton.color = battleModeSets.shootingOnMove ? Color.green : Color.red;
            diffVals_shootOnMove.Add(battleModeSets.shootingOnMove);

            NewUnitUIManager.Instance.ShootOffButton.color = battleModeSets.shootingDisabled ? Color.green : Color.red;
            diffVals_shootOff.Add(battleModeSets.shootingDisabled);

            NewUnitUIManager.Instance.AutoPursuitButton.color = battleModeSets.isAutoTrigger ? Color.green : Color.red;
            diffVals_autoTrigger.Add(battleModeSets.isAutoTrigger);
            AutoPursuitButtonPush autoPursuitButtonPushScript = NewUnitUIManager.Instance.AutoPursuitButton.GetComponent<AutoPursuitButtonPush>();
            autoPursuitButtonPushScript.HidePursuitPanelPanel.SetActive(!battleModeSets.isAutoTrigger);

            #region Pursuite Units Priorities update
            
            NewUnitUIManager.Instance.PursuiteInfantryBut.color = (battleModeSets.autoTriggerUnitTypes & (uint)UnitTypes.BaseInfantry) != 0 ? Color.green : Color.red;
            diffVals_baseInfBut.Add((battleModeSets.autoTriggerUnitTypes & (uint)UnitTypes.BaseInfantry) != 0);
            NewUnitUIManager.Instance.PursuiteMachineGunnerBut.color = (battleModeSets.autoTriggerUnitTypes & (uint)UnitTypes.MachineGunner) != 0 ? Color.green : Color.red;
            diffVals_machineGunnerBut.Add((battleModeSets.autoTriggerUnitTypes & (uint)UnitTypes.MachineGunner) != 0);
            NewUnitUIManager.Instance.PursuiteAntyTankBut.color = (battleModeSets.autoTriggerUnitTypes & (uint)UnitTypes.AntyTank) != 0 ? Color.green : Color.red;
            diffVals_antyTankBut.Add((battleModeSets.autoTriggerUnitTypes & (uint)UnitTypes.AntyTank) != 0);
            NewUnitUIManager.Instance.PursuiteTankBut.color = (battleModeSets.autoTriggerUnitTypes & (uint)UnitTypes.Tank) != 0 ? Color.green : Color.red;
            diffVals_tankBut.Add((battleModeSets.autoTriggerUnitTypes & (uint)UnitTypes.Tank) != 0);
            NewUnitUIManager.Instance.PursuiteArtilleryBut.color = (battleModeSets.autoTriggerUnitTypes & (uint)UnitTypes.Artillery) != 0 ? Color.green : Color.red;
            diffVals_artilleryBut.Add((battleModeSets.autoTriggerUnitTypes & (uint)UnitTypes.Artillery) != 0);
            //Debug.Log($"tankMultiplier = {battleModeSets.autoTriggerUnitTypes & (uint)UnitTypes.Tank}");

            #endregion

            #region Sliders

            {
                float pursuiteTmpVal = (math.sqrt(battleModeSets.autoTriggerRadiusSq)) / 200f;
                NewUnitUIManager.Instance.PursuiteStartRadiusSlider.GetComponent<RadiusSliderControllerScript>().isCosmeticChangeOfValue = true;
                NewUnitUIManager.Instance.PursuiteStartRadiusSlider.value = pursuiteTmpVal;
                diffVals_pursuiteStartRadius.Add(pursuiteTmpVal);
                //NewUnitUIManager.Instance.PursuiteRadiusSliderText.text = ((int)pursuiteRadVal * 200).ToString();
            }

            {
                float pursuiteTmpVal = battleModeSets.autoTriggerMaxHpPercent / 100f;
                NewUnitUIManager.Instance.PursuiteMaxHpSlider.GetComponent<MaxHpForPursuitSliderControllerScript>().isCosmeticChangeOfValue = true;
                NewUnitUIManager.Instance.PursuiteMaxHpSlider.value = pursuiteTmpVal;
                diffVals_pursuiteMaxHp.Add(pursuiteTmpVal);
                //NewUnitUIManager.Instance.PursuiteRadiusSliderText.text = ((int)pursuiteRadVal * 200).ToString();
            }

            {
                float pursuiteTmpVal = (math.sqrt(pursueModeSets.maxShootDistanceSq)) / 400f;
                NewUnitUIManager.Instance.PursuiteMaxAttackDistSlider.GetComponent<AttackDistSliderControllerScript>().isCosmeticChangeOfValue = true;
                NewUnitUIManager.Instance.PursuiteMaxAttackDistSlider.value = pursuiteTmpVal;
                diffVals_pursuiteMaxAttackDist.Add(pursuiteTmpVal);
                //NewUnitUIManager.Instance.PursuiteRadiusSliderText.text = ((int)pursuiteRadVal * 200).ToString();
            }

            {
                float pursuiteTmpVal = (math.sqrt(pursueModeSets.dropDistanceSq)) / 300f;
                NewUnitUIManager.Instance.PursuiteEndRadiusSlider.GetComponent<RadiusEndSliderControllerScript>().isCosmeticChangeOfValue = true;
                NewUnitUIManager.Instance.PursuiteEndRadiusSlider.value = pursuiteTmpVal;
                diffVals_pursuiteEndRadius.Add(pursuiteTmpVal);
                //NewUnitUIManager.Instance.PursuiteRadiusSliderText.text = ((int)pursuiteRadVal * 200).ToString();
            }

            #endregion

            NewUnitUIManager.Instance.PursuiteTimeForEndField.GetComponent<TimeForEndPursuitInput>().isCosmeticChangeOfValue = true;
            NewUnitUIManager.Instance.PursuiteTimeForEndField.text = ((int)pursueModeSets.dropTime).ToString();
            diffVals_pursuiteTimeForEnd.Add((int)pursueModeSets.dropTime);
        }

        //int i = 0;
        //foreach (var (battleModeSets, pursueModeSets, selectTag_EnabledRO)
        //    in SystemAPI.Query<BattleModeComponent, PursuingModeComponent, EnabledRefRO<SelectTag>>().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
        //{
        //    i++;
        //    Debug.Log(i);
        //}
        //Debug.Log(i);

        if (diffVals_shootOnMove.Count > 1)
        {
            NewUnitUIManager.Instance.ShootOnMoveButton.color = Color.white;
            NewUnitUIManager.Instance.ShootOnMoveButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
        }
        else
            NewUnitUIManager.Instance.ShootOnMoveButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;

        if (diffVals_shootOff.Count > 1)
        {
            NewUnitUIManager.Instance.ShootOffButton.color = Color.white;
            NewUnitUIManager.Instance.ShootOffButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
        }
        else
            NewUnitUIManager.Instance.ShootOffButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;

        
        if (diffVals_autoTrigger.Count > 1)
        {
            NewUnitUIManager.Instance.AutoPursuitButton.color = Color.white;
            NewUnitUIManager.Instance.AutoPursuitButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
            AutoPursuitButtonPush autoPursuitButtonPushScript = NewUnitUIManager.Instance.AutoPursuitButton.GetComponent<AutoPursuitButtonPush>();
            autoPursuitButtonPushScript.HidePursuitPanelPanel.SetActive(true);
        }
        else
            NewUnitUIManager.Instance.AutoPursuitButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;

        NewUnitUIManager.Instance.EnemyListBut.color = Color.red;
        NewUnitUIManager.Instance.EnemyListBut.GetComponent<EnemyListButtonScript>().EnemyListPanel.SetActive(false);

        #region Pursuite Units Priorities White update

        if (diffVals_baseInfBut.Count > 1)
        {
            NewUnitUIManager.Instance.PursuiteInfantryBut.color = Color.white;
        }
        if (diffVals_machineGunnerBut.Count > 1)
        {
            NewUnitUIManager.Instance.PursuiteMachineGunnerBut.color = Color.white;
        }
        if (diffVals_antyTankBut.Count > 1)
        {
            NewUnitUIManager.Instance.PursuiteAntyTankBut.color = Color.white;
        }
        if (diffVals_tankBut.Count > 1)
        {
            NewUnitUIManager.Instance.PursuiteTankBut.color = Color.white;
        }
        if (diffVals_artilleryBut.Count > 1)
        {
            NewUnitUIManager.Instance.PursuiteArtilleryBut.color = Color.white;
        }
        //NewUnitUIManager.Instance.PursuiteMachineGunnerBut.color = (battleModeSets.autoTriggerUnitTypes & (uint)UnitTypes.MachineGunner) != 0 ? Color.green : Color.red;
        //NewUnitUIManager.Instance.PursuiteAntyTankBut.color = (battleModeSets.autoTriggerUnitTypes & (uint)UnitTypes.AntyTank) != 0 ? Color.green : Color.red;
        //NewUnitUIManager.Instance.PursuiteTankBut.color = (battleModeSets.autoTriggerUnitTypes & (uint)UnitTypes.Tank) != 0 ? Color.green : Color.red;
        //NewUnitUIManager.Instance.PursuiteArtilleryBut.color = (battleModeSets.autoTriggerUnitTypes & (uint)UnitTypes.Artillery) != 0 ? Color.green : Color.red;

        #endregion

        #region Sliders

        if (diffVals_pursuiteStartRadius.Count > 1)
        {
            //Debug.Log("Entered");
            //NewUnitUIManager.Instance.PursuiteRadiusSlider.value = 0;
            NewUnitUIManager.Instance.PursuiteStartRadiusSliderText.text = "Разн.";
        }

        if (diffVals_pursuiteMaxHp.Count > 1)
        {
            //Debug.Log("Entered");
            //NewUnitUIManager.Instance.PursuiteRadiusSlider.value = 0;
            NewUnitUIManager.Instance.PursuiteMaxHpSliderText.text = "Разн.";
        }

        if (diffVals_pursuiteEndRadius.Count > 1)
        {
            //Debug.Log("Entered");
            //NewUnitUIManager.Instance.PursuiteRadiusSlider.value = 0;
            NewUnitUIManager.Instance.PursuiteEndRadiusSliderText.text = "Разн.";
        }

        if (diffVals_pursuiteMaxAttackDist.Count > 1)
        {
            //Debug.Log("Entered");
            //NewUnitUIManager.Instance.PursuiteRadiusSlider.value = 0;
            NewUnitUIManager.Instance.PursuiteMaxAttackDistSliderText.text = "Разн.";
        }

        if (diffVals_pursuiteTimeForEnd.Count > 1)
        {
            //Debug.Log("Entered");
            //NewUnitUIManager.Instance.PursuiteRadiusSlider.value = 0;
            NewUnitUIManager.Instance.PursuiteTimeForEndField.text = "Разн.";
        }

        #endregion
    }
}


/// <summary>
/// Disable all SelectTag-s (as they are IEnableable-s)
/// </summary>
//[BurstCompile]
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
//[BurstCompile]
public partial struct SingleSelectJob : IJob
{

    [ReadOnly] public CollisionWorld collisionWorld;
    public RaycastInput raycastInput;
    public ComponentLookup<SelectTag> selectTagLookup;
    //public ComponentLookup<BattleModeComponent> attackSetsLookup;

    public EntityCommandBuffer ecb;
    public ComponentLookup<TeamComponent> teamLookup;
    public int curTeam;

    public void Execute()
    {
        if (collisionWorld.CastRay(raycastInput, out RaycastHit raycastHit))
        {
            if (selectTagLookup.HasComponent(raycastHit.Entity) && (teamLookup[raycastHit.Entity].teamInd & curTeam) != 0)
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
                //Entity colorRqstEntity = ecb.CreateEntity();
                //ecb.AddComponent(colorRqstEntity, new ShootModeButChangeColorRqst { color = Color.white });
            }
        }
    }

}


/// <summary>
/// Selects all units, which are inside of the rect (which is in the screen coordinates)
/// All other units are deselected
/// </summary>
//[BurstCompile]
public partial struct MultipleSelectJob : IJobEntity
{
    public WorldToScreen worldToScreen;
    public Rect rect;

    public ComponentLookup<SelectTag> selectTagLookup;
    public EntityCommandBuffer.ParallelWriter ecb;
    public int curTeam;

    public void Execute(Entity entity, in LocalTransform transform, [ChunkIndexInQuery] int chunkIndexInQuery, in TeamComponent team)
    {
        if (rect.Contains(transform.Position.WorldToScreenCoordinatesNative(worldToScreen)) && (team.teamInd & curTeam) != 0)
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

