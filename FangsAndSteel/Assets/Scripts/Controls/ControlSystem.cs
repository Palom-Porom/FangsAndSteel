using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public partial class ControlSystem : SystemBase
{
    private ControlsAsset controlsAssetClass;
    private RefRW<InputData> inputDataSingleton;



    protected override void OnCreate()
    {
        controlsAssetClass = new ControlsAsset();
        EntityManager.AddComponent<InputData>(SystemHandle);


    }


    protected override void OnStartRunning() => controlsAssetClass.Enable();
    protected override void OnStopRunning() => controlsAssetClass.Disable();


    protected override void OnUpdate()
    {
        inputDataSingleton = SystemAPI.GetComponentRW<InputData>(SystemHandle);

        inputDataSingleton.ValueRW.cameraMoveInputs = controlsAssetClass.Game.MoveCamera.ReadValue<Vector2>();
        inputDataSingleton.ValueRW.cameraRotateInputs = controlsAssetClass.Game.RotateCamera.ReadValue<Vector2>();
        inputDataSingleton.ValueRW.cameraZoomInputs = math.sign(controlsAssetClass.Game.ZoomCamera.ReadValue<float>());
    }
}

public struct InputData : IComponentData
{
    //Camera Controls
    public float2 cameraMoveInputs;
    public float2 cameraRotateInputs;
    public float cameraZoomInputs;
}
