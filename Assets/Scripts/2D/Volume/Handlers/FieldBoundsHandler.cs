using UnityEngine;

public class FieldBoundsHandler : IFieldBoundsHandler
{
    public Vector2 ClampToBounds(Vector2 worldPos, Vector2 fieldOrigin, int cols, int rows, float cellSize,
        BoundsType2D xBounds,
        BoundsType2D yBounds,
        float ramp, bool perAxis)
    {
        Vector2 local = worldPos - fieldOrigin;
        Vector2 rel = new Vector2(local.x / (cols * cellSize), local.y / (rows * cellSize));
        float intensity = 1f;

        rel.x = ApplyBounds(rel.x, perAxis ? xBounds : BoundsType2D.Closed, ref intensity, ramp);
        rel.y = ApplyBounds(rel.y, perAxis ? yBounds : BoundsType2D.Closed, ref intensity, ramp);

        int x = Mathf.Clamp(Mathf.FloorToInt(rel.x * cols), 0, cols - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(rel.y * rows), 0, rows - 1);

        return new Vector2(x, y);
    }

    private float ApplyBounds(float rel, BoundsType2D mode, ref float intensity, float ramp)
    {
        switch (mode)
        {
            case BoundsType2D.Closed:
                if (rel < 0f || rel > 1f)
                    return 0f;
                if (ramp > 0f)
                {
                    float t = Mathf.Min(rel, 1f - rel) / ramp;
                    intensity *= Mathf.Clamp01(t);
                }
                return rel;
            case BoundsType2D.Border: return Mathf.Clamp01(rel);
            case BoundsType2D.Repeat: return rel - Mathf.Floor(rel);
            case BoundsType2D.Mirror:
                rel = (rel * 0.5f - Mathf.Floor(rel * 0.5f)) * 2.0f;
                if (rel > 1.0f) rel = 2f - rel;
                return rel;
            default: return rel;
        }
    }
}