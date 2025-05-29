using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FlowFieldVolume))]
public class FlowFieldVolumeEditor : Editor
{
    // Vector field
    SerializedProperty Intensity;
    SerializedProperty Bounds;
    SerializedProperty PerAxisBounds;
    SerializedProperty BoundsX;
    SerializedProperty BoundsY;
    SerializedProperty BoundsZ;
    SerializedProperty ClosedBoundRamp;
    
    // Animation
    SerializedProperty Animate;
    SerializedProperty AnimationType;
    SerializedProperty Curve;
    SerializedProperty Duration;
    
    // Size
    SerializedProperty SizeFromFile;
    SerializedProperty SizeX;
    SerializedProperty SizeY;
    SerializedProperty SizeZ;
    
    // Gizmo
    SerializedProperty ShowGizmos;
    SerializedProperty GizmoScale;
    SerializedProperty VectorFieldColor;
    
    private string[] leafnames = null;
    private int selectedSource = 0;

    private int currentTab = 0;
    
    void OnEnable()
    {
        Intensity = serializedObject.FindProperty("Intensity");
        Bounds = serializedObject.FindProperty("Bounds");
        PerAxisBounds = serializedObject.FindProperty("PerAxisBounds");
        BoundsX = serializedObject.FindProperty("BoundsX");
        BoundsY = serializedObject.FindProperty("BoundsY");
        BoundsZ = serializedObject.FindProperty("BoundsZ");
        ClosedBoundRamp = serializedObject.FindProperty("ClosedBoundRamp");
        
        // Animation
        Animate = serializedObject.FindProperty("Animate");
        AnimationType = serializedObject.FindProperty("AnimationType");
        Curve = serializedObject.FindProperty("Curve");
        Duration = serializedObject.FindProperty("Duration");
        
        // Size
        SizeFromFile = serializedObject.FindProperty("SizeFromFile");
        SizeX = serializedObject.FindProperty("SizeX");
        SizeY = serializedObject.FindProperty("SizeY");
        SizeZ = serializedObject.FindProperty("SizeZ");
        
        // Gizmo
        ShowGizmos = serializedObject.FindProperty("ShowGizmos");
        GizmoScale = serializedObject.FindProperty("GizmoScale");
        VectorFieldColor = serializedObject.FindProperty("VectorFieldColor");

        RefreshFileList();
        
        FlowFieldVolume editedObj = target as FlowFieldVolume;

        for (int i = 0; i<leafnames.Length; i++)
        {
            if (leafnames[i] == editedObj.VFFilename)
                selectedSource = i;
        }
    }
    
    private void RefreshFileList()
    {
        string basicPath = Application.dataPath;
        string[] files = Directory.GetFiles(basicPath, "*.fga", SearchOption.AllDirectories);
        leafnames = new string[files.Length+1];
        leafnames[0] = "None";
        for (int i = 0; i<files.Length; i++) { leafnames[i+1] = Path.GetFileNameWithoutExtension(files[i]); }
    }
    
    private string GetFullName(string leafname)
    {
        string basicPath = Application.dataPath;
        string[] files = Directory.GetFiles(basicPath, "*.fga", SearchOption.AllDirectories);

        for (int i = 0; i<files.Length; i++)
        {
            if (leafname == Path.GetFileNameWithoutExtension(files[i]))
                return files[i];
        }

        return "";
    }
    
    private void ReadVF(int index)
    {
        FlowFieldVolume editedObj = target as FlowFieldVolume;

        editedObj.VFFilename = leafnames[index];
		
        string fullname = GetFullName(editedObj.VFFilename);
        if (string.IsNullOrEmpty(fullname))
            editedObj.ClearFF();
        else
            editedObj.ReadFGA(fullname);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        FlowFieldVolume editedObj = target as FlowFieldVolume;
        
        int previousSelection = selectedSource;
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("FGA File");
        selectedSource = EditorGUILayout.Popup(selectedSource, leafnames);
        if (GUILayout.Button("Refresh"))
            RefreshFileList();
        GUILayout.EndHorizontal();
        GUILayout.Label("Flow field size : "+ editedObj.BoxSize.x.ToString("F2")+", "+editedObj.BoxSize.y.ToString("F2") + ", "+editedObj.BoxSize.z.ToString("F2"));

        if (selectedSource != previousSelection)
            ReadVF(selectedSource);
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Reload FF"))
            ReadVF(selectedSource);
        if (GUILayout.Button("Rename from file"))
        {
            editedObj.name = leafnames[selectedSource];
        }
        GUILayout.EndHorizontal();
        
        GUILayout.BeginVertical("Box");
        currentTab = GUILayout.Toolbar(currentTab, new string[] {"Bounds", "Intensity", "Size", "Gizmo"});
        GUILayout.EndVertical();
        
        GUILayout.BeginVertical(GUILayout.Height(100));
        GUILayout.Space(5);
        switch (currentTab)
        {
            case 0:
                EditorGUILayout.PropertyField(PerAxisBounds);
                if (PerAxisBounds.boolValue)
                {
                    EditorGUILayout.PropertyField(BoundsX);
                    EditorGUILayout.PropertyField(BoundsY);
                    EditorGUILayout.PropertyField(BoundsZ);
                }
                else
                    EditorGUILayout.PropertyField(Bounds);
                EditorGUILayout.PropertyField(ClosedBoundRamp);
                break;
            case 1:
                EditorGUILayout.PropertyField(Intensity);
                EditorGUILayout.PropertyField(Animate);
                if (Animate.boolValue)
                {
                    EditorGUILayout.PropertyField(AnimationType);
                    EditorGUILayout.PropertyField(Curve);
                    EditorGUILayout.PropertyField(Duration);
                }
                break;
            case 2:
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(SizeFromFile);
                if (!SizeFromFile.boolValue)
                {
                    EditorGUILayout.PropertyField(SizeX);
                    EditorGUILayout.PropertyField(SizeY);
                    EditorGUILayout.PropertyField(SizeZ);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    if (SizeFromFile.boolValue)
                        ReadVF(selectedSource);
                    else
                        editedObj.ResizeBox();
                }
                break;
            case 3:
                EditorGUILayout.PropertyField(ShowGizmos);
                EditorGUILayout.PropertyField(GizmoScale);
                EditorGUILayout.PropertyField(VectorFieldColor);
                break;
        }
        GUILayout.EndVertical();
        serializedObject.ApplyModifiedProperties();
    }
}