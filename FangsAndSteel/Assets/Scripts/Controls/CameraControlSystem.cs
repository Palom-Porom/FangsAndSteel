using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[UpdateInGroup(typeof(ControlsSystemGroup))]
public partial class CameraControlSystem : SystemBase
{
    private InputData inputData;

    private Transform cameraPivotTransform;
    private Transform cameraTransform;

    private const float MOVE_SPEED = 30f;
    private const float ROTATION_SPEED = 30f;
    private const float MIN_ANGLE_X = 0f;
    private const float MAX_ANGLE_X = 60f;
    private const float ZOOM_STEP = 10f;
    private const float MIN_DISTANCE_TO_PIVOT = 10-1f;
    private const float MIN_HEIGHT= 25f;

    protected override void OnCreate()
    {
        RequireForUpdate<GameTag>();
        RequireForUpdate<InputData>();
    }

    protected override void OnStartRunning()
    {
        cameraTransform = Camera.main.transform;
        cameraPivotTransform = Camera.main.transform.parent;
    }

    protected override void OnUpdate()
    {
        inputData = SystemAPI.GetSingleton<InputData>();

        cameraPivotTransform.position += cameraPivotTransform.forward * inputData.cameraMoveInputs.y * MOVE_SPEED * SystemAPI.Time.DeltaTime;
        cameraPivotTransform.position += cameraPivotTransform.right * inputData.cameraMoveInputs.x * MOVE_SPEED * SystemAPI.Time.DeltaTime;
        if (cameraPivotTransform.position.y < MIN_HEIGHT)
            cameraPivotTransform.position = new float3(cameraPivotTransform.position.x, MIN_HEIGHT, cameraPivotTransform.position.z);

        float2 rotateInputs = inputData.cameraRotateInputs;
        if (rotateInputs.x != 0.0 || rotateInputs.y != 0.0)
            cameraPivotTransform.rotation = Quaternion.Euler(
            math.clamp(cameraPivotTransform.rotation.eulerAngles.x + rotateInputs.y * ROTATION_SPEED * SystemAPI.Time.DeltaTime, MIN_ANGLE_X, MAX_ANGLE_X),
            cameraPivotTransform.rotation.eulerAngles.y + rotateInputs.x * ROTATION_SPEED * SystemAPI.Time.DeltaTime,
            0f);

        if (inputData.cameraZoomInputs != 0)
            SetCameraOffset(cameraTransform.position + cameraTransform.forward * ZOOM_STEP * inputData.cameraZoomInputs);
    }

    void SetCameraOffset(Vector3 newOffset)
    {
        if (!(math.distancesq(cameraPivotTransform.position, newOffset) < MIN_DISTANCE_TO_PIVOT * MIN_DISTANCE_TO_PIVOT))
            cameraTransform.position = newOffset;
    }
}
