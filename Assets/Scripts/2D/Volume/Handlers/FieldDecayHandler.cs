using UnityEngine;

public class FieldDecayHandler : IFieldDecayHandler
{
    public void ApplyDecay(Vector2[] working, Vector2[] original, float rate, float deltaTime)
    {
        if (working == null || original == null) return;
        for (int i = 0; i < working.Length; i++)
            working[i] = Vector2.Lerp(working[i], original[i], deltaTime * rate);
    }
}
