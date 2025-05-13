using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterElement : ElementBase
{
    
    public WaterElement(FlowFieldTest owner) : base(owner) {}

    public override void GenerateField(Vector2[,] vectors, int cols, int rows, float scale, float increment, float zOffset)
    {
        float waveSpeed = 1.5f;
        float waveFrequency = 0.3f;
        float waveAmplitude = 2.0f;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                float wave = Mathf.Sin((x + zOffset * waveSpeed) * waveFrequency) * waveAmplitude;
                Vector2 dir = new Vector2(0.5f, wave).normalized;
                vectors[x, y] = dir;
            }
        }
    }

    public override void DrawGizmos(Vector2[,] vectors, int cols, int rows, float scale, FlowFieldTest flowFieldTest)
    {
        Gizmos.color = Color.cyan;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                Vector3 pos = new Vector3(x * scale, y * scale, 0) + flowFieldTest.transform.position;
                Gizmos.DrawLine(pos, pos + (Vector3)vectors[x, y] * scale);
            }
        }
    }
    
}