using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowFieldManager : MonoBehaviour
{
    public FlowFieldTest flowField;
    public int numParticles = 300;
    public float fieldWidth = 600;
    public float fieldHeight = 400;
    public ParticleSystem particleSystem;
    private List<Particle> particles;
    private ParticleSystem.Particle[] particleArray;

    [SerializeField] public Color[] colourOptions = new Color[]
    {
        Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.magenta
    };
    
    void Start()
    {
        particles = new List<Particle>();
        particleArray = new ParticleSystem.Particle[numParticles];

        for (int i = 0; i < numParticles; i++)
        {
            Vector3 startPos = new Vector3(Random.Range(0, fieldWidth), Random.Range(0, fieldHeight), 0) + transform.position;
            particles.Add(new Particle(this, startPos, 0.001f));
        }
    }

    void Update()
    {
        for (int i = 0; i < particles.Count; i++)
        {
            particles[i].Follow(flowField);
            particles[i].Update(flowField); 
            particles[i].Edges(fieldWidth, fieldHeight);
            particleArray[i] = particles[i].GetParticle();
        }

        particleSystem.SetParticles(particleArray, particleArray.Length);
    }
    
}