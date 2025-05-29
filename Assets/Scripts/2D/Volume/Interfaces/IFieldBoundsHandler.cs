using UnityEngine;

public interface IFieldBoundsHandler
{
    Vector2 ClampToBounds(Vector2 worldPos, Vector2 fieldOrigin, int cols, int rows, float cellSize,
        BoundsType2D xBounds,
        BoundsType2D yBounds,
        float ramp, bool perAxis);
}
