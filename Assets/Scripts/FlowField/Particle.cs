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
    
    public Particle(FlowFieldManager flowFieldManager ,Vector2 start, float maxSpeed)
    {
        this.maxSpeed = maxSpeed;
        Position = start;
        velocity = Vector2.zero;
        acceleration = Vector2.zero;
        colour = flowFieldManager.colourOptions[Random.Range(0, flowFieldManager.colourOptions.Length)];
        particle = new ParticleSystem.Particle { startSize = 0.1f, startColor = colour, position = Position };
    }

    public void Update()
    {
        velocity += acceleration;
        velocity = Vector2.ClampMagnitude(velocity, maxSpeed);
        Position += velocity;
        acceleration *= 0;
        particle.position = Position;
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
    public Vector2 GetPosition()
    {
        return Position;
    }

    public ParticleSystem.Particle GetParticle() => particle;
}
