using System.Collections.Generic;
using UnityEngine;

public class DefaultFlowFieldInfluenceSource : IFlowFieldInfluenceSource
{
    private readonly bool _useAllFields;
    private readonly List<FlowField2DVolume> _restrictedFields;

    public DefaultFlowFieldInfluenceSource(bool useAll, List<FlowField2DVolume> restricted = null)
    {
        _useAllFields = useAll;
        _restrictedFields = restricted ?? new List<FlowField2DVolume>();
    }

    public Vector3 GetCombinedVectorAtPosition(Vector3 worldPosition)
    {
        Vector2 vec = _useAllFields
            ? FlowField2DVolume.GetCombinedVectors(worldPosition)
            : FlowField2DVolume.GetCombinedVectorsRestricted(worldPosition, _restrictedFields);
        
        return new Vector3(vec.x, vec.y, 0f);
    }
}