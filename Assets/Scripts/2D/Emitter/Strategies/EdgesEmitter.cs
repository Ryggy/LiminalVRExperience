using UnityEngine;

public class EdgesEmitter : IEmitterStrategy
{
    public void Generate(FlowField2DVolume field, float coverage, out Vector3 position, out Vector3 velocity)
    {
        int edge = Random.Range(0, 4);
        int x = 0, y = 0;

        switch (edge)
        {
            case 0: x = Random.Range(0, field.cols); y = 0; break;                      // Bottom
            case 1: x = Random.Range(0, field.cols); y = field.rows - 1; break;         // Top
            case 2: x = 0; y = Random.Range(0, field.rows); break;                      // Left
            case 3: x = field.cols - 1; y = Random.Range(0, field.rows); break;         // Right
        }

        int index = x + y * field.cols;
        Vector2 pos2D = new Vector2(x * field.cellSize, y * field.cellSize) + (Vector2)field.transform.position;
        Vector2 vel2D = field.vectorData[index];

        position = new Vector3(pos2D.x, pos2D.y, 0f);
        velocity = new Vector3(vel2D.x, vel2D.y, 0f);
    }
}