using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class FlowField2DParticleController : MonoBehaviour
{
    public FlowField2DVolume field;
    [Range(0f, 1f)] public float tightness = 0.5f;
    [Range(0.01f, 1f)] public float minInfluence = 0.05f;
    public float multiplier = 1f;

    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void LateUpdate()
    {
        if (field == null) return;

        int maxParticles = ps.main.maxParticles;
        if (particles == null || particles.Length != maxParticles)
            particles = new ParticleSystem.Particle[maxParticles];

        int alive = ps.GetParticles(particles);

        for (int i = 0; i < alive; i++)
        {
            Vector3 pos = particles[i].position;
            Vector2 force = field.GetVectorAtWorldPosition(new Vector2(pos.x, pos.y)) * multiplier;

            float intensity = force.magnitude;
            float blend = Mathf.SmoothStep(-0.0001f, minInfluence, intensity);

            Vector2 currentVelocity = new Vector2(particles[i].velocity.x, particles[i].velocity.y);
            Vector2 newVelocity = Vector2.Lerp(currentVelocity + force * Time.deltaTime * blend, force, tightness * blend);

            particles[i].velocity = new Vector3(newVelocity.x, newVelocity.y, 0);
        }

        ps.SetParticles(particles, alive);
    }
}