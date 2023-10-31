using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;


[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
public partial class ControlSystem : SystemBase
{
    private ControlsAsset controlsAssetClass;
    private RefRW<InputData> inputDataSingleton;

    //Targeting
    private float3 cameraPosition;
    private float3 mouseTargetingPoint;
    private bool neededTargeting = false;


    protected override void OnCreate()
    {
        controlsAssetClass = new ControlsAsset();
        EntityManager.AddComponent<InputData>(SystemHandle);

        controlsAssetClass.Game.TargetSelectedUnits.performed += CollectTargetingInfo;
    }


    protected override void OnStartRunning() => controlsAssetClass.Enable();
    protected override void OnStopRunning() => controlsAssetClass.Disable();


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
            inputDataSingleton.ValueRW.neededTargeting = true;
            neededTargeting = false;
        }
    }

    private void CollectTargetingInfo(InputAction.CallbackContext context)
    {
        var ray = Camera.main.ScreenPointToRay(Mouse.current.position.value);
        mouseTargetingPoint = ray.GetPoint(1000f);
        cameraPosition = Camera.main.transform.position;
        neededTargeting = true;
    }
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
    

}
