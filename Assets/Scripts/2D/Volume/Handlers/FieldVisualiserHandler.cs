using UnityEngine;

public class FieldVisualiserHandler : IFieldVisualiserHandler
{
    public void DrawGizmos(Vector2[] workingVectorData,
        int cols, int rows, float cellSize, Transform transform, bool showGizmos, Color vectorColor, float gizmoScale)
    {
        if (!showGizmos || workingVectorData == null) return;
        
        Gizmos.color = vectorColor;

        for (int y = 0; y < rows; y++)
        for (int x = 0; x < cols; x++)
        {
            int index = x + y * cols;
            if (index < 0 || index >= workingVectorData.Length) continue;
            
            Vector2 dir = workingVectorData[index];
            if (dir.sqrMagnitude < 0.001f) continue;

            Vector3 pos = new Vector3(x * cellSize, y * cellSize, 0) + transform.position;
            Gizmos.DrawRay(pos, (Vector3)dir.normalized * cellSize * gizmoScale);
        }
    }
}
