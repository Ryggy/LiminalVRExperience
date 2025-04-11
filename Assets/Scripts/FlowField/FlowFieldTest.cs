using System.Collections.Generic;
using UnityEngine;
using Valve.VR.Extras;

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
        // Adjust the position by subtracting the object's position
        Vector2 localPosition = position - new Vector2(transform.position.x, transform.position.y);
        
        // Calculate grid coordinates based on the adjusted local position
        int x = Mathf.Clamp(Mathf.FloorToInt(localPosition.x / scale), 0, cols - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(localPosition.y / scale), 0, rows - 1);

        return vectors[x, y];
    }

    public void SetForce(Vector2 position, Vector2 force)
    {
        vectors[(int)position.x, (int)position.y] = force;
    }

    void OnDrawGizmos()
    {
        if (!showFlowField || vectors == null) return;
        Gizmos.color = Color.black;
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                // Adjust the position to be relative to the transform position
                Vector3 pos = new Vector3(x * scale, y * scale, 0) + new Vector3(transform.position.x, transform.position.y, transform.position.z);
                Gizmos.DrawLine(pos, pos + (Vector3)vectors[x, y] * scale);
            }
        }
    }
}