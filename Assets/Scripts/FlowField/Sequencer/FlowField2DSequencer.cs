using UnityEngine;

[AddComponentMenu("FlowField/2D FlowField Sequencer")]
public class FlowField2DSequencer : MonoBehaviour
{
    public FlowField2DSequence sequence;
    public FlowField2DVolume field;
    public FlowField2DParticleController controller;
    public FlowField2DParticleEmitter emitter;

    private float timer;

    void Update()
    {
        if (sequence == null || sequence.keyframes.Length == 0) return;

        timer += Time.deltaTime;
        float playhead = sequence.loop ? timer % sequence.duration : Mathf.Min(timer, sequence.duration);

        // Find surrounding keyframes
        FlowField2DKeyframe prev = sequence.keyframes[0];
        FlowField2DKeyframe next = sequence.keyframes[sequence.keyframes.Length - 1];

        for (int i = 0; i < sequence.keyframes.Length - 1; i++)
        {
            if (sequence.keyframes[i + 1].time >= playhead)
            {
                prev = sequence.keyframes[i];
                next = sequence.keyframes[i + 1];
                break;
            }
        }

        float t = Mathf.InverseLerp(prev.time, next.time, playhead);

        // --- Flow Field Volume ---
        if (field != null && prev.modifyField)
        {
            field.animationSpeed = Mathf.Lerp(prev.animationSpeed, next.animationSpeed, t);
            field.vectorColor = Color.Lerp(prev.vectorColor, next.vectorColor, t);
            // You could also apply intensity here if it's a variable in your FlowField2DVolume
        }

        // --- Particle Controller ---
        if (controller != null && prev.modifyController)
        {
            controller.Tightness = prev.tightnessCurve.Evaluate(t);
            controller.Multiplier = prev.multiplierCurve.Evaluate(t);
        }

        // --- Particle Emitter ---
        if (emitter != null && prev.modifyEmitter)
        {
            emitter.coverage = Mathf.Lerp(prev.coverage, next.coverage, t);
        }
    }

    public void Restart()
    {
        timer = 0f;
    }
}