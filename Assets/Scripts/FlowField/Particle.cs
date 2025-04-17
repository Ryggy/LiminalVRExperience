using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle
{
    public Vector2 Position;
    private Vector2 velocity;
    private Vector2 acceleration;
    private float maxSpeed;
    private ParticleSystem.Particle particle;
    private Color colour;

    // Element zone-related variables
    private bool isStuck = false;
    private bool wasInElementZoneLastFrame = false;
    private Color originalColor;
    private Vector2 stuckPosition;
    private float randomSeed;
    private float elementZoneAge = 0f;
    private const float elementZoneLifespan = 3f;
    private bool shrinking = false;
    private static HashSet<Vector2> takenElementZonePositions = new HashSet<Vector2>();
    // variables

    public Particle(FlowFieldManager flowFieldManager, Vector2 start, float maxSpeed)
    {
        this.maxSpeed = maxSpeed;
        Position = start;
        velocity = Vector2.zero;
        acceleration = Vector2.zero;
        colour = flowFieldManager.colourOptions[Random.Range(0, flowFieldManager.colourOptions.Length)];
        originalColor = colour;

        particle = new ParticleSystem.Particle
        {
            startSize = 0.1f,
            startColor = colour,
            position = Position
        };

        randomSeed = Random.Range(0f, 1000f);
    }

    public void Update(FlowFieldTest field)
    {
        bool isInElementZoneNow = CheckElementZone(field);

        if (!isInElementZoneNow && wasInElementZoneLastFrame)
            ExitElementZone();

        wasInElementZoneLastFrame = isInElementZoneNow;

        if (isStuck)
            UpdateElementZoneEffect();
        else
            UpdateMovement();
    }

    public void ApplyForce(Vector2 force) => acceleration += force;

    public void Edges(float width, float height)
    {
        if (Position.x > width || Position.x < 0 || Position.y > height || Position.y < 0)
            Position = new Vector2(Random.Range(0, width), Random.Range(0, height));
    }

    public void Follow(FlowFieldTest field)
    {
        Vector2 force = field.GetForce(Position);
        ApplyForce(force);
    }

    public ParticleSystem.Particle GetParticle() => particle;

    // ---------- Internal Helpers ----------

    private bool CheckElementZone(FlowFieldTest field)
    {
        if (field == null) return false;

        int x = Mathf.FloorToInt(Position.x / field.scale);
        int y = Mathf.FloorToInt(Position.y / field.scale);

        if (field.outlineType != FlowFieldTest.OutlineType.Fire || !field.IsInFireMask(x, y))
            return false; // for fire zone

        if (!isStuck)
        {
            stuckPosition = GetUniqueElementZonePosition(field);
            Position = stuckPosition;
            isStuck = true;
            elementZoneAge = 0f;
            shrinking = false;
        }

        particle.startColor = field.GetFlickeringColor();
        return true;
    }

    private void ExitElementZone()
    {
        isStuck = false;
        particle.startColor = originalColor;
        particle.startSize = 0.1f;
    }

    private void UpdateMovement()
    {
        velocity += acceleration;
        velocity = Vector2.ClampMagnitude(velocity, maxSpeed);
        Position += velocity;
        acceleration *= 0;
        particle.position = Position;
    }

    private void UpdateElementZoneEffect()
    {
        elementZoneAge += Time.deltaTime;

        // Flicker movement
        float flickerSpeed = 3f;
        float flickerAmount = 0.3f;

        Vector2 flicker = new Vector2(
            Mathf.Sin(Time.time * flickerSpeed + randomSeed) * flickerAmount,
            Mathf.Cos(Time.time * flickerSpeed + randomSeed) * flickerAmount
        );

        Vector2 upwardDrift = new Vector2(0, elementZoneAge * 0.5f);
        particle.position = stuckPosition + flicker + upwardDrift;

        if (elementZoneAge >= elementZoneLifespan)
            shrinking = true;

        if (shrinking)
        {
            particle.startSize = Mathf.Max(0f, particle.startSize - Time.deltaTime * 0.05f);
            if (particle.startSize <= 0.05f)
                ResetParticleToOriginal();
        }
    }

    private void ResetParticleToOriginal()
    {
        particle.startColor = originalColor;
        particle.startSize = 0.1f;
        Position = new Vector2(Random.Range(0, 100), Random.Range(0, 100));
        isStuck = false;
    }

    private Vector2 GetUniqueElementZonePosition(FlowFieldTest field)
    {
        int attempts = 0;
        while (attempts < 1000)
        {
            int x = Random.Range(0, field.cols);
            int y = Random.Range(0, field.rows);

            if (field.IsInFireMask(x, y))
            {
                Vector2 pos = new Vector2(x * field.scale, y * field.scale);
                if (!takenElementZonePositions.Contains(pos))
                {
                    takenElementZonePositions.Add(pos);
                    return pos;
                }
            }

            attempts++;
        }

        return Position;
    }
}