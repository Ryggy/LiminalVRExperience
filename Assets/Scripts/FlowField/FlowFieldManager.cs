using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
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
    public int spawnedParticleCount = 1000;

    public float pulseGrowthSpeed = 0.1f;
    // timer


    [SerializeField] public Color[] colourOptions = new Color[]
    {
        Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.magenta
    };

    void Start()
    {
        particles = new List<Particle>();
        particleArray = new ParticleSystem.Particle[numParticles];

        // Pass both FlowFieldManager (this) and the FlowFieldTest instance (field)
        for (int i = 0; i < numParticles; i++)
        {
            Vector2 startPos = new Vector2(Random.Range(0, fieldWidth), Random.Range(0, fieldHeight)) +
                               (Vector2)transform.position;
            particles.Add(new Particle(this, flowField, startPos, 0.005f)); // Pass FlowFieldManager and FlowFieldTest
        }

        Debug.Log("Starting element cycle coroutine.");
        StartCoroutine(CycleOutlineTypes());
        
        // Start randomizing pulse amount
        StartCoroutine(RandomizePulseAmount());
    }

    private IEnumerator CycleOutlineTypes()
    {
        FlowFieldTest.OutlineType[] types =
        {
            FlowFieldTest.OutlineType.Normal,
            FlowFieldTest.OutlineType.Fire,
            FlowFieldTest.OutlineType.Water,
            FlowFieldTest.OutlineType.Air,
        };

        int index = 0;

        while (true)
        {
            if (normalPulseSpeed >= changeAmount)
            {
                // Move to the next type first, before applying
                index = (index + 1) % types.Length;

                flowField.outlineType = types[index];
                flowField.RegenerateField();

                Debug.Log($"Switched to: {types[index]}");

                normalPulseSpeed = 0f;
                
                // Spawn more particles
                SpawnExtraParticles(spawnedParticleCount); // ← Add 100 new particles (adjust as needed)
                
                

                while (normalPulseSpeed < changeAmount)
                {
                    yield return null;
                }
            }

            yield return null;
        }
    }
    
    
    


    void Update()
    {
        int count = particles.Count;

        if (particleArray.Length < count)
            particleArray = new ParticleSystem.Particle[count];

        for (int i = 0; i < count; i++)
        {
            particles[i].Follow(flowField);
            particles[i].Update(flowField);
            particles[i].Edges(fieldWidth, fieldHeight);
            particleArray[i] = particles[i].GetParticle();
        }

        particleSystem.SetParticles(particleArray, count);
        
        // Simulate pulsing effect for Normal mode
        if (enableNormalSizePulsing)
        {
            // Gradually increase pulse amount over time
            normalPulseSpeed += pulseGrowthSpeed * Time.deltaTime;

            // Cap the value at 10 (so it doesn't go higher than 10)
            if (normalPulseSpeed > changeAmount)
            {
                normalPulseSpeed = changeAmount;
            }
        }
        
        
        
        
        
        
        
        normalPulseAmount = Mathf.MoveTowards(normalPulseAmount, targetPulseAmount, pulseLerpSpeed * Time.deltaTime);
    }
    private IEnumerator RandomizePulseAmount()
    {
        while (true)
        {
            targetPulseAmount = Random.Range(0.05f, 0.25f); // smaller range for smoother pulsing
            yield return new WaitForSeconds(Random.Range(1f, 4f));
        }
    }

    // pusling effect stuff
    [Header("Normal Mode Particle Animation")]
    public bool enableNormalSizePulsing = true;

    [Range(0f, 1f)] public float normalPulseAmount = 0.1f;
    [Range(0.1f, 5f)] public float normalPulseSpeed = 0f;
    public float changeAmount = 2f;
    private float targetPulseAmount = 0.1f;
    public float pulseLerpSpeed = 0.2f; // Adjust this to make transitions faster or slower

    private void OnDrawGizmosSelected()
    {
        if (!enableNormalSizePulsing) return;

        if (flowField != null && flowField.outlineType == FlowFieldTest.OutlineType.Normal)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.3f); // Cyan-ish transparent box
            Vector3 center = transform.position + new Vector3(fieldWidth, fieldHeight, 0) * 0.5f;
            Vector3 size = new Vector3(fieldWidth, fieldHeight, 0.1f);

            Gizmos.DrawCube(center, size);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(center, size);
        }
    }
    
    private void SpawnExtraParticles(int amount)
    {
        int newTotal = particles.Count + amount;
        if (newTotal > particleArray.Length)
        {
            // Expand particleArray if necessary
            particleArray = new ParticleSystem.Particle[newTotal];
        }

        for (int i = 0; i < amount; i++)
        {
            Vector2 startPos = new Vector2(Random.Range(0, fieldWidth), Random.Range(0, fieldHeight)) +
                               (Vector2)transform.position;
            particles.Add(new Particle(this, flowField, startPos, 0.005f));
        }

        Debug.Log($"Spawned {amount} new particles. Total: {particles.Count}");
    }
    
    
    
    
}