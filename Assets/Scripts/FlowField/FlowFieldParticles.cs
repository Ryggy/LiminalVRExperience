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

            particles[i] = Instantiate(particlePrefab, spawnPos, Quaternion.identity);
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
}