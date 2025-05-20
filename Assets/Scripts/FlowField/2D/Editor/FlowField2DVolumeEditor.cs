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
            if (fileOptions[i] == Path.GetFileNameWithoutExtension(flow.fgaFileName))
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
            if (fileOptions == null || fileOptions.Length == 0 || selectedIndex >= fileOptions.Length)
            {
                Debug.LogWarning("No FGA files available or invalid index");
                return;
            }
            string filename = fileOptions[selectedIndex];

            if (filename != null)
            {
                flow.fgaFileName = filename + ".fga";
                flow.ReadFGA(flow.fgaFileName);
            }
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
}
