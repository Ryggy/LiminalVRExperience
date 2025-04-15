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

    // test Varaibles
    private bool isStuck = false;
    private bool wasInFireLastFrame = false;
    private Color originalColor;
    private Vector2 stuckPosition;
    
    public Particle(FlowFieldManager flowFieldManager ,Vector2 start, float maxSpeed)
    {
        this.maxSpeed = maxSpeed;
        Position = start;
        velocity = Vector2.zero;
        acceleration = Vector2.zero;
        colour = flowFieldManager.colourOptions[Random.Range(0, flowFieldManager.colourOptions.Length)];
        originalColor = colour; // Save  color
        particle = new ParticleSystem.Particle { startSize = 0.1f, startColor = colour, position = Position };
    }

    
    // original update
    // public void Update()
    // {
    //     velocity += acceleration;
    //     velocity = Vector2.ClampMagnitude(velocity, maxSpeed);
    //     Position += velocity;
    //     acceleration *= 0;
    //     particle.position = Position;
    // }

    
    // testing stuff below
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
                }

            
                Color fireColor = field.GetFlickeringColor();
                particle.startColor = fireColor;
            }
        }

  
        if (!isInFireNow && wasInFireLastFrame)
        {
            isStuck = false;
            particle.startColor = originalColor;
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
   
            particle.position = stuckPosition;
        }
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

        return Position; 
    }
    // testing above
    

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
