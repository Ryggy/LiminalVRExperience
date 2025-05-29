using UnityEngine;

public class FlowField2DKeyframe : ScriptableObject
{
    public float time;

    [Header("Flow Field Volume")]
    public bool modifyField = false;
    public float animationSpeed = 1.0f;
    public Color vectorColor = Color.yellow;
    public float intensity = 1.0f;

    [Header("Particle Controller")]
    public bool modifyController = false;
    public AnimationCurve tightnessCurve = AnimationCurve.Constant(0, 1, 1f);
    public AnimationCurve multiplierCurve = AnimationCurve.Constant(0, 1, 1f);

    [Header("Particle Emitter")]
    public bool modifyEmitter = false;
    [Range(0f, 1f)] public float coverage = 1.0f;
}