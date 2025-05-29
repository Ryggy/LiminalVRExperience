using UnityEngine;

public class VectorOriginEmitter : IEmitterStrategy
{
    public void Generate(FlowField2DVolume field, float coverage, out Vector3 position, out Vector3 velocity)
    {
        int x = Random.Range(0, field.cols);
        int y = Random.Range(0, field.rows);

        int index = x + y * field.cols;
        Vector2 pos2D = new Vector2(x * field.cellSize, y * field.cellSize) + (Vector2)field.transform.position;
        Vector2 vel2D = field.vectorData[index];

        position = new Vector3(pos2D.x, pos2D.y, 0f);
        velocity = new Vector3(vel2D.x, vel2D.y, 0f);
    }
}