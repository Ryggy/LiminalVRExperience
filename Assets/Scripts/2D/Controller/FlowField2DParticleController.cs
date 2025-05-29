using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Vector Field/2D Particle Controller")]
[ExecuteInEditMode]
[RequireComponent(typeof(ParticleSystem))]
public class FlowField2DParticleController : MonoBehaviour
{
    [Header("Flow Field Settings")]
    public bool AffectedByAllFF = true;
    public List<FlowField2DVolume> FFRestrictedList = new List<FlowField2DVolume>();

    [Header("Blending Settings")]
    [Range(0f, 1f)] public float Tightness = 1f;
    [Range(0.05f, 10f)] public float MinimalInfluence = 0.05f;
    public float Multiplier = 1f;

    private ParticleSystem _ps;
    private ParticleSystem.Particle[] _particles;
    private IFlowFieldInfluenceSource _fieldSource;
    private IParticleBlendingStrategy _blendingStrategy;

    void Awake()
    {
        _ps = GetComponent<ParticleSystem>();
        InitStrategies();
    }
    
    private void InitStrategies()
    {
        _fieldSource = new DefaultFlowFieldInfluenceSource(AffectedByAllFF, FFRestrictedList);
        _blendingStrategy = new BasicBlendingStrategy(Tightness, MinimalInfluence);
    }
    
    private void Update()
    {
        if (_particles == null || _ps.main.maxParticles != _particles.Length)
            _particles = new ParticleSystem.Particle[_ps.main.maxParticles];

        int numParticles = _ps.GetParticles(_particles);
        var simSpace = _ps.main.simulationSpace;
        var l2w = transform.localToWorldMatrix;
        var w2l = transform.worldToLocalMatrix;

        if (simSpace == ParticleSystemSimulationSpace.Custom && _ps.main.customSimulationSpace != null)
        {
            l2w = _ps.main.customSimulationSpace.localToWorldMatrix;
            w2l = _ps.main.customSimulationSpace.worldToLocalMatrix;
        }

        for (int i = 0; i < numParticles; i++)
        {
            Vector3 worldPos = simSpace == ParticleSystemSimulationSpace.World
                ? _particles[i].position
                : l2w.MultiplyPoint(_particles[i].position);

            Vector3 force = _fieldSource.GetCombinedVectorAtPosition(worldPos) * Multiplier;
            Vector3 newVel = _blendingStrategy.ComputeVelocity(_particles[i].velocity, force, Time.smoothDeltaTime);

            _particles[i].velocity = simSpace == ParticleSystemSimulationSpace.World
                ? newVel
                : w2l.MultiplyVector(newVel);
        }

        _ps.SetParticles(_particles, numParticles);
    }
}