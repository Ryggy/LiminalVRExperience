using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

[System.Serializable]
public class FlowField2DWeight
{
    public FlowField2DVolume field;
    public float weight = 1.0f;
}

[AddComponentMenu("Vector Field/2D Particle Controller")]
[ExecuteInEditMode]
[RequireComponent(typeof(ParticleSystem))]
public class FlowField2DParticleController : MonoBehaviour
{
    [Range(0f, 1f)] public float Tightness = 0.5f;
    [Range(0.01f, 1f)] public float MinInfluence = 0.05f;
    public float Multiplier = 1f;

    public bool AffectedByAllFF = true;
    public List<FlowField2DWeight> FFRestrictedList = new List<FlowField2DWeight>();

    public bool AnimateTightness = false;
    public AnimationCurve TightnessOverTime = new AnimationCurve();

    public bool AnimateMultiplier = false;
    public AnimationCurve MultiplierOverTime = new AnimationCurve();
    // Particle System
    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private float hlslSmoothstep(float min, float max, float value)
    {
        float t = Mathf.Clamp01((value - min) / (max - min));
        return t * t * (3f - 2f * t);
    }
    
    void LateUpdate()
    {
        if (ps == null)
        {
            Debug.LogError("Particle system not assigned!");
            return;
        }
        
        int maxParticles = ps.main.maxParticles;
        if (particles == null || particles.Length != maxParticles)
            particles = new ParticleSystem.Particle[maxParticles];

        int alive = ps.GetParticles(particles);
        
        if (alive == 0)
        {
            Debug.Log("No particles alive.");
            return;
        }
        
        for (int i = 0; i < alive; i++)
        {
            Vector3 worldPos = particles[i].position;
            Vector2 force = AffectedByAllFF
                ? CombineAllForces(worldPos)
                : CombineRestrictedForces(worldPos);

            float relativeLife = 1.0f - (particles[i].remainingLifetime / particles[i].startLifetime);
            float intensity = force.magnitude;
            float blend = hlslSmoothstep(-0.0001f, MinInfluence, intensity);

            float tTightness = Tightness * Tightness;
            if (AnimateTightness) tTightness *= TightnessOverTime.Evaluate(relativeLife);
            if (AnimateMultiplier) force *= MultiplierOverTime.Evaluate(relativeLife);

            Vector2 vel = new Vector2(particles[i].velocity.x, particles[i].velocity.y);
            Vector2 newVel = Vector2.Lerp(vel + force * Time.deltaTime * blend, force, tTightness * blend);

            particles[i].velocity = new Vector3(newVel.x, newVel.y, 0);
        }

        ps.SetParticles(particles, alive);
    }
    
    Vector2 CombineAllForces(Vector2 worldPos)
    {
        Vector2 total = Vector2.zero;
        var all = Object.FindObjectsOfType<FlowField2DVolume>();
        foreach (var field in all)
            total += field.GetVectorAtWorldPosition(worldPos) * Multiplier;
        return total;
    }

    Vector2 CombineRestrictedForces(Vector2 worldPos)
    {
        Vector2 total = Vector2.zero;
        foreach (var entry in FFRestrictedList)
        {
            if (entry != null && entry.field != null)
                total += entry.field.GetVectorAtWorldPosition(worldPos) * entry.weight * Multiplier;
        }
        return total;
    }
}