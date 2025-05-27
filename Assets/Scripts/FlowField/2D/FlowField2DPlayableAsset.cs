using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class FlowField2DPlayableAsset : PlayableAsset
{
    // FlowField2DVolume Parameters
    public bool modifyField = true;

    // Grid
    public int cols = 60;
    public int rows = 40;
    public float cellSize = 10f;
    public float intensity = 1f;

    // Bounds
    public bool perAxisBounds = false;
    public FlowField2DVolume.BoundsType2D bounds = FlowField2DVolume.BoundsType2D.Closed;
    public FlowField2DVolume.BoundsType2D boundsX = FlowField2DVolume.BoundsType2D.Closed;
    public FlowField2DVolume.BoundsType2D boundsY = FlowField2DVolume.BoundsType2D.Closed;
    [Range(0f, 1f)] public float closedBoundRamp = 0f;

    // Animate
    public bool animateField = false;
    public AnimationCurve animationCurve = AnimationCurve.Linear(0, 1, 1, 1);
    public float animationSpeed = 1f;
    public float increment = 0.1f;

    // Gizmo
    public bool showGizmos = true;
    public Color vectorColor = Color.yellow;
    public float gizmoScale = 1f;

    // Particle Controller Parameters
    public bool modifyController = true;
    [Range(0f, 1f)] public float tightness = 1f;
    [Range(0.01f, 1f)] public float MinInfluence = 0.05f;
    public float multiplier = 1f;

    public bool AnimateTightness = false;
    public AnimationCurve TightnessOverTime = AnimationCurve.Linear(0, 1, 1, 1);

    public bool AnimateMultiplier = false;
    public AnimationCurve MultiplierOverTime = AnimationCurve.Linear(0, 1, 1, 1);

    // Particle Emitter Parameters
    public bool modifyEmitter = true;
    public FlowField2DParticleEmitter.EmissionType2D emissionType = FlowField2DParticleEmitter.EmissionType2D.VectorOrigins;
    [Range(0f, 1f)] public float coverage = 1f;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var behaviour = new FlowField2DPlayableBehaviour
        {
            modifyField = modifyField,
            cols = cols,
            rows = rows,
            cellSize = cellSize,
            intensity = intensity,

            perAxisBounds = perAxisBounds,
            bounds = bounds,
            boundsX = boundsX,
            boundsY = boundsY,
            closedBoundRamp = closedBoundRamp,

            animateField = animateField,
            animationCurve = animationCurve,
            animationSpeed = animationSpeed,
            increment = increment,

            showGizmos = showGizmos,
            vectorColor = vectorColor,
            gizmoScale = gizmoScale,

            modifyController = modifyController,
            tightness = tightness,
            MinInfluence = MinInfluence,
            multiplier = multiplier,
            AnimateTightness = AnimateTightness,
            TightnessOverTime = TightnessOverTime,
            AnimateMultiplier = AnimateMultiplier,
            MultiplierOverTime = MultiplierOverTime,

            modifyEmitter = modifyEmitter,
            emissionType = emissionType,
            coverage = coverage
        };

        return ScriptPlayable<FlowField2DPlayableBehaviour>.Create(graph, behaviour);
    }
}
