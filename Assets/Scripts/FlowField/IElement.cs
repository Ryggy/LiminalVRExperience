
using UnityEngine;

public interface  IElement
{
    // Called once per frame (or whenever you regenerate)
    void GenerateField(Vector2[,] vectors, int cols, int rows, float scale, float increment, float zOffset);

    // Called in OnDrawGizmos
    void DrawGizmos(Vector2[,] vectors, int cols, int rows, float scale, FlowFieldTest flickerColor);
}