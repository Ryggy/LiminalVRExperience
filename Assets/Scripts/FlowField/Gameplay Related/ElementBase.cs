using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all element types (e.g. fire, water, normal).
/// Holds a reference to the FlowFieldTest and defines required methods.
/// </summary>
public abstract class ElementBase
{
    // Reference to the system that owns this element (for settings, position, etc.)
    protected FlowFieldTest owner;

    // Constructor takes the owning FlowFieldTest
    protected ElementBase(FlowFieldTest owner)
    {
        this.owner = owner;
    }

    /// <summary>
    /// Each element must define how it generates its vector field.
    /// </summary>
    public abstract void GenerateField(Vector2[,] vectors, int cols, int rows, float scale, float increment, float zOffset);

    /// <summary>
    /// Optional: draw visual debug lines in the scene view.
    /// Can be overridden by child classes.
    /// </summary>
    public virtual void DrawGizmos(Vector2[,] vectors, int cols, int rows, float scale, FlowFieldTest flowFieldTest)
    {
        // No default gizmo drawing
    }
    
    // public virtual void DrawSelectedGizmos(Vector2[,] vectors, int cols, int rows, float scale) { }
    
    
    
}