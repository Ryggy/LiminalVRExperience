using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;


[ExecuteInEditMode]
[AddComponentMenu("Vector Field/2D Flow Field Volume")]
public class FlowField2DVolume : MonoBehaviour
{
   // IO
    public bool loadFromFile = true;
    [HideInInspector]
    [FormerlySerializedAs("fgaPath")]
    public string fgaFileName;
    [FormerlySerializedAs("globalScale")] public float importVectorScale = 0.01f;
    
    // Grid
    public int cols = 60;
    public int rows = 40;
    [FormerlySerializedAs("scale")] public float cellSize = 10f;
    public float increment = 0.1f;
    
    [SerializeField] public Vector2[] vectorData;
    private Vector2[] originalVectorData;
    private Vector2[] workingVectorData;
    
    // Decay
    [Range(0f, 5f)] public float decayRate = 0.1f;
    
    // Bounds
    public enum BoundsType2D
    {
        Closed,
        Border,
        Repeat,
        Mirror
    }
    
    public bool perAxisBounds = false;
    public BoundsType2D bounds = BoundsType2D.Closed;
    public BoundsType2D boundsX = BoundsType2D.Closed;
    public BoundsType2D boundsY = BoundsType2D.Closed;
    [Range(0f, 1f)] public float closedBoundRamp = 0f;
   
    // Animate
    public bool animateField = false;
    public AnimationCurve animationCurve = new AnimationCurve();
    public float animationSpeed = 1.0f;
    
    // Gizmos
    public bool showGizmos = true;
    public Color vectorColor = Color.yellow;
    public float gizmoScale = 1f;
    
    //Behaviour
    public enum FieldType { Normal, Fire }
    public FieldType fieldType = FieldType.Normal;

    private float zOffset = 0f;
    private Dictionary<FieldType, IFlowField2DBehaviour> behaviours;
    private IFlowField2DBehaviour activeBehaviour;

    void Awake()
    {
        vectorData = new Vector2[cols * rows];
    }
    
    void Start()
    {
        if (loadFromFile && !string.IsNullOrEmpty(fgaFileName))
        {
            //Debug.Log("Trying to load FGA file: " + fgaFileName);
            StartCoroutine(LoadFGA(fgaFileName));
        }
    }

    void Update()
    {
        if (animateField)
        {
            zOffset += Time.deltaTime * animationSpeed;
            GenerateField();
        }
        
        if (workingVectorData != null && originalVectorData != null)
        {
            for (int i = 0; i < workingVectorData.Length; i++)
            {
                workingVectorData[i] = Vector2.Lerp(workingVectorData[i], originalVectorData[i], Time.deltaTime * decayRate);
            }
        }
    }

    void InitBehaviour()
    {
        behaviours = new Dictionary<FieldType, IFlowField2DBehaviour>
        {
            { FieldType.Normal, new Normal2DField() },
            { FieldType.Fire, new Fire2DField(this) }
        };
        activeBehaviour = behaviours[fieldType];
        vectorData = new Vector2[cols * rows];
        originalVectorData = new Vector2[cols * rows];
        workingVectorData = new Vector2[cols * rows];
    }

    public void GenerateField()
    {
        if (activeBehaviour == null) InitBehaviour();
        activeBehaviour.GenerateField(vectorData, cols, rows, cellSize, increment, zOffset);
        
        for (int i = 0; i < vectorData.Length; i++)
        {
            originalVectorData[i] = vectorData[i];
            workingVectorData[i] = vectorData[i];
        }
    }

    public Vector2 GetVectorAtWorldPosition(Vector2 worldPos)
    {
        Vector2 local = worldPos - (Vector2)transform.position;
        Vector2 relative = new Vector2(local.x / (cols * cellSize), local.y / (rows * cellSize));

        float borderIntensity = 1f;

        BoundsType2D bx = perAxisBounds ? boundsX : bounds;
        BoundsType2D by = perAxisBounds ? boundsY : bounds;

        // Handle X bounds
        relative.x = ApplyBounds(relative.x, bx, ref borderIntensity);
        // Handle Y bounds
        relative.y = ApplyBounds(relative.y, by, ref borderIntensity);

        // Grid lookup
        int x = Mathf.Clamp(Mathf.FloorToInt(relative.x * cols), 0, cols - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(relative.y * rows), 0, rows - 1);

        int index = x + y * cols;
        return workingVectorData[index] * borderIntensity * borderIntensity;
    }
  
    public void SetForce(Vector2 gridPos, Vector2 direction)
    {
        int x = Mathf.Clamp((int)gridPos.x, 0, cols - 1);
        int y = Mathf.Clamp((int)gridPos.y, 0, rows - 1);
        int index = x + y * cols;
        workingVectorData[index] = Vector2.Lerp(workingVectorData[index], direction.normalized, 0.5f);
    }
    
