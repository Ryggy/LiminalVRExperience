using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(ParticleSystem))]
public class FlowFieldParticleEmitter : MonoBehaviour
{
    public enum Emission
    {
        VectorOrigins,
        InsideField,
        Corners,
        Edges,
        Surface, 
        FaceXPositive,
        FaceXNegative,
        FaceYPositive,
        FaceYNegative,
        FaceZPositive,
        FaceZNegative
    };
    
    public FlowFieldVolume FlowFieldVolumeSource;
    public Emission EmissionType;
    [Range(0f, 1f)]
    public float Coverage = 1.0f;
    
    ParticleSystem ps;
    ParticleSystem.EmitParams emitParams;
    float emissionRate = 0;
    float timer = 0;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        ps.Stop();
        emitParams = new ParticleSystem.EmitParams();
        
        emitParams.startColor = ps.main.startColor.Evaluate(0.0f);
        emitParams.startSize = ps.main.startSize.Evaluate(0.0f);
        emitParams.startLifetime = ps.main.startLifetime.Evaluate(0.0f);
        emissionRate = 1f / ps.emission.rateOverTime.constant;
    }

   		void FixedUpdate()
		{
			if (FlowFieldVolumeSource == null)
				return;
			
			if (timer > emissionRate)
			{
				int numToEmit = (int) (timer / emissionRate);

				float invCoverage = 1.0f-Coverage;
				
				for (int i = 0; i < numToEmit; i++)
				{

					switch (EmissionType)
					{
						case Emission.VectorOrigins:
							
							Vector3 pos;
							Vector3 dir;
							FlowFieldVolumeSource.GetRandomVector(out pos, out dir);
								
							emitParams.position = pos;
							emitParams.velocity = dir;
							break;
						case Emission.InsideField:
							emitParams.position = FlowFieldVolumeSource.GetPointInField(invCoverage);
							emitParams.velocity = FlowFieldVolumeSource.GetVector(emitParams.position);
							break;
						case Emission.Corners:
							emitParams.position = FlowFieldVolumeSource.GetRandomCorner();
							emitParams.velocity = FlowFieldVolumeSource.GetVector(emitParams.position);
							break;
						case Emission.Edges:
							emitParams.position = FlowFieldVolumeSource.GetPointOnEdge(invCoverage);
							emitParams.velocity = FlowFieldVolumeSource.GetVector(emitParams.position);
							break;
						case Emission.Surface:
							emitParams.position = FlowFieldVolumeSource.GetPointOnVolume(invCoverage);
							emitParams.velocity = FlowFieldVolumeSource.GetVector(emitParams.position);
							break;
						case Emission.FaceXPositive:
							emitParams.position = FlowFieldVolumeSource.GetPointOnFace(5,invCoverage);
							emitParams.velocity = FlowFieldVolumeSource.GetVector(emitParams.position);
							break;
						case Emission.FaceXNegative:
							emitParams.position = FlowFieldVolumeSource.GetPointOnFace(3,invCoverage);
							emitParams.velocity = FlowFieldVolumeSource.GetVector(emitParams.position);
							break;
						case Emission.FaceYPositive:
							emitParams.position = FlowFieldVolumeSource.GetPointOnFace(4,invCoverage);
							emitParams.velocity = FlowFieldVolumeSource.GetVector(emitParams.position);
							break;
						case Emission.FaceYNegative:
							emitParams.position = FlowFieldVolumeSource.GetPointOnFace(2,invCoverage);
							emitParams.velocity = FlowFieldVolumeSource.GetVector(emitParams.position);
							break;
						case Emission.FaceZPositive:
							emitParams.position = FlowFieldVolumeSource.GetPointOnFace(1,invCoverage);
							emitParams.velocity = FlowFieldVolumeSource.GetVector(emitParams.position);
							break;
						case Emission.FaceZNegative:
							emitParams.position = FlowFieldVolumeSource.GetPointOnFace(0,invCoverage);
							emitParams.velocity = FlowFieldVolumeSource.GetVector(emitParams.position);
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}

					if (ps.main.simulationSpace == ParticleSystemSimulationSpace.Custom )
					{
						emitParams.position = ps.main.customSimulationSpace.worldToLocalMatrix.MultiplyPoint(emitParams.position);
						emitParams.velocity = ps.main.customSimulationSpace.worldToLocalMatrix.MultiplyVector(emitParams.velocity);
					}
					else if (ps.main.simulationSpace == ParticleSystemSimulationSpace.Local )
					{
						emitParams.position = transform.worldToLocalMatrix.MultiplyPoint(emitParams.position);
						emitParams.velocity = transform.worldToLocalMatrix.MultiplyVector(emitParams.velocity);
					}

					ps.Emit(emitParams, 1);
				}
				timer = 0;
			}
			timer += Time.deltaTime;
		}
	}