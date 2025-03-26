using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowFieldLines : MonoBehaviour
{
    
    public FlowField flowField;
    public int particleCount = 100;

    // Array 
    public GameObject[] particlePrefabs;

    private GameObject[] particles;
    private Vector2[] velocities;

    void Start()
    {

        particles = new GameObject[particleCount];
        velocities = new Vector2[particleCount];

        // Instantiate particles 
        for (int i = 0; i < particleCount; i++)
        {
            Vector2 spawnPos = new Vector2(
                Random.Range(0, flowField.width * flowField.cellSize),
                Random.Range(0, flowField.height * flowField.cellSize)
            );

            // Randomly select a prefab 
            GameObject selectedPrefab = particlePrefabs[Random.Range(0, particlePrefabs.Length)];
            particles[i] = Instantiate(selectedPrefab, spawnPos, Quaternion.identity, transform);
            velocities[i] = Vector2.zero;
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
                position += flowDirection * Time.deltaTime;

                particle.transform.position = position;
            }
        }
    }
}