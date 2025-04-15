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

    //test
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


    public enum OutlineType
    {
        Fire,
        Lightning
    }

    void Start()
    {
        vectors = new Vector2[cols, rows];
        GenerateField();
    }

    void Update()
    {
        //GenerateField();
    }

    void GenerateField()
    {
        float yOffset = 0;
        for (int y = 0; y < rows; y++)
        {
            float xOffset = 0;
            for (int x = 0; x < cols; x++)
            {
                float angle = Mathf.PerlinNoise(xOffset, yOffset) * Mathf.PI * 2;
                vectors[x, y] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                xOffset += increment;
            }

            yOffset += increment;
        }

        zOffset += 0.004f;
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

    void OnDrawGizmos()
    {
        if (!showFlowField || vectors == null) return;
        Gizmos.color = Color.black;
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                Vector2 pos = new Vector2(x * scale, y * scale);
                Gizmos.DrawLine(pos, pos + vectors[x, y] * scale);
            }
        }

        // fire stuff
        if (showFlowField && vectors != null)
        {
 
            Gizmos.color = GetFlickeringColor();

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    if (outlineType == OutlineType.Fire && !IsInFireMask(x, y))
                        continue;

                    if (outlineType == OutlineType.Lightning && !IsInLightningMask(x, y))
                        continue;

                    Vector2 pos = new Vector2(x * scale, y * scale);
                    Gizmos.DrawLine(pos, pos + vectors[x, y] * scale * 0.5f);
                }
            }
        }
        // fire stuff

    }
    public Color GetFlickeringColor()
    {
        float lerpTime = Mathf.PingPong(Time.time * 0.5f, 1f); 
        Color lerpedColor1 = Color.Lerp(startColor, middleColor, lerpTime);
        Color lerpedColor2 = Color.Lerp(middleColor, endColor, lerpTime);
        return Color.Lerp(lerpedColor1, lerpedColor2, lerpTime);
    }

    public bool IsInFireMask(int x, int y)
    {
        int midX = cols / 2;
        int baseY = 5;
        int height = 25;

        if (y < baseY || y > baseY + height)
            return false;

        float t = (y - baseY) / (float)height;
  
        float flicker =
            Mathf.Sin(y * 0.5f + Time.time * flickerSpeed) * flickerAmount; 
        float width = Mathf.Lerp(fireBaseWidth, fireTipWidth, Mathf.Pow(t, 1.5f)) +
                      flicker; 
        width *= fireScale;
        return Mathf.Abs(x - midX) <= width;
    }

    // testing, need to delete
    bool IsInLightningMask(int x, int y)
    {
        int midX = cols / 2;
        int baseY = 5;
        int height = 25;

        if (y < baseY || y > baseY + height)
            return false;
        float zStep = Mathf.Floor((y + Time.time * 2f) / 5f); 
        float offsetX = zStep % 2 == 0 ? Mathf.Sin(y * 0.3f) * 5f : Mathf.Cos(y * 0.3f) * 5f;
        return Mathf.Abs(x - (midX + offsetX)) < 2;
    }
}