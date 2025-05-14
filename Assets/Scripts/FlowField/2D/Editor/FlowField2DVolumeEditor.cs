using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FlowField2DVolume))]
public class FlowField2DVolumeEditor : Editor
{
    string[] fileOptions;
    int selectedIndex = 0;

    void OnEnable()
    {
        RefreshFGAList();

        var flow = (FlowField2DVolume)target;
        for (int i = 0; i < fileOptions.Length; i++)
        {
            if (fileOptions[i] == Path.GetFileNameWithoutExtension(flow.fgaPath))
                selectedIndex = i;
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        FlowField2DVolume flow = (FlowField2DVolume)target;

        GUILayout.Label("FGA File Loader", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        selectedIndex = EditorGUILayout.Popup("FGA File", selectedIndex, fileOptions);
        if (GUILayout.Button("Refresh"))
        {
            RefreshFGAList();
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Reload FGA"))
        {
            string filename = fileOptions[selectedIndex];
            flow.fgaPath = FindFGAPathByName(filename);
            flow.ReadFGA(flow.fgaPath);
        }

        GUILayout.Space(10);
        DrawDefaultInspector();

        serializedObject.ApplyModifiedProperties();
    }

    void RefreshFGAList()
    {
        string[] paths = Directory.GetFiles(Application.dataPath, "*.fga", SearchOption.AllDirectories);
        fileOptions = new string[paths.Length + 1];
        fileOptions[0] = "None";

        for (int i = 0; i < paths.Length; i++)
        {
            fileOptions[i + 1] = Path.GetFileNameWithoutExtension(paths[i]);
        }
    }

    string FindFGAPathByName(string filename)
    {
        string[] paths = Directory.GetFiles(Application.dataPath, "*.fga", SearchOption.AllDirectories);
        foreach (var full in paths)
        {
            if (Path.GetFileNameWithoutExtension(full) == filename)
                return full;
        }
        return null;
    }
}
