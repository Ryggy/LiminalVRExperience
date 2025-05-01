using System.Collections.Generic;
using UnityEngine;

public class StreamlineGenerator : MonoBehaviour
{
    [Header("Flow Field Settings")]
    public int cols = 40;
    public int rows = 40;
    public float cellSize = 1f;
    public float noiseScale = 0.1f;
    public float zOffset = 0f;

    [Header("Streamline Settings")]
    public int numLines = 100;
    public int stepsPerLine = 100;
    public float stepLength = 0.2f;
    public FlowFieldStyle flowFieldStyle = FlowFieldStyle.PerlinSmooth;
    
    [Header("Drawing")]
    public Gradient lineColor;
    public Material lineMaterial;
    
    [Header("Realtime Animation")]
    public bool animateField = true;
    public float noiseAnimationSpeed = 0.2f;
    public float updateInterval = 0.25f;
    private float timer = 0f;
    private List<GameObject> activeLines = new List<GameObject>();
    
    [Header("Gizmo Debugging")]
    public bool showGridVectors = true;
    public float arrowLength = 0.4f;
    
    private FlowField2D flowField;

    public enum SamplingMethod
    {
        Random,
        Grid,
        Poisson
    }

    [Header("Sampling Settings")]
    public SamplingMethod samplingMethod = SamplingMethod.Random;

    
    void Start()
    {
        flowField = new FlowField2D(cols, rows, cellSize, noiseScale, transform.position);

        float fieldWidth = cols * cellSize;
        float fieldHeight = rows * cellSize;

        List<Vector2> samples;

        switch (samplingMethod)
        {
            case SamplingMethod.Grid:
                samples = Sampler.GridSample(numLines, fieldWidth, fieldHeight);
                break;
            case SamplingMethod.Poisson:
                float minDistance = Mathf.Sqrt((fieldWidth * fieldHeight) / numLines) * 0.8f;
                samples = Sampler.PoissonSample(fieldWidth, fieldHeight, minDistance);
                break;
            case SamplingMethod.Random:
            default:
                samples = Sampler.RandomSample(numLines, fieldWidth, fieldHeight);
                break;
        }


        foreach (var localSample in samples)
        {
            Vector2 worldSample = (Vector2)transform.position + localSample;
            DrawStreamline(worldSample);
        }
    }
   
    void Update()
    {
        if (!animateField)
            return;

        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            zOffset += noiseAnimationSpeed;

            flowField.UpdateField(zOffset);
            ClearLines();
            RedrawStreamlines();
        }
    }
    
    void DrawStreamline(Vector2 start)
    {
        List<Vector3> points = new List<Vector3>();
        Vector2 pos = start;

        float fieldWidth = cols * cellSize;
        float fieldHeight = rows * cellSize;
        Vector2 min = (Vector2)transform.position;
        Vector2 max = min + new Vector2(fieldWidth, fieldHeight);

        for (int i = 0; i < stepsPerLine; i++)
        {
            if (pos.x < min.x || pos.x >= max.x || pos.y < min.y || pos.y >= max.y)
                break;

            points.Add(new Vector3(pos.x, pos.y, 0));
            pos += flowField.GetFlowDirection(pos) * stepLength;
        }

        if (points.Count < 2) return;

        GameObject go = new GameObject("Streamline");
        go.transform.parent = transform;

        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = points.Count;
        lr.SetPositions(points.ToArray());
        lr.widthMultiplier = 0.05f;
        lr.material = lineMaterial;
        lr.colorGradient = lineColor;

        activeLines.Add(go);
    }


    void RedrawStreamlines()
    {
        float fieldWidth = cols * cellSize;
        float fieldHeight = rows * cellSize;

        List<Vector2> samples;

        switch (samplingMethod)
        {
            case SamplingMethod.Grid:
                samples = Sampler.GridSample(numLines, fieldWidth, fieldHeight);
                break;
            case SamplingMethod.Poisson:
                float minDistance = Mathf.Sqrt((fieldWidth * fieldHeight) / numLines) * 0.8f;
                samples = Sampler.PoissonSample(fieldWidth, fieldHeight, minDistance);
                break;
            default:
                samples = Sampler.RandomSample(numLines, fieldWidth, fieldHeight);
                break;
        }

        foreach (var sample in samples)
        {
            Vector2 worldPos = (Vector2)transform.position + sample;
            DrawStreamline(worldPos);
        }
    }
    
    void ClearLines()
    {
        foreach (var line in activeLines)
        {
            if (line != null) Destroy(line);
        }
        activeLines.Clear();
    }

    void OnDrawGizmos()
    {
        if (!showGridVectors || flowField == null) return;

        Vector2[,] vectors = flowField.Vectors;
        float scale = flowField.Scale;

        Gizmos.color = Color.gray;

        for (int x = 0; x < flowField.Cols; x++)
        {
            for (int y = 0; y < flowField.Rows; y++)
            {
                Vector3 start = transform.position + new Vector3(x * scale, y * scale, 0);
                Vector2 dir = vectors[x, y];
                Vector3 end = start + new Vector3(dir.x, dir.y, 0) * arrowLength;

                // Draw line
                Gizmos.DrawLine(start, end);
            }
        }
    }
}
