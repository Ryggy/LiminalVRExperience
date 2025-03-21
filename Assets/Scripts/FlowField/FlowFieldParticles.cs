using UnityEngine;

public class FlowFieldParticles : MonoBehaviour
{
    public FlowField flowField;
    public int particleCount = 50;
    public GameObject particlePrefab;
    
    private GameObject[] particles;

    void Start()
    {
        particles = new GameObject[particleCount];
    
        for (int i = 0; i < particleCount; i++)
        {
            Vector2 spawnPos = new Vector2(
                Random.Range(0, flowField.width * flowField.cellSize),
                Random.Range(0, flowField.height * flowField.cellSize)
            );
    
            particles[i] = Instantiate(particlePrefab, spawnPos, Quaternion.identity, this.transform);
        }
    }
    
    void Update()
    {
        foreach (GameObject particle in particles)
        {
            Vector2 position = particle.transform.position;
            int gridX = Mathf.FloorToInt(position.x / flowField.cellSize);
            int gridY = Mathf.FloorToInt(position.y / flowField.cellSize);
    
            if (gridX >= 0 && gridX < flowField.width && gridY >= 0 && gridY < flowField.height)
            {
                Vector2 flowDirection = flowField.GetFlowDirection(gridX, gridY);
                particle.transform.position += (Vector3)flowDirection * Time.deltaTime;
            }
        }
    }
    
    // public FlowField flowField; // Reference to the FlowField class
    // public float particleSpeed = 0.5f; // Control how fast particles follow the flow
    // public ParticleSystem particleSystem; // Reference to the particle system
    // private ParticleSystem.Particle[] particles; // Array of particles
    //
    // void Start()
    // {
    //     if (particleSystem == null)
    //     {
    //         particleSystem = GetComponent<ParticleSystem>();
    //     }
    //     
    //     particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
    // }
    //
    // void Update()
    // {
    //     // Get all particles from the particle system
    //     int particleCount = particleSystem.GetParticles(particles);
    //
    //     // Loop through all particles and update their positions based on the flow field
    //     for (int i = 0; i < particleCount; i++)
    //     {
    //         ParticleSystem.Particle particle = particles[i];
    //         Vector3 particlePosition = particle.position;
    //
    //         // Convert particle position to grid coordinates
    //         int x = Mathf.FloorToInt(particlePosition.x / flowField.cellSize);
    //         int y = Mathf.FloorToInt(particlePosition.y / flowField.cellSize);
    //
    //         // Ensure the position is within bounds
    //         x = Mathf.Clamp(x, 0, flowField.width - 1);
    //         y = Mathf.Clamp(y, 0, flowField.height - 1);
    //
    //         // Get the flow direction for this position in the grid
    //         Vector2 flowDirection = flowField.GetFlowDirection(x, y);
    //
    //         // Apply the flow direction to the particle velocity
    //         particle.velocity = new Vector3(flowDirection.x, flowDirection.y, 0) * particleSpeed;
    //
    //         // Update the particle's position (you can also use velocity in the particle system for movement)
    //         particles[i] = particle;
    //     }
    //
    //     // Apply changes to the particle system
    //     particleSystem.SetParticles(particles, particleCount);
    // }
}