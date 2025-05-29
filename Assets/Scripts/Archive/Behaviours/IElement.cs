
using UnityEngine;

public interface  IElement
{
    void GenerateField(Vector2[,] vectors, int cols, int rows, float scale, float increment, float zOffset);

    void DrawGizmos(Vector2[,] vectors, int cols, int rows, float scale, FlowFieldTest flickerColor);
}