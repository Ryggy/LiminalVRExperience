using UnityEngine;

public class FlowField : MonoBehaviour
{
    public enum FlowFieldType {random, wave, center_attract, center_repel, perlin};
    
    public int width = 20;
    public int height = 20;
    public float cellSize = 1f;
    public FlowFieldType flowFieldType = FlowFieldType.wave;
    
    private float[,] scalarField;
    private Vector2[,] flowVectors;
    
    void Start()
    {
        InitialiseGrid();
        UpdateFlowField();
    }

    void InitialiseGrid()
    {
        scalarField = new float[width, height];
        flowVectors = new Vector2[width, height];
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                switch (flowFieldType)
                {
                    case FlowFieldType.random:
                        scalarField[x, y] = Random.Range(-1f, 1f);
                        break;

                    case FlowFieldType.wave: // Creates a wavy pattern across the grid
                        scalarField[x, y] = Mathf.Sin(x * 0.5f) * Mathf.Cos(y * 0.5f);
                        break;

                    case FlowFieldType.center_attract: // Pulls flow toward center
                        float distToCenter = Vector2.Distance(new Vector2(x, y), new Vector2(width / 2, height / 2));
                        scalarField[x, y] = -distToCenter / (width / 2);
                        break;

                    case FlowFieldType.center_repel: // Pushes flow outward from center
                        float distFromCenter = Vector2.Distance(new Vector2(x, y), new Vector2(width / 2, height / 2));
                        scalarField[x, y] = distFromCenter / (width / 2);
                        break;
                    case FlowFieldType.perlin: 
                        var scaled_x = x * 0.1f;
                        var scaled_y = y * 0.1f;
                        
                        // Generate Perlin noise for each grid cell
                        var noise_val = Mathf.PerlinNoise(scaled_x, scaled_y);
                        
                        // Scale Perlin value to the desired range, e.g., between -1 and 1
                        scalarField[x, y] = Mathf.Lerp(-1f, 1f, noise_val);
                        break;
                    default:
                        scalarField[x, y] = 0; // Default empty field
                        break;
                }

                flowVectors[x, y] = Vector2.zero; // Reset vectors
            }
        }
    }

    public void UpdateFlowField()
    {
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                float dx = scalarField[x + 1, y] - scalarField[x - 1, y];
                float dy = scalarField[x, y + 1] - scalarField[x, y - 1];

                flowVectors[x, y] = new Vector2(dx, dy).normalized; // Normalize vector
            }
        }
    }

    public Vector2 GetFlowDirection(int x, int y)
    {
        return flowVectors != null ? flowVectors[x, y] : Vector2.zero;
    }

    public void ModifyScalarValue(int x, int y, float intensity)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            scalarField[x, y] += intensity;
            UpdateFlowField();
        }
        else
        {
            Debug.LogWarning($"Input location ({x},{y}) is out of bounds");
        }
    }
}