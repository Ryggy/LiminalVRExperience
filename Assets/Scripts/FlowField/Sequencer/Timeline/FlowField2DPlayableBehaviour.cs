using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

// A behaviour that is attached to a playable
public class FlowField2DPlayableBehaviour : PlayableBehaviour
{
    // Flow field volume parameters
    // Grid
    public bool modifyField;
    public int cols;
    public int rows;
    public float cellSize;
    
    public float intensity;
    
    // Bounds
    public bool perAxisBounds = false;
    public FlowField2DVolume.BoundsType2D bounds;
    public FlowField2DVolume.BoundsType2D boundsX;
    public FlowField2DVolume.BoundsType2D boundsY;
    [Range(0f, 1f)] public float closedBoundRamp;

    // Animate
    public bool animateField;
    public AnimationCurve animationCurve = new AnimationCurve();
    public float animationSpeed;
    public float increment;
    
    // Gizmos
    public bool showGizmos;
    public Color vectorColor;
    public float gizmoScale;

    // -----------------------------
    // Particle controller parameters
    public bool modifyController;
    [Range(0f, 1f)] public float tightness;
    [Range(0.01f, 1f)] public float MinInfluence;
    public float multiplier;
    
    public bool AnimateTightness;
    public AnimationCurve TightnessOverTime = new AnimationCurve();

    public bool AnimateMultiplier;
    public AnimationCurve MultiplierOverTime = new AnimationCurve();
    // -----------------------------
    // Particle emitter parameters
    public bool modifyEmitter;
    public FlowField2DParticleEmitter.EmissionType2D emissionType;
    [Range(0f, 1f)] public float coverage;
    

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        var sequencer = playerData as FlowField2DSequencer;
        if (sequencer == null) return;

        if (modifyField && sequencer.field != null)
        {
            int oldHash = sequencer.field.GetFieldConfigHash();
            
            var field = sequencer.field;
            field.cols = cols;
            field.rows = rows;
            field.cellSize = cellSize;
            field.increment = increment;
            field.vectorColor = vectorColor;
           
            field.animationSpeed = animationSpeed;
            field.animateField = animateField;
            field.animationCurve = animationCurve;

            field.perAxisBounds = perAxisBounds;
            field.bounds = bounds;
            field.boundsX = boundsX;
            field.boundsY = boundsY;
            field.closedBoundRamp = closedBoundRamp;

            field.showGizmos = showGizmos;
            field.gizmoScale = gizmoScale;

            int newHash = sequencer.field.GetFieldConfigHash();
            if (oldHash != newHash)
            {
                sequencer.field.GenerateField();
            }
        }

        // -----------------------------
        // Particle Controller
        if (modifyController && sequencer.controller != null)
        {
            var controller = sequencer.controller;
            controller.Tightness = tightness;
            controller.MinInfluence = MinInfluence;
            controller.Multiplier = multiplier;

            controller.AnimateTightness = AnimateTightness;
            controller.TightnessOverTime = TightnessOverTime;

            controller.AnimateMultiplier = AnimateMultiplier;
            controller.MultiplierOverTime = MultiplierOverTime;
        }

        // -----------------------------
        // Particle Emitter
        if (modifyEmitter && sequencer.emitter != null)
        {
            var emitter = sequencer.emitter;
            emitter.emissionType = emissionType;
            emitter.coverage = coverage;
        }
    }
}
