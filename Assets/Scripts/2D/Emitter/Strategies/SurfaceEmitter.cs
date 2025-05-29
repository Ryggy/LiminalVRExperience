using UnityEngine;

public class SurfaceEmitter : IEmitterStrategy
{
    public void Generate(FlowField2DVolume field, float coverage, out Vector3 position, out Vector3 velocity)
    {
        var edgeEmitter = new EdgesEmitter();
        edgeEmitter.Generate(field, coverage, out position, out velocity);
    }
}