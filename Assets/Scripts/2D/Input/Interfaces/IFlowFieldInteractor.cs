using UnityEngine;

public interface IFlowFieldInteractor
{
    void ApplyForce(Vector2 centerGridPos, Vector2 direction, int influenceRadius, FlowField2DVolume flowField);
    
    void ApplyForce(Vector2 centerGridPos, Vector2 direction, int influenceRadius, FlowFieldVolume flowField);
}
