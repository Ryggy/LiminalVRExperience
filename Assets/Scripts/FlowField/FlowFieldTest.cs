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
    public float fireScale = 3f;
    public float flickerSpeed = 2f;
    public float flickerAmount = 1.5f;
    public float fireBaseWidth = 12f;
    public float fireTipWidth = 1.5f;
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

        // Base flow field in black
        Gizmos.color = Color.black;
        for (int y = 0; y < rows; y++)
        for (int x = 0; x < cols; x++)
        {
            Vector2 pos = new Vector2(x * scale, y * scale);
            Gizmos.DrawLine(pos, pos + vectors[x, y] * scale * 0.5f);
        }

        // Only draw overlay if behavior is assigned
        if (behavior != null)
        {
            behavior.DrawGizmos(vectors, cols, rows, scale, this);
        }
    }
    void OnDrawGizmosSelected()
    {
        // if (vectors == null) return;
        //
        // for (int y = 0; y < rows; y++)
        // for (int x = 0; x < cols; x++)
        // {
        //     if (IsInFireMask(x, y))
        //     {
        //         Gizmos.color = new Color(1f, 0.3f, 0f, 0.6f); // semi-transparent orange-red
        //         Vector3 pos = new Vector3(x * scale, y * scale, 0);
        //         Gizmos.DrawCube(pos, Vector3.one * scale * 0.3f); // small cubes for visual fire zone
        //     }
        // }
        //
        
        // 2) If we're in Fire mode, draw the fire‐masked lines on top in flickering color
        if (outlineType == OutlineType.Fire)
        {
            Gizmos.color = GetFlickeringColor();
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    if (!IsInFireMask(x, y)) continue;
                    Vector2 pos = new Vector2(x * scale, y * scale);
                    Gizmos.DrawLine(pos, pos + vectors[x, y] * scale * 0.5f);
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
        int x = Mathf.Clamp(Mathf.FloorToInt(position.x / scale), 0, cols - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(position.y / scale), 0, rows - 1);
        return vectors[x, y];
    }

    public void SetForce(Vector2 position, Vector2 force)
    {
        vectors[(int)position.x, (int)position.y] = force;
    }
    
    void Update()
    {
        // Optional: uncomment to regenerate every frame
        // GenerateField();
    }
    /// <summary>
    /// If you want turbulence everywhere, including the fire, and you don’t mind managing the ring‑effect → stick with this
    /// </summary>
    // void GenerateField()
    // {
    //     float yOffset = 0;
    //     for (int y = 0; y < rows; y++)
    //     {
    //         float xOffset = 0;
    //         for (int x = 0; x < cols; x++)
    //         {
    //             // float angle = Mathf.PerlinNoise(xOffset, yOffset) * Mathf.PI * 2;
    //             // vectors[x, y] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    //             
    //             
    //             // Add vertical bias
    //             // float angle = Mathf.PerlinNoise(xOffset, yOffset) * Mathf.PI * 2;
    //             // Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) + 1f).normalized;
    //             // vectors[x, y] = direction;
    //             //
    //             
    //             float angle = Mathf.PerlinNoise(xOffset, yOffset + zOffset) * Mathf.PI * 2;
    //             Vector2 baseDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    //             Vector2 upwardBias = new Vector2(0, 1);
    //             vectors[x, y] = (baseDirection + upwardBias * 0.5f).normalized;
    //             
    //             
    //             
    //             
    //             
    //             
    //             xOffset += increment;
    //         }
    //         yOffset += increment;
    //     }
    //
    //     zOffset += 0.004f;
    // }

    /// <summary>
    /// If you want no rings in the fire at all and don’t care about any swirling inside it 
    /// </summary>
    // void GenerateField()
    // {
    //     float yOffset = 0f;
    //     for (int y = 0; y < rows; y++)
    //     {
    //         float xOffset = 0f;
    //         for (int x = 0; x < cols; x++)
    //         {
    //             Vector2 dir;
    //
    //             if (outlineType == OutlineType.Fire && IsInFireMask(x, y))
    //             {
    //                 // Fire region: straight up + small Perlin jitter on X
    //                 float jitter = (Mathf.PerlinNoise(x * 0.1f, Time.time * 0.5f) - 0.5f) * 0.3f;
    //                 dir = new Vector2(jitter, 1f).normalized;
    //             }
    //             else // if not in ring, keep it normal
    //             {
    //                 // Outside fire: full swirling field (or straight up for Normal)
    //                 float noise = Mathf.PerlinNoise(xOffset, yOffset + zOffset);
    //                 float angle = noise * Mathf.PI * 2f;
    //                 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    //             }
    //
    //             vectors[x, y] = dir;
    //             xOffset += increment;
    //         }
    //
    //         yOffset += increment;
    //     }
    //
    //     zOffset += 0.004f;
    // }

    //If you want a bit of flame‑lick motion (a little swirl but always rising) 
    // void GenerateField()
    // {
    //     float yOffset = 0;
    //     for (int y = 0; y < rows; y++)
    //     {
    //         float xOffset = 0;
    //         for (int x = 0; x < cols; x++)
    //         {
    //             float noise = Mathf.PerlinNoise(xOffset, yOffset);
    //             float angle;
    //
    //             if (outlineType == OutlineType.Fire)
    //             {
    //                 // Skew the angle to point mostly upward with flicker
    //                 float upwardBias = Mathf.Lerp(0.4f, 0.6f, noise); // around upward (90 degrees)
    //                 angle = upwardBias * Mathf.PI; 
    //                 angle += Mathf.Sin(Time.time + x * 0.3f + y * 0.2f) * 0.5f; // flicker
    //             }
    //             else
    //             {
    //                 angle = noise * Mathf.PI * 2; // original circular pattern
    //             }
    //
    //             vectors[x, y] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    //             xOffset += increment;
    //         }
    //
    //         yOffset += increment;
    //     }
    //
    //     zOffset += 0.004f;
    // }


    // void OnDrawGizmos()
    // {
    //     if (!showFlowField || vectors == null) return;
    //
    //     Gizmos.color = outlineType == OutlineType.Fire ? GetFlickeringColor() : Color.black;
    //
    //     for (int y = 0; y < rows; y++)
    //     {
    //         for (int x = 0; x < cols; x++)
    //         {
    //             if (outlineType == OutlineType.Fire && !IsInFireMask(x, y)) continue;
    //             if (outlineType == OutlineType.Lightning && !IsInLightningMask(x, y)) continue;
    //             // In Normal mode, allow all points
    //
    //             Vector2 pos = new Vector2(x * scale, y * scale);
    //             Gizmos.DrawLine(pos, pos + vectors[x, y] * scale * 0.5f);
    //         }
    //     }
    // }

    // void OnDrawGizmos()
    // {
    //     if (!showFlowField || vectors == null) return;
    //
    //     // 1) Draw the base flow‐field in black for every mode
    //     Gizmos.color = Color.black;
    //     for (int y = 0; y < rows; y++)
    //     {
    //         for (int x = 0; x < cols; x++)
    //         {
    //             Vector2 pos = new Vector2(x * scale, y * scale);
    //             Gizmos.DrawLine(pos, pos + vectors[x, y] * scale * 0.5f);
    //         }
    //     }
    //
    //     // 2) If we're in Fire mode, draw the fire‐masked lines on top in flickering color
    //     if (outlineType == OutlineType.Fire)
    //     {
    //         Gizmos.color = GetFlickeringColor();
    //         for (int y = 0; y < rows; y++)
    //         {
    //             for (int x = 0; x < cols; x++)
    //             {
    //                 if (!IsInFireMask(x, y)) continue;
    //                 Vector2 pos = new Vector2(x * scale, y * scale);
    //                 Gizmos.DrawLine(pos, pos + vectors[x, y] * scale * 0.5f);
    //             }
    //         }
    //     }
    //
    //     // 3) (Optional) Lightning overlay—if you want lightning on top of normal lines:
    //     else if (outlineType == OutlineType.Lightning)
    //     {
    //         Gizmos.color = Color.black;
    //         for (int y = 0; y < rows; y++)
    //         {
    //             for (int x = 0; x < cols; x++)
    //             {
    //                 if (!IsInLightningMask(x, y)) continue;
    //                 Vector2 pos = new Vector2(x * scale, y * scale);
    //                 Gizmos.DrawLine(pos, pos + vectors[x, y] * scale * 0.5f);
    //             }
    //         }
    //     }
    // }

    // public bool IsInLightningMask(int x, int y)
    // {
    //     int midX = cols / 2;
    //     int baseY = 5;
    //     int height = 25;
    //
    //     if (y < baseY || y > baseY + height)
    //         return false;
    //
    //     float zStep = Mathf.Floor((y + Time.time * 2f) / 5f);
    //     float offsetX = zStep % 2 == 0 ? Mathf.Sin(y * 0.3f) * 5f : Mathf.Cos(y * 0.3f) * 5f;
    //
    //     return Mathf.Abs(x - (midX + offsetX)) < 2;
    // }
}