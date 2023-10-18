using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class GUI_Utilities
{
    private static Texture2D insideBoxTexture;


    /// <summary>
    /// Base texture of inside space in the BoxSelection rectangle
    /// </summary>
    private static Texture2D InsideBoxTexture
    {
        get
        {
            if (insideBoxTexture == null)
            {
                //Color semiTransperentWhite = new Color(1, 1, 1, 0.2f);
                insideBoxTexture = new Texture2D(1, 1);
                insideBoxTexture.SetPixel(0, 0, new Color(1, 1, 1, 0.2f));
                insideBoxTexture.Apply();
            }
            return insideBoxTexture;
        }
    }


    /// <summary>
    /// Get the rectangle based on 2 corners (placed as a diagonal in the rect)
    /// </summary>
    /// <param name="screenPosition1"></param>
    /// <param name="screenPosition2"></param>
    /// <returns></returns>
    public static Rect GetScreenRect(float2 screenPosition1, float2 screenPosition2)
    {
        // Move origin from bottom left to top left, beacuse DrawTexture has origin in the topLeft
        screenPosition1.y = Screen.height - screenPosition1.y;
        screenPosition2.y = Screen.height - screenPosition2.y;

        //get corners of rect
        float2 topLeft = math.min(screenPosition1, screenPosition2);
        float2 bottomRight = math.max(screenPosition1, screenPosition2);

        //Create Rect
        return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
    }


    /// <summary>
    /// Draw a rect on the screen
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="color"></param>
    public static void DrawScreenRect(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, InsideBoxTexture);
    }


    /// <summary>
    /// Draw a borders of given rect on the screen
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="thickness"></param>
    /// <param name="color"></param>
    public static void DrawRectBorders(Rect rect, float thickness, Color color)
    {
        //Top
        DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);

        //Right
        DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);

        //Left
        DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);

        //Bottom
        DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
    }



    /// <summary>
    /// Draw a rect with borders on the screen
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="bordersThickness"></param>
    /// <param name="color"></param>
    public static void DrawRectWithBorders(Rect rect, float bordersThickness, Color insideColor, Color bordersColor)
    {
        DrawScreenRect(rect, insideColor);
        DrawRectBorders(rect, bordersThickness, bordersColor);
    }


}
