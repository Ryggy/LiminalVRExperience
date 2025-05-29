using UnityEngine;

public class FlowFieldInteractor2D : IFlowFieldInteractor
{
    public void ApplyForce(Vector2 centerGridPos, Vector2 direction, int influenceRadius, FlowField2DVolume flowField)
    {
        int cx = (int)centerGridPos.x;
        int cy = (int)centerGridPos.y;

        for (int y = -influenceRadius; y <= influenceRadius; y++)
        {
            for (int x = -influenceRadius; x <= influenceRadius; x++)
            {
                if (x * x + y * y > influenceRadius * influenceRadius)
                    continue;

                int tx = cx + x;
                int ty = cy + y;

                if (tx >= 0 && tx < flowField.cols && ty >= 0 && ty < flowField.rows)
                    flowField.SetForce(new Vector2(tx, ty), direction);
            }
        }
    }

    public void ApplyForce(Vector2 centerGridPos, Vector2 direction, int influenceRadius, FlowFieldVolume flowField)
    {
        throw new System.NotImplementedException();
    }
}