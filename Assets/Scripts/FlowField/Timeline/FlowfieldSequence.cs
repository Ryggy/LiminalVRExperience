using UnityEngine;

[CreateAssetMenu(fileName = "NewFlowField2DSequence", menuName = "FlowField/FlowField 2D Sequence")]
public class FlowField2DSequence : ScriptableObject
{
    public FlowField2DKeyframe[] keyframes;
    public float duration = 10f;
    public bool loop = true;
}