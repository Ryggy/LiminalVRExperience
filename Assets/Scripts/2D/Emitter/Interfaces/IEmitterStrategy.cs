using UnityEngine;

public interface IEmitterStrategy
{
    void Generate(FlowField2DVolume field, float coverage, out Vector3 position, out Vector3 velocity);
}