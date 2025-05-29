using UnityEngine;

public class BasicBlendingStrategy : IParticleBlendingStrategy
{
    private readonly float _tightness;
    private readonly float _minInfluence;

    public BasicBlendingStrategy(float tightness, float minInfluence)
    {
        _tightness = tightness * tightness;
        _minInfluence = minInfluence;
    }

    private float SmoothBlend(float intensity)
    {
        float t = Mathf.Clamp01((intensity - (-0.0001f)) / (_minInfluence - (-0.0001f)));
        return t * t * (3f - 2f * t);
    }

    public Vector3 ComputeVelocity(Vector3 currentVelocity, Vector3 force, float deltaTime)
    {
        float blend = SmoothBlend(force.magnitude);
        return Vector3.Lerp(currentVelocity + force * deltaTime * blend, force, _tightness * blend);
    }
}