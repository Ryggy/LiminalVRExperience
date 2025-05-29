using UnityEngine;

public interface IFlowFieldInfluenceSource
{
    Vector3 GetCombinedVectorAtPosition(Vector3 worldPosition);
}