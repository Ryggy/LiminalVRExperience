using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ElementBase
{
    protected FlowFieldTest owner;

    protected ElementBase(FlowFieldTest owner)
    {
        this.owner = owner;
    }

    public abstract void GenerateField(Vector2[,] vectors, int cols, int rows, float scale, float increment, float zOffset);

    public virtual void DrawGizmos(Vector2[,] vectors, int cols, int rows, float scale, FlowFieldTest flowFieldTest)
    {
        // Optional base gizmo logic
    }
}