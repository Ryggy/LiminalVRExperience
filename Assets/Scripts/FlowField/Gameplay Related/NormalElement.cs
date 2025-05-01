using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalElement : ElementBase
{
    public NormalElement(FlowFieldTest owner) : base(owner) {}

    public override void GenerateField(Vector2[,] vectors, int cols, int rows, float scale, float increment, float zOffset)
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

    public override void DrawGizmos(Vector2[,] vectors, int cols, int rows, float scale, FlowFieldTest flowFieldTest)
    {
        // // Optional: You can implement gizmo drawing for NormalElement here
        // Gizmos.color = Color.white;
        // Vector3 offset = owner.transform.position;
        //
        // for (int y = 0; y < rows; y++)
        // for (int x = 0; x < cols; x++)
        // {
        //     Vector3 pos = new Vector3(x * scale, y * scale, 0) + offset;
        //     Gizmos.DrawLine(pos, pos + (Vector3)vectors[x, y] * scale * 0.5f);
        // }
    }
}