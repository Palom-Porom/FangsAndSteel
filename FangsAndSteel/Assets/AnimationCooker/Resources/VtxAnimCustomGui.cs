//// This custom GUI is nearly an exact copy of the custom GUI made for the SimpleLit shader.
//// The only difference is that it has an extra field to select the position texture used in vertex animations.
//// It derives from BaseShaderGUI

//#if UNITY_EDITOR

//using UnityEditor;
//using System;
//using UnityEngine;
//using UnityEditor.Rendering.Universal.ShaderGUI;

//public struct VertexAnimationProperties
//{
//    public MaterialProperty posMap;
//    public MaterialProperty nmlMap;
//    public MaterialProperty tanMap;
//    //public MaterialProperty useNml;
//    //public MaterialProperty useTan;
//    //public MaterialProperty onlyUsePosMap;
//    //public MaterialProperty curTime;
//    //public MaterialProperty clipIdx;
//    //public MaterialProperty speedInst;
//    //public MaterialProperty speedMat;

//    public VertexAnimationProperties(MaterialProperty[] properties)
//    {
//        posMap = BaseShaderGUI.FindProperty("_PosMap", properties);
//        nmlMap = BaseShaderGUI.FindProperty("_NmlMap", properties);
//        tanMap = BaseShaderGUI.FindProperty("_TanMap", properties);
//        //onlyUsePosMap = BaseShaderGUI.FindProperty("_OnlyUsePosMap", properties);
//        //curTime = BaseShaderGUI.FindProperty("_CurTime", properties, false);
//        //clipIdx = BaseShaderGUI.FindProperty("_ClipIdx", properties, false);
//        //speedInst = BaseShaderGUI.FindProperty("_SpeedInst", properties, false);
//        //speedMat = BaseShaderGUI.FindProperty("_SpeedMat", properties, false);
//    }
//}

//class VertexAnimationCustomGui : BaseShaderGUI
//{
//    // Properties
//    private SimpleLitGUI.SimpleLitProperties shadingModelProperties;
//    private VertexAnimationProperties vtxAnimProps;

//    // collect properties from the material properties
//    public override void FindProperties(MaterialProperty[] properties)
//    {
//        base.FindProperties(properties);
//        shadingModelProperties = new SimpleLitGUI.SimpleLitProperties(properties);
//        vtxAnimProps = new VertexAnimationProperties(properties);
//    }

//    // material changed check
//    public override void ValidateMaterial(Material material)
//    {
//        SetMaterialKeywords(material, SimpleLitGUI.SetMaterialKeywords);
//    }

//    // material main surface options
//    public override void DrawSurfaceOptions(Material material)
//    {
//        if (material == null) { throw new ArgumentNullException("material"); }

//        // Use default labelWidth
//        EditorGUIUtility.labelWidth = 0f;

//        base.DrawSurfaceOptions(material);
//    }

//    // material main surface inputs
//    public override void DrawSurfaceInputs(Material material)
//    {
//        materialEditor.TexturePropertySingleLine(new GUIContent("Position Map"), vtxAnimProps.posMap);
//        materialEditor.TexturePropertySingleLine(new GUIContent("Normal Map"), vtxAnimProps.nmlMap);
//        materialEditor.TexturePropertySingleLine(new GUIContent("Tangent Map"), vtxAnimProps.tanMap);
//        base.DrawSurfaceInputs(material);
//        SimpleLitGUI.Inputs(shadingModelProperties, materialEditor, material);
//        DrawEmissionProperties(material, true);
//        DrawTileOffset(materialEditor, baseMapProp);
//    }

//    public override void DrawAdvancedOptions(Material material)
//    {
//        SimpleLitGUI.Advanced(shadingModelProperties);
//        base.DrawAdvancedOptions(material);
//    }

//    public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
//    {
//        if (material == null)
//            throw new ArgumentNullException("material");

//        // _Emission property is lost after assigning Standard shader to the material
//        // thus transfer it before assigning the new shader
//        if (material.HasProperty("_Emission"))
//        {
//            material.SetColor("_EmissionColor", material.GetColor("_Emission"));
//        }

//        base.AssignNewShaderToMaterial(material, oldShader, newShader);

//        if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
//        {
//            SetupMaterialBlendMode(material);
//            return;
//        }

//        SurfaceType surfaceType = SurfaceType.Opaque;
//        BlendMode blendMode = BlendMode.Alpha;
//        if (oldShader.name.Contains("/Transparent/Cutout/"))
//        {
//            surfaceType = SurfaceType.Opaque;
//            material.SetFloat("_AlphaClip", 1);
//        }
//        else if (oldShader.name.Contains("/Transparent/"))
//        {
//            // NOTE: legacy shaders did not provide physically based transparency
//            // therefore Fade mode
//            surfaceType = SurfaceType.Transparent;
//            blendMode = BlendMode.Alpha;
//        }
//        material.SetFloat("_Surface", (float)surfaceType);
//        material.SetFloat("_Blend", (float)blendMode);
//    }
//}

//#endif