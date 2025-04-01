using System.Collections.Generic;
using UnityEngine;

public class FlowFieldTest : MonoBehaviour
{
    public int cols = 60, rows = 40;
    public float scale = 10f;
    public float increment = 0.1f;
    private float zOffset = 0f;
    private Vector2[,] vectors;
    public bool showFlowField = true;

    void Start()
    {
        vectors = new Vector2[cols, rows];
        GenerateField();
    }

    void Update()
    {
        //GenerateField();
    }

    void GenerateField()
    {
        float yOffset = 0;
        for (int y = 0; y < rows; y++)
        {
            float xOffset = 0;
            for (int x = 0; x < cols; x++)
            {
                float angle = Mathf.PerlinNoise(xOffset, yOffset) * Mathf.PI * 2;
                vectors[x, y] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                xOffset += increment;
            }
            yOffset += increment;
        }
        zOffset += 0.004f;
    }

    public Vector2 GetForce(Vector2 position)
    {
        int x = Mathf.Clamp(Mathf.FloorToInt(position.x / scale), 0, cols - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(position.y / scale), 0, rows - 1);
        return vectors[x, y];
    }

    void OnDrawGizmos()
    {
        if (!showFlowField || vectors == null) return;
        Gizmos.color = Color.black;
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                Vector2 pos = new Vector2(x * scale, y * scale);
                Gizmos.DrawLine(pos, pos + vectors[x, y] * scale);
            }
        }
    }
}