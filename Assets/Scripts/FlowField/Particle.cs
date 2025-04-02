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
    
    public Particle(FlowFieldManager flowFieldManager ,Vector2 start, float maxSpeed)
    {
        this.maxSpeed = maxSpeed;
        Position = start;
        velocity = Vector2.zero;
        acceleration = Vector2.zero;
        this.flowFieldManager = flowFieldManager;
        colour = flowFieldManager.colourOptions[Random.Range(0, flowFieldManager.colourOptions.Length)];
        particle = new ParticleSystem.Particle { startSize = 0.01f, startColor = colour, position = Position };
    }

    public void Update()
    {
        velocity += acceleration;
        velocity = Vector2.ClampMagnitude(velocity, maxSpeed);
        Position += velocity;
        acceleration *= 0;
        particle.position = new Vector3(Position.x, Position.y, flowFieldManager.transform.position.z);
    }

    public void ApplyForce(Vector2 force)
    {
        acceleration += force;
    }

    public void Edges(float width, float height)
    {
        var x = flowFieldManager.transform.position.x;
        var y = flowFieldManager.transform.position.y;
        
        if (Position.x > width + x) Position = new Vector2(Random.Range(0, width), Random.Range(0, height)) + new Vector2(x, y);
        if (Position.x < 0 + x) Position =new Vector2(Random.Range(0, width), Random.Range(0, height)) + new Vector2(x, y);
        if (Position.y > height + y) Position = new Vector2(Random.Range(0, width), Random.Range(0, height)) + new Vector2(x, y);
        if (Position.y < 0 + y) Position = new Vector2(Random.Range(0, width), Random.Range(0, height)) + new Vector2(x, y);
    }

    public void Follow(FlowFieldTest field)
    {
        Vector2 force = field.GetForce(Position);
        ApplyForce(force);
    }

    public ParticleSystem.Particle GetParticle() => particle;
}
