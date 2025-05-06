using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirElement : ElementBase
{
    public AirElement(FlowFieldTest owner) : base(owner) {}

    public override void GenerateField(Vector2[,] vectors, int cols, int rows, float scale, float increment, float zOffset)
    {
        float windSpeed = 1.0f; // Speed of the wind flow
        float windFrequency = 0.2f; // Frequency of the oscillations (wind turbulence)
        float turbulenceStrength = 0.5f; // How much the wind swirls

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                // Create a slight oscillating movement
                float windX = Mathf.Sin((x + zOffset * windSpeed) * windFrequency);
                float windY = Mathf.Cos((y + zOffset * windSpeed) * windFrequency);

                // Add some randomness to simulate air turbulence
                float randomX = windX + Random.Range(-turbulenceStrength, turbulenceStrength);
                float randomY = windY + Random.Range(-turbulenceStrength, turbulenceStrength);

                // Normalize to ensure smooth flow
                Vector2 windDir = new Vector2(randomX, randomY).normalized;

                vectors[x, y] = windDir;
            }
        }
    }

    public override void DrawGizmos(Vector2[,] vectors, int cols, int rows, float scale, FlowFieldTest flowFieldTest)
    {
        Gizmos.color = Color.white; // Air element can be represented by white or light color

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                Vector3 pos = new Vector3(x * scale, y * scale, 0) + flowFieldTest.transform.position;
                Gizmos.DrawLine(pos, pos + (Vector3)vectors[x, y] * scale); // Draw the flow vectors
            }
        }
    }
}