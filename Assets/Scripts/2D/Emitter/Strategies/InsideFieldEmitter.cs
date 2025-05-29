using UnityEngine;

public class InsideFieldEmitter : IEmitterStrategy
{
    public void Generate(FlowField2DVolume field, float coverage, out Vector3 position, out Vector3 velocity)
    {
        float inv = 1f - coverage;
        int minX = Mathf.FloorToInt(field.cols * inv * 0.5f);
        int minY = Mathf.FloorToInt(field.rows * inv * 0.5f);
        int maxX = field.cols - minX;
        int maxY = field.rows - minY;

        int x = Random.Range(minX, maxX);
        int y = Random.Range(minY, maxY);
        int index = x + y * field.cols;

        Vector2 pos2D = new Vector2(x * field.cellSize, y * field.cellSize) + (Vector2)field.transform.position;
        Vector2 vel2D = field.vectorData[index];

        position = new Vector3(pos2D.x, pos2D.y, 0f);
        velocity = new Vector3(vel2D.x, vel2D.y, 0f);
    }
}