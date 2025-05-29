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
    public BoundsType2D bounds = BoundsType2D.Closed;
    public BoundsType2D boundsX = BoundsType2D.Closed;
    public BoundsType2D boundsY = BoundsType2D.Closed;
    [Range(0f, 1f)] public float closedBoundRamp = 0f;
    
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
    public FlowField2DParticleEmitter.Emission emissionType = FlowField2DParticleEmitter.Emission.VectorOrigins;
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

            modifyController = modifyController,
            tightness = tightness,
            MinInfluence = MinInfluence,
            multiplier = multiplier,

            modifyEmitter = modifyEmitter,
            emissionType = emissionType,
            coverage = coverage
        };

        return ScriptPlayable<FlowField2DPlayableBehaviour>.Create(graph, behaviour);
    }
}
