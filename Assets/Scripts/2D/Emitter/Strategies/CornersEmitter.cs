using UnityEngine;

public class CornersEmitter : IEmitterStrategy
{
    public void Generate(FlowField2DVolume field, float coverage, out Vector3 position, out Vector3 velocity)
    {
        Vector2[] corners =
        {
            new Vector2(0, 0),
            new Vector2(field.cols - 1, 0),
            new Vector2(0, field.rows - 1),
            new Vector2(field.cols - 1, field.rows - 1)
        };

        Vector2 grid = corners[Random.Range(0, corners.Length)];
        int index = (int)(grid.x + grid.y * field.cols);

        Vector2 pos2D = new Vector2(grid.x * field.cellSize, grid.y * field.cellSize) + (Vector2)field.transform.position;
        Vector2 vel2D = field.vectorData[index];

        position = new Vector3(pos2D.x, pos2D.y, 0f);
        velocity = new Vector3(vel2D.x, vel2D.y, 0f);
    }
}