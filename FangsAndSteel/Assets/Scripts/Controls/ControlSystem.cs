using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[UpdateInGroup(typeof(ControlsSystemGroup), OrderFirst = true)]
public partial class ControlSystem : SystemBase
{
    static public ControlsAsset controlsAssetClass;
    private RefRW<InputData> inputDataSingleton;

    //Targeting
    private float3 cameraPosition;
    private float3 mouseTargetingPoint;
    private bool neededTargeting = false;
    private bool shiftTargeting = false;


    protected override void OnCreate()
    {
        RequireForUpdate<GameTag>();

        controlsAssetClass = new ControlsAsset();
        EntityManager.AddComponent<InputData>(SystemHandle);
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
    }
    protected override void OnStopRunning()
    {
        controlsAssetClass.Game.TargetSelectedUnits.performed -= CollectTargetingInfo;
        controlsAssetClass.Game.Shift_TargetSelectedUnits.performed -= CollectTargetingInfo;
        controlsAssetClass.Game.Shift_TargetSelectedUnits.performed -= SetFlagForShiftTargeting;

        controlsAssetClass.Disable();
    }


    protected override void OnUpdate()
    {
        inputDataSingleton = SystemAPI.GetComponentRW<InputData>(SystemHandle);

        //Camera Controls
        inputDataSingleton.ValueRW.cameraMoveInputs = controlsAssetClass.Game.MoveCamera.ReadValue<Vector2>();
        inputDataSingleton.ValueRW.cameraRotateInputs = controlsAssetClass.Game.RotateCamera.ReadValue<Vector2>();
        inputDataSingleton.ValueRW.cameraZoomInputs = math.sign(controlsAssetClass.Game.ZoomCamera.ReadValue<float>());

        //Targeting
        if (neededTargeting)
        {
            inputDataSingleton.ValueRW.cameraPosition = cameraPosition;
            inputDataSingleton.ValueRW.mouseTargetingPoint = mouseTargetingPoint;
        }
        inputDataSingleton.ValueRW.neededTargeting = neededTargeting;
        inputDataSingleton.ValueRW.shiftTargeting = shiftTargeting;
        neededTargeting = false;
        shiftTargeting = false;
    }
    private void CollectTargetingInfo(InputAction.CallbackContext context)
    {
        var ray = Camera.main.ScreenPointToRay(Mouse.current.position.value);
        mouseTargetingPoint = ray.GetPoint(1000f);
        cameraPosition = Camera.main.transform.position;
        neededTargeting = true;
    }
    private void SetFlagForShiftTargeting(InputAction.CallbackContext context) { shiftTargeting = true; }
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
    

}
