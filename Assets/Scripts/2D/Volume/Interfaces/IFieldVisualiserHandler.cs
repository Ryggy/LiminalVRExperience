using UnityEngine;
public interface IFieldVisualiserHandler
{
    void DrawGizmos(Vector2[] workingVectorData,
        int cols, int rows, float cellSize, Transform transform, bool showGizmos, Color vectorColor, float gizmoScale);
}
