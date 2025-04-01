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

    public Particle(Vector2 start, float maxSpeed)
    {
        this.maxSpeed = maxSpeed;
        Position = start;
        velocity = Vector2.zero;
        acceleration = Vector2.zero;
        particle = new ParticleSystem.Particle { startSize = 1f, startColor = Color.white, position = Position };
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
        if (Position.x > width) Position.x = Random.Range(0, width);
        if (Position.x < 0) Position.x = Random.Range(0, width);;
        if (Position.y > height) Position.y = Random.Range(0, height);;
        if (Position.y < 0) Position.y = Random.Range(0, height);
    }

    public void Follow(FlowFieldTest field)
    {
        Vector2 force = field.GetForce(Position);
        ApplyForce(force);
    }

    public ParticleSystem.Particle GetParticle() => particle;
}
