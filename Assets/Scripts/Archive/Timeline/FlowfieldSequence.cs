using UnityEngine;

public class FlowField2DSequence : ScriptableObject
{
    public FlowField2DKeyframe[] keyframes;
    public float duration = 10f;
    public bool loop = true;
}