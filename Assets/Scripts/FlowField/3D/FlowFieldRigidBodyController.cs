using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Vector Field/Rigidbody Controller")]
[ExecuteInEditMode]
[RequireComponent(typeof(Rigidbody))]
public class FlowFieldRigidBodyController : MonoBehaviour
{
    [Range(0f, 1f)]
    public float Tightness=1.0f;
    [Range(0.05f, 10f)]
    public float MinimalInfluence = 0.05f;
    public float Multiplier = 1.0f;
    public bool AffectedByAllFF = true;
    public List<FlowFieldVolume> FFRestrictedList = new List<FlowFieldVolume>();

    Rigidbody rb;
    
    private void Awake()
    {
        rb = this.GetComponent<Rigidbody>();
    }
    
    private float hlslSmoothstep(float min, float max, float value)
    {
        float t = Mathf.Clamp01((value - min) / (max - min));
        return t * t * (3.0f - 2.0f * t);
    }
	
    void Update ()
    {
        Vector3 force;

        if (AffectedByAllFF)
            force = FlowFieldVolume.GetCombinedVectors(transform.position)*Multiplier;
        else
            force = FlowFieldVolume.GetCombinedVectorsRestricted(transform.position, FFRestrictedList)*Multiplier;

        float intensity = force.magnitude;
        float blendIntensity = hlslSmoothstep(0.0f, MinimalInfluence+0.0001f, intensity);

        float SQTightness = Tightness*Tightness;
			
        rb.velocity = Vector3.Lerp(rb.velocity, force, SQTightness*blendIntensity);
        rb.AddForce(force*(1.0f - SQTightness)*blendIntensity, ForceMode.Force);
    }
}
