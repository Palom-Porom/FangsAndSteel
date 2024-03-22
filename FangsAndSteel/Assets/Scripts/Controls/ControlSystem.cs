using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;

//[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
//[UpdateInGroup(typeof(ControlsSystemGroup), OrderFirst = true)]
[UpdateInGroup(typeof(GhostInputSystemGroup))] //Exists only in Client world
public partial class ControlSystem : SystemBase
{
    static public ControlsAsset controlsAssetClass;
    //private RefRW<InputData> inputDataSingleton;

    //Targeting
    private float3 cameraPosition;
    private float3 mouseTargetingPoint;
    private bool neededTargeting = false;
    private bool shiftTargeting = false;

    private bool cameraBordersDisabled = false;


    protected override void OnCreate()
    {
        RequireForUpdate<GameTag>();

        controlsAssetClass = new ControlsAsset();
        //EntityManager.AddComponent<InputData>(SystemHandle); - authored to the player entity
    }

    protected override void OnDestroy()
    {
        controlsAssetClass.Dispose();
    }


    protected override void OnStartRunning()
    {
        controlsAssetClass.Enable();

        controlsAssetClass.Game.TargetSelectedUnits.performed += CollectTargetingInfo;
        controlsAssetClass.Game.Shift_TargetSelectedUnits.performed += CollectTargetingInfo;
        controlsAssetClass.Game.Shift_TargetSelectedUnits.performed += SetFlagForShiftTargeting;
        controlsAssetClass.Game.DisableCameraBorders.performed += DisableCameraBorders;
    }
    protected override void OnStopRunning()
    {
        controlsAssetClass.Game.TargetSelectedUnits.performed -= CollectTargetingInfo;
        controlsAssetClass.Game.Shift_TargetSelectedUnits.performed -= CollectTargetingInfo;
        controlsAssetClass.Game.Shift_TargetSelectedUnits.performed -= SetFlagForShiftTargeting;
        controlsAssetClass.Game.DisableCameraBorders.performed -= DisableCameraBorders;

        controlsAssetClass.Disable();
    }


    protected override void OnUpdate()
    {
        //inputDataSingleton = SystemAPI.GetComponentRW<InputData>(SystemHandle);

        //RefRW<LocalInputData> localInputData = SystemAPI.GetSingletonRW<LocalInputData>();
        //RefRW<GlobalInputData> globalInputData = SystemAPI.GetSingletonRW<GlobalInputData>();

        foreach ((RefRW<LocalInputData> localInputData, RefRW<GlobalInputData> globalInputData)
            in SystemAPI.Query<RefRW<LocalInputData>, RefRW<GlobalInputData>>().WithAll<GhostOwnerIsLocal>())
        {

            //Camera Controls
            localInputData.ValueRW.cameraMoveInputs = controlsAssetClass.Game.MoveCamera.ReadValue<Vector2>();
            localInputData.ValueRW.cameraRotateInputs = controlsAssetClass.Game.RotateCamera.ReadValue<Vector2>();
            localInputData.ValueRW.cameraZoomInputs = math.sign(controlsAssetClass.Game.ZoomCamera.ReadValue<float>());
            localInputData.ValueRW.cameraBordersDisabled = cameraBordersDisabled;
            //inputDataSingleton.ValueRW.cameraMoveInputs = controlsAssetClass.Game.MoveCamera.ReadValue<Vector2>();
            //inputDataSingleton.ValueRW.cameraRotateInputs = controlsAssetClass.Game.RotateCamera.ReadValue<Vector2>();
            //inputDataSingleton.ValueRW.cameraZoomInputs = math.sign(controlsAssetClass.Game.ZoomCamera.ReadValue<float>());
            //inputDataSingleton.ValueRW.cameraBordersDisabled = cameraBordersDisabled;


            //Targeting
            if (neededTargeting)
            {
                globalInputData.ValueRW.cameraPosition = cameraPosition;
                globalInputData.ValueRW.mouseTargetingPoint = mouseTargetingPoint;
                //inputDataSingleton.ValueRW.cameraPosition = cameraPosition;
                //inputDataSingleton.ValueRW.mouseTargetingPoint = mouseTargetingPoint;
            }
            if (neededTargeting) globalInputData.ValueRW.neededTargeting.Set();
            if (shiftTargeting) globalInputData.ValueRW.shiftTargeting.Set();
            //inputDataSingleton.ValueRW.neededTargeting = neededTargeting;
            //inputDataSingleton.ValueRW.shiftTargeting = shiftTargeting;
            neededTargeting = false;
            shiftTargeting = false;
        }
    }


    private void CollectTargetingInfo(InputAction.CallbackContext context)
    {
        var ray = Camera.main.ScreenPointToRay(Mouse.current.position.value);
        mouseTargetingPoint = ray.GetPoint(1000f);
        cameraPosition = Camera.main.transform.position;
        neededTargeting = true;
    }
    private void SetFlagForShiftTargeting(InputAction.CallbackContext context) { shiftTargeting = true; }

    private void DisableCameraBorders(InputAction.CallbackContext context) { cameraBordersDisabled = !cameraBordersDisabled; }
}


public struct LocalInputData : IComponentData
{
    //Camera Controls
    public float2 cameraMoveInputs;
    public float2 cameraRotateInputs;
    public float cameraZoomInputs;

    public bool cameraBordersDisabled;
}


[GhostComponent(PrefabType = GhostPrefabType.All)]
public struct GlobalInputData : IInputComponentData
{
    public int teamInd;

    //Targeting
    public float3 cameraPosition;
    public float3 mouseTargetingPoint;
    public InputEvent neededTargeting;
    public InputEvent shiftTargeting;
}

/// <summary>
/// Contains all input data for access from any place of code (even in the ISystem)
/// </summary>
public struct InputData : IComponentData
{
    //Camera Controls
    public float2 cameraMoveInputs;
    public float2 cameraRotateInputs;
    public float cameraZoomInputs;

    //Targeting
    public float3 cameraPosition;
    public float3 mouseTargetingPoint;
    public bool neededTargeting;
    public bool shiftTargeting;

    public bool cameraBordersDisabled;
}
