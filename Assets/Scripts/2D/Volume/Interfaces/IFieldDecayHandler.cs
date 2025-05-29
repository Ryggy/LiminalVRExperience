using UnityEngine;

public interface IFieldDecayHandler
{
    void ApplyDecay(Vector2[] working, Vector2[] original, float rate, float deltaTime);
}
