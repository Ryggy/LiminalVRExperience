using UnityEngine;

public class FlowField2D
{
    private Vector2[,] field;
    private int cols, rows;
    private float scale;
    private float noiseScale;
    private Vector2 origin;

    public FlowField2D(int cols, int rows, float scale, float noiseScale, Vector2 origin)
    {
        this.cols = cols;
        this.rows = rows;
        this.scale = scale;
        this.noiseScale = noiseScale;
        this.origin = origin;

        field = new Vector2[cols, rows];
    }

    private void GenerateField(float offset)
    {
        for (int x = 0; x < cols; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                float angle = Mathf.PerlinNoise(
                    (x + offset) * noiseScale,
                    (y + offset) * noiseScale
                ) * Mathf.PI * 2f;
                field[x, y] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            }
        }
    }
    
    public void UpdateField(float zOffset)
    {
        GenerateField(zOffset);
    }
    
    
    public Vector2 GetFlowDirection(Vector2 worldPosition)
    {
        Vector2 localPos = worldPosition - origin;
        int x = Mathf.Clamp((int)(localPos.x / scale), 0, cols - 1);
        int y = Mathf.Clamp((int)(localPos.y / scale), 0, rows - 1);
        return field[x, y];
    }

    public Vector2[,] Vectors => field;
    public int Cols => cols;
    public int Rows => rows;
    public float Scale => scale;
}
