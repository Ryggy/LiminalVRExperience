using UnityEngine;

public interface IParticleBlendingStrategy
{
    Vector3 ComputeVelocity(Vector3 currentVelocity, Vector3 force, float deltaTime);
}