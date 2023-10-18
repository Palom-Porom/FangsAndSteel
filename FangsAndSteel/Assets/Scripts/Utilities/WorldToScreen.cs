using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public struct WorldToScreen
{
    /// <summary>
    /// Camera projection matrix
    /// </summary>
    public float4x4 projectionMatrix;

    /// <summary>
    /// Camera world space position
    /// </summary>
    public float3 position;

    /// <summary>
    /// Camera up vector
    /// </summary>
    public float3 up;

    /// <summary>
    /// Camera right vector
    /// </summary>
    public float3 right;

    /// <summary>
    /// Camera forward vector
    /// </summary>
    public float3 forward;

    /// <summary>
    /// Camera view pixel width
    /// </summary>
    public float pixelWidth;

    /// <summary>
    /// Camera view pixel height
    /// </summary>
    public float pixelHeight;

    /// <summary>
    /// View scale factor
    /// </summary>
    public float scaleFactor;



    public static WorldToScreen Create(Camera camera, float scaleFactor = 1)
    {
        Transform camTransform = camera.transform;
        WorldToScreen result = new WorldToScreen
        {
            projectionMatrix = camera.projectionMatrix,
            position = camTransform.position,
            up = camTransform.up,
            right = camTransform.right,
            forward = camTransform.forward,
            pixelWidth = camera.pixelWidth,
            pixelHeight = camera.pixelHeight,
            scaleFactor = scaleFactor
        };
        return result;
    }
}


public static class WorldToScreenExtention
{
    /// <summary>
    /// Converts world space point to screen space
    /// </summary>
    /// <param name="point"></param>
    /// <param name="worldToScreen"></param>
    /// <returns></returns>
    public static float2 WorldToScreenCoordinatesNative(this float3 point, WorldToScreen worldToScreen)
    {
        return point.WorldToScreenCoordinatesNative(worldToScreen.projectionMatrix, worldToScreen.position,
                                                    worldToScreen.up, worldToScreen.right, worldToScreen.forward,
                                                    worldToScreen.pixelWidth, worldToScreen.pixelHeight, worldToScreen.scaleFactor);
    }

    /// <summary>
    /// Converts world space point to screen space
    /// </summary>
    /// <param name="point"></param>
    /// <param name="camProjectionMatrix"></param>
    /// <param name="camPosition"></param>
    /// <param name="camUp"></param>
    /// <param name="camRight"></param>
    /// <param name="camForward"></param>
    /// <param name="pixelWidth"></param>
    /// <param name="pixelHeight"></param>
    /// <param name="scaleFactor"></param>
    /// <returns></returns>
    public static float2 WorldToScreenCoordinatesNative(this float3 point, float4x4 camProjectionMatrix, float3 camPosition,
                                                        float3 camUp, float3 camRight, float3 camForward,
                                                        float pixelWidth, float pixelHeight, float scaleFactor)
    {
        //1. Translate coordinates to CameraBasis
        float4 pointInCameraBasis = point.ConvertWorldToCameraCoords(camPosition, camUp, camRight, camForward);

        //2. Apply ProjectionMatrix
        float4 pointInClipCoordinates = math.mul(camProjectionMatrix, pointInCameraBasis);

        //3. Convert from homogeneous coordinates (однородных координат) to cartesian coordinate system (декартова система координат)
        float4 pointInCartesianCoords = pointInClipCoordinates / pointInClipCoordinates.w;

        //4. Convert to screen coordinates
        float2 pointInScreenCoordinates;
        pointInScreenCoordinates.x = pixelWidth / 2.0f * (pointInCartesianCoords.x + 1);
        pointInScreenCoordinates.y = pixelHeight / 2.0f * (pointInCartesianCoords.y + 1);

        //5. Apply scale factor
        return pointInScreenCoordinates / scaleFactor;
    }

    private static float4 ConvertWorldToCameraCoords(this float3 point, float3 camPosition,
                                                      float3 camUp, float3 camRight, float3 camForward)
    {
        //Relative to camera coords, but not in camera basis
        float3 camRelativeCoords = point - camPosition;

        float3x3 transformationMatrix = new float3x3();
        transformationMatrix.c0 = new float3(camRight.x, camUp.x, -camForward.x);
        transformationMatrix.c1 = new float3(camRight.y, camUp.y, -camForward.y);
        transformationMatrix.c2 = new float3(camRight.z, camUp.z, -camForward.z);

        float3 transformedPoint = math.mul(transformationMatrix, camRelativeCoords);

        return new float4(transformedPoint, 1f);
    }
}