    private float ApplyBounds(float rel, BoundsType2D mode, ref float borderIntensity)
    {
        switch (mode)
        {
            case BoundsType2D.Closed:
                if (rel < 0f || rel > 1f)
                    return 0f;
                if (closedBoundRamp > 0f)
                {
                    float t = Mathf.Min(rel, 1f - rel) / closedBoundRamp;
                    borderIntensity *= Mathf.Clamp01(t);
                }
                return rel;

            case BoundsType2D.Border:
                return Mathf.Clamp01(rel);

            case BoundsType2D.Repeat:
                return rel - Mathf.Floor(rel);

            case BoundsType2D.Mirror:
                rel = (rel * 0.5f - Mathf.Floor(rel * 0.5f)) * 2.0f;
                if (rel > 1.0f)
                    rel = 2f - rel;
                return rel;

            default:
                return rel;
        }
    }
    
    public void ReadFGA(string filename)
    {
        StartCoroutine(LoadFGA(filename));
    }

   private IEnumerator LoadFGA(string filename)
    {
        string path = Path.Combine(Application.streamingAssetsPath, filename);

        UnityWebRequest www = UnityWebRequest.Get(path);
        yield return www.SendWebRequest();

    #if UNITY_2020_1_OR_NEWER
        if (www.result != UnityWebRequest.Result.Success)
    #else
        if (www.isNetworkError || www.isHttpError)
    #endif
        {
            Debug.LogError("Failed to load FGA file: " + path + "\n" + www.error);
            yield break;
        }

        string[] lines = www.downloadHandler.text.Split('\n');
        if (lines.Length < 4)
        {
            Debug.LogError("FGA file is malformed or empty: " + filename);
            yield break;
        }

        var size = lines[0].Split(',');
        int xSize = (int)float.Parse(size[0].Trim(), System.Globalization.CultureInfo.InvariantCulture);
        int ySize = (int)float.Parse(size[1].Trim(), System.Globalization.CultureInfo.InvariantCulture);
        int zSize = (int)float.Parse(size[2].Trim(), System.Globalization.CultureInfo.InvariantCulture);

        if (zSize != 1)
            Debug.LogWarning("FGA has Z > 1, only Z=1 is used for 2D slice");

        cols = xSize;
        rows = ySize;
        vectorData = new Vector2[cols * rows];
        originalVectorData = new Vector2[cols * rows];
        workingVectorData = new Vector2[cols * rows];

        int lineIndex = 3; // skip size, min, max
        for (int z = 0; z < zSize; z++)
        {
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    if (lineIndex >= lines.Length) break;

                    var parts = lines[lineIndex++].Split(',');
                    Vector3 rawVec = new Vector3(
                        float.Parse(parts[0]) * importVectorScale,
                        float.Parse(parts[1]) * importVectorScale,
                        float.Parse(parts[2]) * importVectorScale
                    );

                    int index = x + y * cols;
                    vectorData[index] = new Vector2(rawVec.x, rawVec.y);
                    originalVectorData[index] = vectorData[index];
                    workingVectorData[index] = vectorData[index];
                }
            }
        }

        Debug.Log("FGA successfully loaded via UnityWebRequest: " + filename + $" ({cols}x{rows}) Sample[0]: {vectorData[0]}");
    }
    
    void OnDrawGizmos()
    {
        if (!showGizmos || workingVectorData == null) return;

        Gizmos.color = vectorColor;

        for (int y = 0; y < rows; y++)
        for (int x = 0; x < cols; x++)
        {
            int index = x + y * cols;
            Vector2 dir = workingVectorData[index];
            if (dir.sqrMagnitude < 0.001f) continue;

            Vector3 pos = new Vector3(x * cellSize, y * cellSize, 0) + transform.position;
            Gizmos.DrawRay(pos, (Vector3)dir.normalized * cellSize * gizmoScale);
        }
    }
    
    public int GetFieldConfigHash()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + cols.GetHashCode();
            hash = hash * 31 + rows.GetHashCode();
            hash = hash * 31 + cellSize.GetHashCode();
            hash = hash * 31 + increment.GetHashCode();
            hash = hash * 31 + fieldType.GetHashCode();
            return hash;
        }
    }
}

public interface IFlowField2DBehaviour
{
    void GenerateField(Vector2[] vectorData, int cols, int rows, float scale, float increment, float zOffset);
}

public class Normal2DField : IFlowField2DBehaviour
{
    public void GenerateField(Vector2[] vectorData, int cols, int rows, float scale, float increment, float zOffset)
    {
        float yOffset = 0;
        for (int y = 0; y < rows; y++)
        {
            float xOffset = 0;
            for (int x = 0; x < cols; x++)
            {
                float angle = Mathf.PerlinNoise(xOffset, yOffset + zOffset) * Mathf.PI * 2;
                Vector2 baseDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 upwardBias = new Vector2(0, 1);
                int index = x + y * cols;
                vectorData[index] = (baseDirection + upwardBias * 0.5f).normalized;
                xOffset += increment;
            }
            yOffset += increment;
        }
    }
}

public class Fire2DField : IFlowField2DBehaviour
{
    private FlowField2DVolume owner;

    public Fire2DField(FlowField2DVolume owner) { this.owner = owner; }

    public void GenerateField(Vector2[] vectorData, int cols, int rows, float scale, float increment, float zOffset)
    {
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                float jitter = (Mathf.PerlinNoise(x * 0.1f, Time.time * 0.5f) - 0.5f) * 0.3f;
                
                int index = x + y * cols;
                vectorData[index] = new Vector2(jitter, 1f).normalized;
            }
        }
    }
}
