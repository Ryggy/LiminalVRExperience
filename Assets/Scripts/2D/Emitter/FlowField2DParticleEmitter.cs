using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(ParticleSystem))]
public class FlowField2DParticleEmitter : MonoBehaviour
{
    public enum Emission
    {
        VectorOrigins,
        InsideField,
        Corners,
        Edges,
        Surface,
    }

    public FlowField2DVolume FlowFieldVolumeSource;
    public Emission EmissionType;
    [Range(0f, 1f)] public float Coverage = 1.0f;

    private ParticleSystem ps;
    private ParticleSystem.EmitParams emitParams;
    private float emissionRate;
    private float timer = 0f;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        ps.Stop();

        emitParams = new ParticleSystem.EmitParams
        {
            startColor = ps.main.startColor.Evaluate(0),
            startSize = ps.main.startSize.Evaluate(0),
            startLifetime = ps.main.startLifetime.Evaluate(0)
        };

        emissionRate = 1f / ps.emission.rateOverTime.constant;
    }

    private void FixedUpdate()
    {
        if (FlowFieldVolumeSource == null)
            return;

        if (timer > emissionRate)
        {
            int emitCount = (int)(timer / emissionRate);
            float invCoverage = 1f - Coverage;

            var strategy = EmitterStrategyFactory.GetStrategy(EmissionType);

            for (int i = 0; i < emitCount; i++)
            {
                strategy.Generate(FlowFieldVolumeSource, invCoverage, out Vector3 pos, out Vector3 vel);

                emitParams.position = pos;
                emitParams.velocity = vel;

                ApplySimulationSpace(ref emitParams);

                ps.Emit(emitParams, 1);
            }

            timer = 0f;
        }

        timer += Time.deltaTime;
    }

    private void ApplySimulationSpace(ref ParticleSystem.EmitParams param)
    {
        var main = ps.main;
        if (main.simulationSpace == ParticleSystemSimulationSpace.Custom && main.customSimulationSpace != null)
        {
            var matrix = main.customSimulationSpace.worldToLocalMatrix;
            param.position = matrix.MultiplyPoint(param.position);
            param.velocity = matrix.MultiplyVector(param.velocity);
        }
        else if (main.simulationSpace == ParticleSystemSimulationSpace.Local)
        {
            var matrix = transform.worldToLocalMatrix;
            param.position = matrix.MultiplyPoint(param.position);
            param.velocity = matrix.MultiplyVector(param.velocity);
        }
    }
}
