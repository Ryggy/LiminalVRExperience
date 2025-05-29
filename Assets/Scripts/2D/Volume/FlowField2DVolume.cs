using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

public class FlowField2DVolume : MonoBehaviour, IFlowField, IFieldLoader
{
    #region Variables
    public static List<FlowField2DVolume> All2DFields = new List<FlowField2DVolume>();
    
    private IFieldDecayHandler _decayHandler = new FieldDecayHandler();
    private IFieldBoundsHandler _boundsHandler = new FieldBoundsHandler();
    private IFieldVisualiserHandler _visualiserHandler = new FieldVisualiserHandler();

    private IFieldLoader _fileLoader;
    
    [Header("IO Settings")]
    [HideInInspector]
    public string fgaFileName;
    public bool loadFromFile = true;
    public float importVectorScale = 1f;
    
    [Header("Flow Field Settings")]
    public int cols = 60;
    public int rows = 40;
    public float cellSize = 10f;
    
    [HideInInspector] public Vector2[] vectorData;
    private Vector2[] originalVectorData;
    private Vector2[] workingVectorData;
    
    [Header("Bounds Settings")]
    public bool perAxisBounds = false;
    public BoundsType2D bounds = BoundsType2D.Closed;
    public BoundsType2D boundsX = BoundsType2D.Closed;
    public BoundsType2D boundsY = BoundsType2D.Closed;
    [Range(0f, 1f)] public float closedBoundRamp = 0f;
    
    [Header("Gizmo Settings")]
    public bool showGizmos = true;
    public Color vectorColor = Color.yellow;
    public float gizmoScale = 1f;
    #endregion
    
    #region Unity Methods
    private void OnEnable()
    {
        if (!All2DFields.Contains(this))
            All2DFields.Add(this);
    }

    private void OnDisable()
    {
        All2DFields.Remove(this);
    }
    private void Awake()
    {
        vectorData = new Vector2[cols * rows];
        originalVectorData = new Vector2[cols * rows];
        workingVectorData = new Vector2[cols * rows];

       InitLoader();
    }

    // ensures FF is loaded when values change in the Inspector outside of runtime
    private void OnValidate()
    {
        InitLoader();
    }

    private void Start()
    {
        if (loadFromFile && !string.IsNullOrEmpty(fgaFileName))
            LoadFromFile(fgaFileName);
    }
    #endregion 
    
    #region FlowField Methods
    public void GenerateField()
    {
        for (int i = 0; i < vectorData.Length; i++)
        {
            originalVectorData[i] = vectorData[i];
            workingVectorData[i] = vectorData[i];
        }
    }
    
    public void SetForce(Vector2 gridPos, Vector2 direction)
    {
        int x = Mathf.Clamp((int)gridPos.x, 0, cols - 1);
        int y = Mathf.Clamp((int)gridPos.y, 0, rows - 1);
        int index = x + y * cols;
        workingVectorData[index] = Vector2.Lerp(workingVectorData[index], direction.normalized, 0.5f);
    }
    
    public Vector2 GetVectorAtWorldPosition(Vector2 worldPos)
    {
        Vector2 gridIndex = _boundsHandler.ClampToBounds(
            worldPos, transform.position, cols, rows, cellSize,
            boundsX, boundsY, closedBoundRamp, perAxisBounds
        );

        int index = (int)(gridIndex.x + gridIndex.y * cols);
        return workingVectorData[index];
    }

    public static Vector2 GetCombinedVectors(Vector3 worldPosition)
    {
        Vector2 combined = Vector2.zero;
        foreach (var field in All2DFields)
        {
            if (field != null)
                combined += field.GetVectorAtWorldPosition(worldPosition);
        }
        return combined;
    }

    public static Vector2 GetCombinedVectorsRestricted(Vector3 worldPosition, List<FlowField2DVolume> ffList)
    {
        Vector2 combined = Vector2.zero;
        foreach (var field in ffList)
        {
            if (field != null)
                combined += field.GetVectorAtWorldPosition(worldPosition);
        }
        return combined;
    }
  
    public void ApplyDecay(float rate, float deltaTime)
    {
        _decayHandler.ApplyDecay(workingVectorData, originalVectorData, rate, deltaTime);
    }
    #endregion
    
    #region Loading Methods
    private void InitLoader()
    {
        if (_fileLoader == null)
            _fileLoader = new FGAFileLoader(this, this);
    }
    
    public void LoadFromFile(string filename)
    {
        _fileLoader.LoadFromFile(filename);
    }
    
    public void SetInternalBuffers(Vector2[] original, Vector2[] working)
    {
        originalVectorData = original;
        workingVectorData = working;
    }
    #endregion

    #region Gizmos
    void OnDrawGizmos()
    {
        _visualiserHandler?.DrawGizmos(workingVectorData, cols, rows, cellSize, transform, showGizmos, vectorColor, gizmoScale);
    }
    #endregion
}
