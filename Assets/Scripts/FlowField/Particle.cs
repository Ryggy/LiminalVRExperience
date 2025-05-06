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
    private FlowFieldManager flowFieldManager;

    //respawn Logic
    // Movement and other variables
    private Vector2 lastPosition;
    private float noMovementTimer = 0f;
    private const float maxNoMovementTime = 2f; // seconds before respawn
    private const float minimumMovementDistance = 0.01f; // how far must it move over time

    public Particle(FlowFieldManager flowFieldManager, Vector2 start, float maxSpeed)
    {
        this.maxSpeed = maxSpeed;
        Position = start;
        velocity = Vector2.zero;
        acceleration = Vector2.zero;
        colour = flowFieldManager.colourOptions[Random.Range(0, flowFieldManager.colourOptions.Length)];
        this.flowFieldManager = flowFieldManager;

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

        // if (isStuck)
        //     UpdateElementZoneEffect();
        // else
        //     UpdateMovement();
        
        
        
        
        if (isStuck)
        {
            UpdateElementZoneEffect();
        }
        else
        {
            UpdateMovement();
            ApplyNormalPulseEffect(field);
        }
    }

    public void ApplyForce(Vector2 force) => acceleration += force;

    public void Edges(float width, float height)
    {
        if (Position.x > width + flowFieldManager.transform.position.x ||
            Position.x < flowFieldManager.transform.position.x ||
            Position.y > height + flowFieldManager.transform.position.y ||
            Position.y < flowFieldManager.transform.position.y)
            Position = new Vector2(Random.Range(0, width), Random.Range(0, height));
    }

    public void Follow(FlowFieldTest field)
    {
        Vector2 force = field.GetForce(Position);
        ApplyForce(force);
    }

    private void UpdateMovement()
    {
        velocity += acceleration;
        velocity = Vector2.ClampMagnitude(velocity, maxSpeed);

        Position += velocity;
        acceleration *= 0;

        // Movement detection based on position delta
        float movedDistanceSq = (Position - lastPosition).sqrMagnitude;
        if (movedDistanceSq < minimumMovementDistance * minimumMovementDistance)
        {
            noMovementTimer += Time.deltaTime;
            if (noMovementTimer >= maxNoMovementTime)
            {
                Respawn();
                return;
            }
        }
        else
        {
            noMovementTimer = 0f;
            lastPosition = Position;
        }

        particle.position = new Vector3(Position.x, Position.y, flowFieldManager.transform.position.z);
    }

    private void Respawn()
    {
        isStuck = false;
        shrinking = false;
        particle.startSize = 0.1f;
        particle.startColor = originalColor;

        // Respawn somewhere random inside the flow field area
        float width = flowFieldManager.fieldWidth;
        float height = flowFieldManager.fieldHeight;
        Position = new Vector2(
            Random.Range(0, width) + flowFieldManager.transform.position.x,
            Random.Range(0, height) + flowFieldManager.transform.position.y
        );

        velocity = Vector2.zero;
        acceleration = Vector2.zero;
    }
    
    
    
    // pusling effect
    
    private void ApplyNormalPulseEffect(FlowFieldTest field)
    {
        if (field.outlineType != FlowFieldTest.OutlineType.Normal || !flowFieldManager.enableNormalSizePulsing)
            return;

        float pulseSpeed = flowFieldManager.normalPulseSpeed;
        float pulseAmount = flowFieldManager.normalPulseAmount;

        float sizeOscillation = Mathf.Sin(Time.time * pulseSpeed + randomSeed) * pulseAmount;
        particle.startSize = 0.1f + sizeOscillation;
    }

    public ParticleSystem.Particle GetParticle() => particle;


    #region Element Zone Logic

    private bool isStuck = false;
    private bool wasInElementZoneLastFrame = false;
    private Color originalColor;
    private Vector2 stuckPosition;
    private float randomSeed;
    private float elementZoneAge = 0f;
    private const float elementZoneLifespan = 100f; // lower value, can reduce clumping
    private bool shrinking = false;
    private static HashSet<Vector2> takenElementZonePositions = new HashSet<Vector2>();

    private bool CheckElementZone(FlowFieldTest field)
    {
        if (field == null) return false;

        int x = Mathf.FloorToInt(Position.x / field.scale);
        int y = Mathf.FloorToInt(Position.y / field.scale);

        // Check for Fire element
        if (field.outlineType == FlowFieldTest.OutlineType.Fire && field.IsInFireMask(x, y))
        {
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

        // Check for Water element
        if (field.outlineType == FlowFieldTest.OutlineType.Water && field.IsInWaterMask(x, y))
        {
            if (!isStuck)
            {
                stuckPosition = GetUniqueElementZonePosition(field);
                Position = stuckPosition;
                isStuck = true;
                elementZoneAge = 0f;
                shrinking = false;
            }

            particle.startColor = field.GetWaveColor(); // Use water's dynamic color
            return true;
        }

        return false;
    }

    private void ExitElementZone()
    {
        isStuck = false;
        particle.startColor = originalColor;
        particle.startSize = 0.1f;
    }

    private void UpdateElementZoneEffect()
    {
        elementZoneAge += Time.deltaTime;

        // Fire Element effects
        {
            // Fire flicker and upward drift
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

        // Water Element effects
        {
            // Water movement could be a simple drifting effect
            Vector2 drift = new Vector2(
                Mathf.Sin(Time.time * 0.5f + randomSeed) * 0.1f,
                Mathf.Cos(Time.time * 0.5f + randomSeed) * 0.1f
            );

            Vector2 upwardDrift = new Vector2(0, elementZoneAge * 0.2f);
            particle.position = stuckPosition + drift + upwardDrift;

            // Gradually fade out if desired
            if (elementZoneAge >= elementZoneLifespan)
                shrinking = true;

            if (shrinking)
            {
                particle.startSize = Mathf.Max(0f, particle.startSize - Time.deltaTime * 0.05f);
                if (particle.startSize <= 0.05f)
                    ResetParticleToOriginal();
            }
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

    #endregion // End of Element Zone Logic
}