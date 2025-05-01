using System.Collections.Generic;
using UnityEngine;
using Valve.VR.Extras;

public class FlowFieldTest : MonoBehaviour
{
    public int cols = 60, rows = 40;
    public float scale = 10f;
    public float increment = 0.1f;
    private float zOffset = 0f;
    private Vector2[,] vectors;

    // Fire settings
    public bool showFlowField = true;
    public float fireScale = 9f;
    public float flickerSpeed = 0f;
    public float flickerAmount = 0f;
    public float fireBaseWidth = 0f;
    public float fireTipWidth = -2f;
    public Color startColor = Color.red;
    public Color middleColor = new Color(1f, 0.647f, 0f);
    public Color endColor = Color.yellow;
    
    public OutlineType outlineType = OutlineType.Fire;
    public enum OutlineType { Normal, Fire, Lightning }

    private IElement behavior;
    private Dictionary<OutlineType, IElement> behaviors;

    void Awake()
    {
        behaviors = new Dictionary<OutlineType, IElement>()
        {
            { OutlineType.Normal,    new NormalElement() },
            { OutlineType.Fire,      new FireElement(this) },
            // { OutlineType.Lightning, new LightningBehavior() }
        };
    }

    void Start()
    {
        vectors = new Vector2[cols, rows];
        behavior = behaviors[outlineType];
        GenerateField();
    }

    void GenerateField()
    {
        behavior.GenerateField(vectors, cols, rows, scale, increment, zOffset);
        zOffset += 0.004f;
    }

    void OnDrawGizmos()
    {
        if (!showFlowField || vectors == null) return;


        Gizmos.color = Color.black;
        for (int y = 0; y < rows; y++)
        for (int x = 0; x < cols; x++)
        {
            // Adjust the position to be relative to the transform position
            Vector3 pos = new Vector3(x * scale, y * scale, 0) + new Vector3(transform.position.x, transform.position.y, transform.position.z);
            Gizmos.DrawLine(pos, pos + (Vector3)vectors[x, y] * scale);
        }

  
        if (behavior != null)
        {
            behavior.DrawGizmos(vectors, cols, rows, scale, this);
        }
    }
    void OnDrawGizmosSelected()
    {
        if (outlineType == OutlineType.Fire && vectors != null)
        {
            Gizmos.color = GetFlickeringColor();
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    if (!IsInFireMask(x, y)) continue;

                    Vector3 pos = new Vector3(x * scale, y * scale, 0) + transform.position;
                    Gizmos.DrawLine(pos, pos + (Vector3)vectors[x, y] * scale * 0.5f);
                }
            }
        }
    }
    

    public Color GetFlickeringColor()
    {
        float t = Mathf.PingPong(Time.time * 0.5f, 1.0f);
        return Color.Lerp(startColor, endColor, t);
    }

    public bool IsInFireMask(int x, int y)
    {
        int midX = cols / 2;
        int baseY = 5;
        int height = 25;
        if (y < baseY || y > baseY + height) return false;
        float t = (y - baseY) / (float)height;
        float flicker = Mathf.Sin(y * 0.5f + Time.time * flickerSpeed) * flickerAmount;
        float noise = Mathf.PerlinNoise(x * 0.15f, y * 0.1f + Time.time * 0.3f) * 3f;
        float width = (Mathf.Lerp(fireBaseWidth, fireTipWidth, t) + flicker + noise) * fireScale;
        return Mathf.Abs(x - midX) <= width;
    }
    
    public Vector2 GetForce(Vector2 position)
    {
        // Adjust the position by subtracting the object's position
        Vector2 localPosition = position - new Vector2(transform.position.x, transform.position.y);
        
        // Calculate grid coordinates based on the adjusted local position
        int x = Mathf.Clamp(Mathf.FloorToInt(localPosition.x / scale), 0, cols - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(localPosition.y / scale), 0, rows - 1);
        
        return vectors[x, y];
    }

    public void SetForce(Vector2 position, Vector2 force)
    {
        vectors[(int)position.x, (int)position.y] = force;
    }

}