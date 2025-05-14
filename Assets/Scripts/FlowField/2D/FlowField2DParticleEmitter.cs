using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(ParticleSystem))]
public class FlowField2DParticleEmitter : MonoBehaviour
{
    public enum EmissionType2D
    {
        VectorOrigins,
        InsideField,
        Edges
    }

    public FlowField2DVolume flowFieldSource;
    public EmissionType2D emissionType = EmissionType2D.VectorOrigins;
    [Range(0f, 1f)] public float coverage = 1.0f;

    private ParticleSystem ps;
    private ParticleSystem.EmitParams emitParams;
    private float emissionRate = 0;
    private float timer = 0;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        ps.Stop();
        emitParams = new ParticleSystem.EmitParams();

        var main = ps.main;
        emitParams.startColor = main.startColor.Evaluate(0.0f);
        emitParams.startSize = main.startSize.Evaluate(0.0f);
        emitParams.startLifetime = main.startLifetime.Evaluate(0.0f);

        emissionRate = 1f / main.maxParticles; // fallback
        if (ps.emission.rateOverTime.constant > 0)
            emissionRate = 1f / ps.emission.rateOverTime.constant;
    }

    private void FixedUpdate()
    {
        if (flowFieldSource == null || flowFieldSource.vectorData == null)
            return;

        if (timer > emissionRate)
        {
            int numToEmit = (int)(timer / emissionRate);
            float invCoverage = 1.0f - coverage;

            for (int i = 0; i < numToEmit; i++)
            {
                switch (emissionType)
                {
                    case EmissionType2D.VectorOrigins:
                        EmitFromRandomVector();
                        break;

                    case EmissionType2D.InsideField:
                        EmitFromFieldInterior(invCoverage);
                        break;

                    case EmissionType2D.Edges:
                        EmitFromFieldEdge(invCoverage);
                        break;
                }

                ApplySimulationSpace();

                ps.Emit(emitParams, 1);
            }

            timer = 0;
        }

        timer += Time.deltaTime;
    }

    void EmitFromRandomVector()
    {
        int x = Random.Range(0, flowFieldSource.cols);
        int y = Random.Range(0, flowFieldSource.rows);

        int index = x + y * flowFieldSource.cols;
        Vector2 pos = new Vector2(x * flowFieldSource.cellSize, y * flowFieldSource.cellSize) + (Vector2)flowFieldSource.transform.position;

        emitParams.position = new Vector3(pos.x, pos.y, 0);
        emitParams.velocity = flowFieldSource.vectorData[index];
    }

    void EmitFromFieldInterior(float invCoverage)
    {
        float paddingX = flowFieldSource.cols * invCoverage * 0.5f;
        float paddingY = flowFieldSource.rows * invCoverage * 0.5f;

        int x = Random.Range((int)paddingX, flowFieldSource.cols - (int)paddingX);
        int y = Random.Range((int)paddingY, flowFieldSource.rows - (int)paddingY);
        int index = x + y * flowFieldSource.cols;

        Vector2 pos = new Vector2(x * flowFieldSource.cellSize, y * flowFieldSource.cellSize) + (Vector2)flowFieldSource.transform.position;

        emitParams.position = new Vector3(pos.x, pos.y, 0);
        emitParams.velocity = flowFieldSource.vectorData[index];
    }

    void EmitFromFieldEdge(float invCoverage)
    {
        int edge = Random.Range(0, 4); // 0=top, 1=bottom, 2=left, 3=right
        int x = 0, y = 0;

        switch (edge)
        {
            case 0: // Top
                x = Random.Range(0, flowFieldSource.cols);
                y = flowFieldSource.rows - 1;
                break;
            case 1: // Bottom
                x = Random.Range(0, flowFieldSource.cols);
                y = 0;
                break;
            case 2: // Left
                x = 0;
                y = Random.Range(0, flowFieldSource.rows);
                break;
            case 3: // Right
                x = flowFieldSource.cols - 1;
                y = Random.Range(0, flowFieldSource.rows);
                break;
        }

        int index = x + y * flowFieldSource.cols;
        Vector2 pos = new Vector2(x * flowFieldSource.cellSize, y * flowFieldSource.cellSize) + (Vector2)flowFieldSource.transform.position;

        emitParams.position = new Vector3(pos.x, pos.y, 0);
        emitParams.velocity = flowFieldSource.vectorData[index];
    }

    void ApplySimulationSpace()
    {
        var sim = ps.main.simulationSpace;
        if (sim == ParticleSystemSimulationSpace.Custom && ps.main.customSimulationSpace != null)
        {
            Matrix4x4 matrix = ps.main.customSimulationSpace.worldToLocalMatrix;
            emitParams.position = matrix.MultiplyPoint(emitParams.position);
            emitParams.velocity = matrix.MultiplyVector(emitParams.velocity);
        }
        else if (sim == ParticleSystemSimulationSpace.Local)
        {
            Matrix4x4 matrix = transform.worldToLocalMatrix;
            emitParams.position = matrix.MultiplyPoint(emitParams.position);
            emitParams.velocity = matrix.MultiplyVector(emitParams.velocity);
        }
    }
}
