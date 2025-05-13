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
    }

    private IEnumerator CycleOutlineTypes()
    {
        FlowFieldTest.OutlineType[] types =
        {
            FlowFieldTest.OutlineType.Normal,
            FlowFieldTest.OutlineType.Fire,
            FlowFieldTest.OutlineType.Water,
            FlowFieldTest.OutlineType.Earth,
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
        // Update particle positions and behavior
        for (int i = 0; i < particles.Count; i++)
        {
            particles[i].Follow(flowField);
            particles[i].Update(flowField);
            particles[i].Edges(fieldWidth, fieldHeight);
            particleArray[i] = particles[i].GetParticle();
        }

        particleSystem.SetParticles(particleArray, particleArray.Length);

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
    }


    // pusling effect stuff
    [Header("Normal Mode Particle Animation")]
    public bool enableNormalSizePulsing = true;

    [Range(0f, 1f)] public float normalPulseAmount = 0.1f;
    [Range(0.1f, 5f)] public float normalPulseSpeed = 0f;
    public float changeAmount = 5f;

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
}