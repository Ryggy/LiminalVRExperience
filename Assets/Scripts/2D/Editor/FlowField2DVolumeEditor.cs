using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FlowField2DVolume))]
public class FlowField2DVolumeEditor : Editor
{
    string[] fileOptions;
    int selectedIndex = 0;
    int previousIndex = -1;

    void OnEnable()
    {
        RefreshFGAList();

        var flow = (FlowField2DVolume)target;
        for (int i = 0; i < fileOptions.Length; i++)
        {
            if (fileOptions[i] == Path.GetFileNameWithoutExtension(flow.fgaFileName))
                selectedIndex = i;
        }
        
        previousIndex = selectedIndex;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        FlowField2DVolume flow = (FlowField2DVolume)target;

        GUILayout.Label("FGA File Loader", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        selectedIndex = EditorGUILayout.Popup("FGA File", selectedIndex, fileOptions);
        
        if (selectedIndex != previousIndex)
        {
            previousIndex = selectedIndex;

            if (fileOptions != null && selectedIndex >= 0 && selectedIndex < fileOptions.Length)
            {
                string filename = fileOptions[selectedIndex];
                flow.fgaFileName = filename + ".fga";
                flow.LoadFromFile(flow.fgaFileName);
                Debug.Log($"Auto-loaded FGA: {filename}");
            }
        }
        
        if (GUILayout.Button("Refresh List"))
        {
            RefreshFGAList();
        }
        
        // if (GUILayout.Button("Load FGA"))
        // {
        //     if (fileOptions == null || fileOptions.Length == 0 || selectedIndex >= fileOptions.Length)
        //     {
        //         Debug.LogWarning("No FGA files available or invalid index");
        //         return;
        //     }
        //     string filename = fileOptions[selectedIndex];
        //
        //     if (filename != null)
        //     {
        //         flow.fgaFileName = filename + ".fga";
        //         flow.LoadFromFile(flow.fgaFileName);
        //     }
        // }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);
        DrawDefaultInspector();

        serializedObject.ApplyModifiedProperties();
    }

    void RefreshFGAList()
    {
        string[] paths = Directory.GetFiles(Application.dataPath, "*.fga", SearchOption.AllDirectories);
        fileOptions = new string[paths.Length];

        for (int i = 0; i < paths.Length; i++)
        {
            fileOptions[i] = Path.GetFileNameWithoutExtension(paths[i]);
        }
    }
}
