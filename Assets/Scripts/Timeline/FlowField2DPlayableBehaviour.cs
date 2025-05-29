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
    public BoundsType2D bounds;
    public BoundsType2D boundsX;
    public BoundsType2D boundsY;
    [Range(0f, 1f)] public float closedBoundRamp;

    // -----------------------------
    // Particle controller parameters
    public bool modifyController;
    [Range(0f, 1f)] public float tightness;
    [Range(0.01f, 1f)] public float MinInfluence;
    public float multiplier;
    
    // -----------------------------
    // Particle emitter parameters
    public bool modifyEmitter;
    public FlowField2DParticleEmitter.Emission emissionType;
    [Range(0f, 1f)] public float coverage;
    

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        var sequencer = playerData as FlowField2DSequencer;
        if (sequencer == null) return;

        if (modifyField && sequencer.field != null)
        {
           
            var field = sequencer.field;
            field.cols = cols;
            field.rows = rows;
            field.cellSize = cellSize;

            field.perAxisBounds = perAxisBounds;
            field.bounds = bounds;
            field.boundsX = boundsX;
            field.boundsY = boundsY;
            field.closedBoundRamp = closedBoundRamp;
        }

        // -----------------------------
        // Particle Controller
        if (modifyController && sequencer.controller != null)
        {
            var controller = sequencer.controller;
            controller.Tightness = tightness;
            controller.MinimalInfluence = MinInfluence;
            controller.Multiplier = multiplier;
        }

        // -----------------------------
        // Particle Emitter
        if (modifyEmitter && sequencer.emitter != null)
        {
            var emitter = sequencer.emitter;
            emitter.EmissionType = emissionType;
            emitter.Coverage = coverage;
        }
    }
}
