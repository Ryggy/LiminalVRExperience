using UnityEngine;

public interface IFlowField
{
    void GenerateField();
    void SetForce(Vector2 gridPos, Vector2 direction);
    Vector2 GetVectorAtWorldPosition(Vector2 worldPos);
    void ApplyDecay(float rate, float deltaTime);
}
