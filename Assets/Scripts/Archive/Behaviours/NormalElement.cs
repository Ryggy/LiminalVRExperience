using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NormalElement creates a smooth flow field with slight upward motion.
/// </summary>
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
        // No debug visuals yet — can be added if needed
    }
}