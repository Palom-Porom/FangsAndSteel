using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
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
    private const float MIN_DISTANCE_TO_PIVOT = 10 - 1f;
    private const float MIN_HEIGHT = 45f;

    private const float X_RIGHT_BORDER = 500f;
    private const float X_LEFT_BORDER = -500f;
    private const float Z_FWD_BORDER = 500f;
    private const float Z_BCK_BORDER = -500f;

    public static float3 lastCameraPos;
    public static Quaternion lastPivotRotation;
    public static float3 lastPivotPos;

    protected override void OnCreate()
    {
        RequireForUpdate(new EntityQueryBuilder(Allocator.Temp).WithAny<GameTag, TutorialTag>().Build(this));
        //RequireForUpdate<GameTag>();
        RequireForUpdate<InputData>();
    }

    protected override void OnStartRunning()
    {
        cameraTransform = Camera.main.transform;
        cameraPivotTransform = Camera.main.transform.parent;

        //lastCameraPos = cameraTransform.position;
        //lastPivotPos = cameraPivotTransform.position;
        //lastPivotRotation = cameraPivotTransform.rotation;
    }

    protected override void OnUpdate()
    {
        inputData = SystemAPI.GetSingleton<InputData>();

        var fwd = cameraPivotTransform.forward;
        fwd.y = 0;
        fwd.Normalize();

        var rgt = cameraPivotTransform.right;
        rgt.y = 0;
        rgt.Normalize();

        if (!inputData.cameraBordersDisabled)
        {
            //cameraPivotTransform.position += cameraPivotTransform.forward * inputData.cameraMoveInputs.y * MOVE_SPEED * SystemAPI.Time.DeltaTime;
            cameraPivotTransform.position += fwd * inputData.cameraMoveInputs.y * MOVE_SPEED * SystemAPI.Time.DeltaTime;
            //cameraPivotTransform.position += cameraPivotTransform.right * inputData.cameraMoveInputs.x * MOVE_SPEED * SystemAPI.Time.DeltaTime;
            cameraPivotTransform.position += rgt * inputData.cameraMoveInputs.x * MOVE_SPEED * SystemAPI.Time.DeltaTime;
        }
        else
        {
            cameraPivotTransform.position += cameraPivotTransform.forward * inputData.cameraMoveInputs.y * MOVE_SPEED * SystemAPI.Time.DeltaTime;
            cameraPivotTransform.position += cameraPivotTransform.right * inputData.cameraMoveInputs.x * MOVE_SPEED * SystemAPI.Time.DeltaTime;
        }
        if (!inputData.cameraBordersDisabled && cameraPivotTransform.position.y < MIN_HEIGHT)
            cameraPivotTransform.position = new float3(cameraPivotTransform.position.x, MIN_HEIGHT, cameraPivotTransform.position.z);

        cameraPivotTransform.position = new Vector3(
            math.clamp(cameraPivotTransform.position.x, X_LEFT_BORDER, X_RIGHT_BORDER),
            cameraPivotTransform.position.y,
            math.clamp(cameraPivotTransform.position.z, Z_BCK_BORDER, Z_FWD_BORDER)
            );

        float2 rotateInputs = inputData.cameraRotateInputs;
        if (rotateInputs.x != 0.0 || rotateInputs.y != 0.0)
            cameraPivotTransform.rotation = Quaternion.Euler(
            math.clamp(cameraPivotTransform.rotation.eulerAngles.x + rotateInputs.y * ROTATION_SPEED * SystemAPI.Time.DeltaTime, MIN_ANGLE_X, MAX_ANGLE_X),
            cameraPivotTransform.rotation.eulerAngles.y + rotateInputs.x * ROTATION_SPEED * SystemAPI.Time.DeltaTime,
            0f);

        if (inputData.cameraZoomInputs != 0 && !EventSystem.current.IsPointerOverGameObject())
            SetCameraOffset(cameraTransform.position + cameraTransform.forward * ZOOM_STEP * inputData.cameraZoomInputs);
        //else if (EventSystem.current.IsPointerOverGameObject()) Debug.Log("IsOverGO");
    }

    void SetCameraOffset(Vector3 newOffset)
    {
        //Debug.Log(newOffset);
        if (!(math.distancesq(cameraPivotTransform.position, newOffset) < MIN_DISTANCE_TO_PIVOT * MIN_DISTANCE_TO_PIVOT) /*&& newOffset.z < 0*/)
            cameraTransform.position = newOffset;
    }

    public static void StepUpdateCameraPos()
    {
        var temp1 = lastCameraPos;
        var temp2 = lastPivotPos;
        var temp3 = lastPivotRotation;
        lastCameraPos = Camera.main.transform.localPosition;
        lastPivotPos = Camera.main.transform.parent.position;
        lastPivotRotation = Camera.main.transform.parent.rotation;
        Camera.main.transform.parent.position = temp2;
        Camera.main.transform.localPosition = temp1;
        Camera.main.transform.parent.rotation = temp3;
    }
}
