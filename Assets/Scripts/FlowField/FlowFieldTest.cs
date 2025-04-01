using UnityEngine;
using Valve.VR.Extras;

public class FlowFieldTest : MonoBehaviour
{
    public int cols = 60, rows = 40;
    public float scale = 10f;
    public float increment = 0.1f;
    private float zOffset = 0f;
    private Vector2[,] vectors;

    // Enum to choose the flow field layout
    public enum FlowFieldType
    {
        Random,
        Wave,
        CenterAttract,
        CenterRepel,
        Perlin
    }

    public FlowFieldType flowFieldType = FlowFieldType.Perlin; // Set default type

    public bool showFlowField = true;

    void Start()
    {
        vectors = new Vector2[cols, rows];
        GenerateField();
    }

    void Update()
    {
        // Optionally, regenerate the field if needed in the update (if dynamic changes are needed)
        // GenerateField();
    }

    void GenerateField()
    {
        float yOffset = 0;
        for (int y = 0; y < rows; y++)
        {
            float xOffset = 0;
            for (int x = 0; x < cols; x++)
            {
                float scalarValue = 0f;

                // Select the layout based on flowFieldType
                switch (flowFieldType)
                {
                    case FlowFieldType.Random:
                        scalarValue = Random.Range(-1f, 1f);
                        break;

                    case FlowFieldType.Wave:
                        scalarValue = Mathf.Sin(xOffset * 0.5f) * Mathf.Cos(yOffset * 0.5f);
                        break;

                    case FlowFieldType.CenterAttract:
                        float distToCenter = Vector2.Distance(new Vector2(x, y), new Vector2(cols / 2, rows / 2));
                        scalarValue = -distToCenter / (cols / 2); // Attract toward the center
                        break;

                    case FlowFieldType.CenterRepel:
                        float distFromCenter = Vector2.Distance(new Vector2(x, y), new Vector2(cols / 2, rows / 2));
                        scalarValue = distFromCenter / (cols / 2); // Repel from the center
                        break;

                    case FlowFieldType.Perlin:
                        var scaled_x = x * 0.1f;
                        var scaled_y = y * 0.1f;

                        // Generate Perlin noise for each grid cell
                        var noise_val = Mathf.PerlinNoise(scaled_x, scaled_y);

                        // Scale Perlin value to the desired range, e.g., between -1 and 1
                        scalarValue = Mathf.Lerp(-1f, 1f, noise_val);
                        break;

                    default:
                        scalarValue = 0;
                        break;
                }

                // Generate flow vectors based on scalar field values
                vectors[x, y] = new Vector2(Mathf.Cos(scalarValue * Mathf.PI), Mathf.Sin(scalarValue * Mathf.PI));

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
        // Adjust the position by subtracting the object's position
        Vector2 localPosition = position - new Vector2(transform.position.x, transform.position.y);

        // Calculate grid coordinates based on the adjusted local position
        int x = Mathf.Clamp(Mathf.FloorToInt(localPosition.x / scale), 0, cols - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(localPosition.y / scale), 0, rows - 1);
        
        vectors[x, y] = force;
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