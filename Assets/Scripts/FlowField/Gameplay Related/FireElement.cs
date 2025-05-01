using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireElement : ElementBase
{
    public FireElement(FlowFieldTest owner) : base(owner) {}

    public override void GenerateField(Vector2[,] vectors, int cols, int rows, float scale, float increment, float zOffset)
    {
        for (int y = 0; y < rows; y++)
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

    public override void DrawGizmos(Vector2[,] vectors, int cols, int rows, float scale, FlowFieldTest flowFieldTest)
    {
        Gizmos.color = owner.GetFlickeringColor();
        Vector3 offset = owner.transform.position;

        for (int y = 0; y < rows; y++)
        for (int x = 0; x < cols; x++)
            if (owner.IsInFireMask(x, y))
            {
                Vector3 pos = new Vector3(x * scale, y * scale, 0) + offset;
                Gizmos.DrawLine(pos, pos + (Vector3)vectors[x, y] * scale * 0.5f);
            }
    }
}