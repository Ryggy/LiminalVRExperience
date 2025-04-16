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

    // Fire-related variables
    private bool isStuck = false;
    private bool wasInFireLastFrame = false;
    private Color originalColor;
    private Vector2 stuckPosition;
    private float randomSeed;
    private float fireAge = 0f;
    private const float fireLifespan = 3f;
    private bool shrinking = false;

    public Particle(FlowFieldManager flowFieldManager, Vector2 start, float maxSpeed)
    {
        this.maxSpeed = maxSpeed;
        Position = start;
        velocity = Vector2.zero;
        acceleration = Vector2.zero;
        colour = flowFieldManager.colourOptions[Random.Range(0, flowFieldManager.colourOptions.Length)];
        originalColor = colour;
        particle = new ParticleSystem.Particle { startSize = 0.1f, startColor = colour, position = Position };
        randomSeed = Random.Range(0f, 1000f);
    }

    public void Update(FlowFieldTest field)
    {
        bool isInFireNow = false;

        if (field != null)
        {
            int x = Mathf.FloorToInt(Position.x / field.scale);
            int y = Mathf.FloorToInt(Position.y / field.scale);

            if (field.outlineType == FlowFieldTest.OutlineType.Fire && field.IsInFireMask(x, y))
            {
                isInFireNow = true;

                if (!isStuck)
                {
                    stuckPosition = GetUniqueFirePosition(field);
                    Position = stuckPosition;
                    isStuck = true;
                    fireAge = 0f;
                    shrinking = false;
                }

                Color fireColor = field.GetFlickeringColor();
                particle.startColor = fireColor;
            }
        }

        if (!isInFireNow && wasInFireLastFrame)
        {
            isStuck = false;
            particle.startColor = originalColor;
            particle.startSize = 0.1f;
        }

        wasInFireLastFrame = isInFireNow;

        if (!isStuck)
        {
            velocity += acceleration;
            velocity = Vector2.ClampMagnitude(velocity, maxSpeed);
            Position += velocity;
            acceleration *= 0;
            particle.position = Position;
        }
        else
        {
            fireAge += Time.deltaTime;

            float flickerSpeed = 3f;
            float flickerAmount = 0.3f;
            Vector2 flicker = new Vector2(
                Mathf.Sin(Time.time * flickerSpeed + randomSeed) * flickerAmount,
                Mathf.Cos(Time.time * flickerSpeed + randomSeed) * flickerAmount
            );

            Vector2 upwardDrift = new Vector2(0, fireAge * 0.5f);
            particle.position = stuckPosition + flicker + upwardDrift;

            if (fireAge >= fireLifespan)
            {
                shrinking = true;
            }

            // if (shrinking)
            // {
            //     particle.startSize = Mathf.Max(0f, particle.startSize - Time.deltaTime * 0.05f);
            // }
            
            if (shrinking)
            {
                particle.startSize = Mathf.Max(0f, particle.startSize - Time.deltaTime * 0.05f);

                // Once particle gets very small, reset it to normal behavior
                if (particle.startSize <= 0.05f) // When it gets small enough
                {
                    ResetParticleToOriginal(); // Reset size and color to original values
                }
            }
        }
        
    }
    // Reset particle to original size, color, and random position
    private void ResetParticleToOriginal()
    {
        particle.startColor = originalColor;
        particle.startSize = 0.1f; // Original size
        Position = new Vector2(Random.Range(0, 100), Random.Range(0, 100)); // Random position
    }

    private static HashSet<Vector2> takenFirePositions = new HashSet<Vector2>();

    private Vector2 GetUniqueFirePosition(FlowFieldTest field)
    {
        int attempts = 0;

        while (attempts < 1000)
        {
            int x = Random.Range(0, field.cols);
            int y = Random.Range(0, field.rows);

            if (field.IsInFireMask(x, y))
            {
                Vector2 pos = new Vector2(x * field.scale, y * field.scale);

                if (!takenFirePositions.Contains(pos))
                {
                    takenFirePositions.Add(pos);
                    return pos;
                }
            }

            attempts++;
        }

        return Position; // fallback
    }

    public void ApplyForce(Vector2 force)
    {
        acceleration += force;
    }

    public void Edges(float width, float height)
    {
        if (Position.x > width) Position = new Vector2(Random.Range(0, width), Random.Range(0, height));
        if (Position.x < 0) Position = new Vector2(Random.Range(0, width), Random.Range(0, height));
        if (Position.y > height) Position = new Vector2(Random.Range(0, width), Random.Range(0, height));
        if (Position.y < 0) Position = new Vector2(Random.Range(0, width), Random.Range(0, height));
    }

    public void Follow(FlowFieldTest field)
    {
        Vector2 force = field.GetForce(Position);
        ApplyForce(force);
    }

    public ParticleSystem.Particle GetParticle() => particle;
}