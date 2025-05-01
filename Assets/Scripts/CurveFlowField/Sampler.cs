using System.Collections.Generic;
using UnityEngine;

public static class Sampler
{
    public static List<Vector2> RandomSample(int count, float width, float height)
    {
        var samples = new List<Vector2>();
        for (int i = 0; i < count; i++)
        {
            samples.Add(new Vector2(Random.Range(0, width), Random.Range(0, height)));
        }
        return samples;
    }

    public static List<Vector2> GridSample(int count, float width, float height)
    {
        var samples = new List<Vector2>();
        int gridSize = Mathf.CeilToInt(Mathf.Sqrt(count));
        float spacingX = width / gridSize;
        float spacingY = height / gridSize;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                samples.Add(new Vector2((x + 0.5f) * spacingX, (y + 0.5f) * spacingY));
                if (samples.Count >= count)
                    return samples;
            }
        }

        return samples;
    }
    
    public static List<Vector2> PoissonSample(float width, float height, float minDist, int k = 30)
    {
        List<Vector2> samples = new List<Vector2>();
        List<Vector2> spawnPoints = new List<Vector2>();

        float cellSize = minDist / Mathf.Sqrt(2);
        int gridWidth = Mathf.CeilToInt(width / cellSize);
        int gridHeight = Mathf.CeilToInt(height / cellSize);
        Vector2[,] grid = new Vector2[gridWidth, gridHeight];

        // Start with a random point
        Vector2 firstPoint = new Vector2(Random.value * width, Random.value * height);
        samples.Add(firstPoint);
        spawnPoints.Add(firstPoint);
        grid[(int)(firstPoint.x / cellSize), (int)(firstPoint.y / cellSize)] = firstPoint;

        while (spawnPoints.Count > 0)
        {
            int spawnIndex = Random.Range(0, spawnPoints.Count);
            Vector2 spawnCenter = spawnPoints[spawnIndex];
            bool accepted = false;

            for (int i = 0; i < k; i++)
            {
                float angle = Random.value * Mathf.PI * 2;
                float radius = Random.Range(minDist, 2 * minDist);
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 candidate = spawnCenter + dir * radius;

                if (candidate.x >= 0 && candidate.x < width && candidate.y >= 0 && candidate.y < height)
                {
                    int cellX = (int)(candidate.x / cellSize);
                    int cellY = (int)(candidate.y / cellSize);
                    bool valid = true;

                    for (int x = Mathf.Max(0, cellX - 2); x < Mathf.Min(cellX + 3, gridWidth); x++)
                    {
                        for (int y = Mathf.Max(0, cellY - 2); y < Mathf.Min(cellY + 3, gridHeight); y++)
                        {
                            if (grid[x, y] != Vector2.zero && (grid[x, y] - candidate).sqrMagnitude < minDist * minDist)
                            {
                                valid = false;
                                break;
                            }
                        }
                        if (!valid) break;
                    }

                    if (valid)
                    {
                        samples.Add(candidate);
                        spawnPoints.Add(candidate);
                        grid[cellX, cellY] = candidate;
                        accepted = true;
                        break;
                    }
                }
            }

            if (!accepted)
                spawnPoints.RemoveAt(spawnIndex);
        }

        return samples;
    }
}
