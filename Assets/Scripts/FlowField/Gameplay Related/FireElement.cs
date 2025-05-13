using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FireElement creates upward flickering flow, simulating flames.
/// </summary>
public class FireElement : ElementBase
{
    public FireElement(FlowFieldTest owner) : base(owner) {}

    // public override void GenerateField(Vector2[,] vectors, int cols, int rows, float scale, float increment, float zOffset)
    // {
    //     for (int y = 0; y < rows; y++)
    //     for (int x = 0; x < cols; x++)
    //     {
    //         // Only apply fire behavior inside the fire mask
    //         if (owner.IsInFireMask(x, y))
    //         {
    //             // Add random side flicker using Perlin noise
    //             float flicker = (Mathf.PerlinNoise(x * 0.1f, Time.time * 0.5f) - 0.5f) * 0.3f;
    //             vectors[x, y] = new Vector2(flicker, 1f).normalized; // Flicker + upward flow
    //         }
    //         else
    //         {
    //             vectors[x, y] = Vector2.up; // Default flow
    //         }
    //     }
    // }
    
    public override void GenerateField(Vector2[,] vectors, int cols, int rows, float scale, float increment, float zOffset)
    {
        float yOffset = 0;

        for (int y = 0; y < rows; y++)
        {
            float xOffset = 0;

            for (int x = 0; x < cols; x++)
            {
                // Use Perlin noise to get a direction
                float angle = Mathf.PerlinNoise(xOffset, yOffset + zOffset) * Mathf.PI * 2;

                // Create a direction vector from the angle
                Vector2 baseDir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                // Add a slight upward push
                Vector2 up = new Vector2(0, 1);

                // Combine and normalize
                vectors[x, y] = (baseDir + up * 0.5f).normalized;

                xOffset += increment;
            }

            yOffset += increment;
        }
    }

    public override void DrawGizmos(Vector2[,] vectors, int cols, int rows, float scale, FlowFieldTest flowFieldTest)
    {
        Gizmos.color = owner.GetFlickeringColor();
        Vector3 offset = owner.transform.position;

        for (int y = 0; y < rows; y++)
        for (int x = 0; x < cols; x++)
        {
            if (owner.IsInFireMask(x, y))
            {
                Vector3 start = new Vector3(x * scale, y * scale, 0) + offset;
                Vector3 end = start + (Vector3)vectors[x, y] * scale * 0.5f;
                Gizmos.DrawLine(start, end); // Show direction of flow
            }
        }
    }
}