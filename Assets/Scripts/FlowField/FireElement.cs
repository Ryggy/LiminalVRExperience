using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireElement : IElement
{
    private FlowFieldTest owner;

    public FireElement(FlowFieldTest owner) { this.owner = owner; }

    public void GenerateField(Vector2[,] vectors, int cols, int rows, float scale, float increment, float zOffset)
    {
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                if (owner.IsInFireMask(x, y))
                {
                    float jitter = (Mathf.PerlinNoise(x * 0.1f, Time.time * 0.5f) - 0.5f) * 0.3f;
                    vectors[x, y] = new Vector2(jitter, 1f).normalized;
                }
                else
                {
                    vectors[x, y] = Vector2.up;
                }
            }
        }
    }

    public void DrawGizmos(Vector2[,] vectors, int cols, int rows, float scale, FlowFieldTest owner)
    {
        Gizmos.color = owner.GetFlickeringColor();
        for (int y = 0; y < rows; y++)
        for (int x = 0; x < cols; x++)
            if (owner.IsInFireMask(x, y))
            {
                Vector2 pos = new Vector2(x * scale, y * scale);
                Gizmos.DrawLine(pos, pos + vectors[x, y] * scale * 0.5f);
            }
    }
}