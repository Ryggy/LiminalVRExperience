using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalElement : IElement
{
    public void GenerateField(Vector2[,] vectors, int cols, int rows, float scale, float increment, float zOffset)
    {
        float yOffset = 0;
        for (int y = 0; y < rows; y++)
        {
            float xOffset = 0;
            for (int x = 0; x < cols; x++)
            {
                float angle = Mathf.PerlinNoise(xOffset, yOffset + zOffset) * Mathf.PI * 2;
                Vector2 baseDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 upwardBias = new Vector2(0, 1);
                vectors[x, y] = (baseDirection + upwardBias * 0.5f).normalized;

                xOffset += increment;
            }
            yOffset += increment;
        }
    }

    public void DrawGizmos(Vector2[,] vectors, int cols, int rows, float scale, FlowFieldTest owner)
    {
        // No overlay needed; base gizmos are enough
    }
}