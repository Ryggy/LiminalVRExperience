using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(FlowFieldTest))]

public class FlowFieldEditor : Editor
{
    // Cache the target FlowFieldTest object for easier access
    private FlowFieldTest flowFieldTest;

    // Called when the editor window is created
    private void OnEnable()
    {
        flowFieldTest = (FlowFieldTest)target;
    }

    // Draw the custom inspector in the Unity editor
    public override void OnInspectorGUI()
    {
        // Add a title for the FlowFieldTest section
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Flow Field Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Display the basic grid settings
        flowFieldTest.cols = EditorGUILayout.IntField("Columns", flowFieldTest.cols);
        flowFieldTest.rows = EditorGUILayout.IntField("Rows", flowFieldTest.rows);
        flowFieldTest.scale = EditorGUILayout.FloatField("Scale", flowFieldTest.scale);
        flowFieldTest.increment = EditorGUILayout.FloatField("Increment", flowFieldTest.increment);
        
        
        // Add a black line to separate sections
        DrawSeparator();

        // Outline Type Section
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Outline Type", EditorStyles.boldLabel);
        flowFieldTest.outlineType = (FlowFieldTest.OutlineType)EditorGUILayout.EnumPopup("Outline Type", flowFieldTest.outlineType);
        
        
        // Add a black line to separate sections
        DrawSeparator();

        // Fire Settings Section
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Fire Field Settings", EditorStyles.boldLabel);
        flowFieldTest.fireScale = EditorGUILayout.FloatField("Fire Scale", flowFieldTest.fireScale);
        flowFieldTest.flickerSpeed = EditorGUILayout.FloatField("Flicker Speed", flowFieldTest.flickerSpeed);
        flowFieldTest.flickerAmount = EditorGUILayout.FloatField("Flicker Amount", flowFieldTest.flickerAmount);
        flowFieldTest.fireBaseWidth = EditorGUILayout.FloatField("Fire Base Width", flowFieldTest.fireBaseWidth);
        flowFieldTest.fireTipWidth = EditorGUILayout.FloatField("Fire Tip Width", flowFieldTest.fireTipWidth);
        flowFieldTest.startColor = EditorGUILayout.ColorField("Fire Start Color", flowFieldTest.startColor);
        flowFieldTest.middleColor = EditorGUILayout.ColorField("Fire Middle Color", flowFieldTest.middleColor);
        flowFieldTest.endColor = EditorGUILayout.ColorField("Fire End Color", flowFieldTest.endColor);
        

        // Add a black line to separate sections
        DrawSeparator();

        // Earth and Water settings (for future expansion)
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Earth Field Settings", EditorStyles.boldLabel);
        // Earth-specific settings can be added here in the future
        
        // Add a black line to separate sections
        DrawSeparator();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Water Field Settings", EditorStyles.boldLabel);
        // Water-specific settings can be added here in the future
        flowFieldTest.waveAmplitude = EditorGUILayout.Slider("Wave Amplitude", flowFieldTest.waveAmplitude, 0f, 1000f);
        flowFieldTest.waveFrequency = EditorGUILayout.Slider("Wave Frequency", flowFieldTest.waveFrequency, 0f, 2000f);
        flowFieldTest.waveSpeed = EditorGUILayout.Slider("Wave Speed", flowFieldTest.waveSpeed, 0f, 1000f);
        flowFieldTest.waterLevel = EditorGUILayout.Slider("Water Level", flowFieldTest.waterLevel, 0f, 4000f);
        flowFieldTest.waterStartColor = EditorGUILayout.ColorField("Water Start Color", flowFieldTest.waterStartColor);
        flowFieldTest.waterEndColor = EditorGUILayout.ColorField("Water End Color", flowFieldTest.waterEndColor);
        
        
        

        // Add a black line to separate sections
        DrawSeparator();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Air Field Settings", EditorStyles.boldLabel);
        // Air Field Settings (New section for air)
        flowFieldTest.windStrength = EditorGUILayout.Slider("Wind Strength", flowFieldTest.windStrength, 0f, 10f);
        flowFieldTest.windFrequency = EditorGUILayout.Slider("Wind Frequency", flowFieldTest.windFrequency, 0f, 5f);
        flowFieldTest.windSpeed = EditorGUILayout.Slider("Wind Speed", flowFieldTest.windSpeed, 0f, 10f);
        flowFieldTest.airTurbulence = EditorGUILayout.Slider("Air Turbulence", flowFieldTest.airTurbulence, 0f, 1f);
        flowFieldTest.airStartColor = EditorGUILayout.ColorField("Air Start Color", flowFieldTest.airStartColor);
        flowFieldTest.airEndColor = EditorGUILayout.ColorField("Air End Color", flowFieldTest.airEndColor);

        
        // Add a black line to separate sections
        DrawSeparator();

        // Show the "Show Flow Field" toggle for visualization
        flowFieldTest.showFlowField = EditorGUILayout.Toggle("Show Flow Field", flowFieldTest.showFlowField);

        // Apply changes to the serialized object
        if (GUI.changed)
        {
            EditorUtility.SetDirty(flowFieldTest);
        }

        // Always call base method to ensure proper inspector functionality
        base.OnInspectorGUI();
    }

    // Helper method to draw a black line
    private void DrawSeparator()
    {
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
    }
}