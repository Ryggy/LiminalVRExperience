using System.Collections.Generic;
using UnityEngine;
using Valve.VR.Extras;

/*
 * Summary:
 * The FlowFieldTest script generates and visualizes a dynamic flow field in Unity. It stores a grid of directional vectors and supports multiple behaviors based on the selected outline type (e.g., Fire, Normal).
 * It can display effects like fire flickering, and the flow field is rendered in the Scene view using Gizmos. The script also allows for manipulating the flow vectors at specific positions.
 */

public class FlowFieldTest : MonoBehaviour
{
    // Grid settings (hidden from Inspector) uses editor to display
    [HideInInspector] public int cols = 60, rows = 40;
    [HideInInspector] public float scale = 10f;
    [HideInInspector] public float increment = 0.1f;
    [HideInInspector] private float zOffset = 0f;
    [HideInInspector] private Vector2[,] vectors;
    [HideInInspector] public bool showFlowField = true;

    // Outline type setting (hidden from Inspector) uses editor to display
    [HideInInspector] public OutlineType outlineType = OutlineType.Fire;

    public enum OutlineType
    {
        Normal,
        Fire,
        Water,
        Air
    }

    // Reference to the behavior class (changes depending on outline type)
    private ElementBase behavior;

    // Dictionary of available behaviors for different outline types
    private Dictionary<OutlineType, ElementBase> behaviors;

    // Initializes the behavior dictionary with available outline types.
    void Awake()
    {
        behaviors = new Dictionary<OutlineType, ElementBase>()
        {
            { OutlineType.Normal, new NormalElement(this) },
            { OutlineType.Fire, new FireElement(this) },
            { OutlineType.Water, new WaterElement(this) },
            { OutlineType.Air, new AirElement(this) }       
        };
    }

    // Initializes the grid and sets up the flow field based on the selected behavior.
    void Start()
    {
        vectors = new Vector2[cols, rows];
        behavior = behaviors[outlineType];
        GenerateField();
    }

    // Generates the flow field by updating the flow vectors using the selected behavior.
    void GenerateField()
    {
        behavior.GenerateField(vectors, cols, rows, scale, increment, zOffset);
        zOffset += 0.004f;
    }

    // This method is used to regenerate the field based on outline type change
    public void RegenerateField()
    {
        // Ensure behavior is properly initialized based on the outlineType
        switch (outlineType)
        {
         
            case OutlineType.Fire:
                behavior = new FireElement(this);
                break;
            case OutlineType.Water:
                behavior = new WaterElement(this);
                break;
            // case OutlineType.Earth:
            //     behavior = new EarthElement(this);
            //     break;
            case OutlineType.Air:
                behavior = new AirElement(this);
                break;
            case OutlineType.Normal:
                behavior = new NormalElement(this);
                break;
        }

        if (behavior != null) 
        {
            // Generate the field if behavior is properly set
            GenerateField();
        }
        else 
        {
            Debug.LogError("Behavior is not set correctly!");
        }
    }
    

    // Retrieves the flow vector at a given position on the grid.
    // Converts the global position to local grid coordinates and returns the flow vector.
    public Vector2 GetForce(Vector2 position)
    {
        Vector2 localPosition = position - new Vector2(transform.position.x, transform.position.y);


        int x = Mathf.Clamp(Mathf.FloorToInt(localPosition.x / scale), 0, cols - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(localPosition.y / scale), 0, rows - 1);

        return vectors[x, y];
    }

    // Sets the flow vector at a specific position on the grid.
    // Updates the flow vector at the given grid coordinates.
    public void SetForce(Vector2 position, Vector2 force)
    {
        vectors[(int)position.x, (int)position.y] = force;
    }

    // Draws the flow field vectors in the Scene view using Gizmos.
    // It visualizes the direction and strength of the flow.
    void OnDrawGizmos()
    {
        if (!showFlowField || vectors == null) return;

        Gizmos.color = Color.black;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                Vector3 pos = new Vector3(x * scale, y * scale, 0) + transform.position;
                Gizmos.DrawLine(pos, pos + (Vector3)vectors[x, y] * scale);
            }
        }

        behavior?.DrawGizmos(vectors, cols, rows, scale, this);
    }


    // Draws additional gizmos when the object is selected in the editor.
    // Specifically for visualizing fire behavior and effects.
    // needs to  be adde dinto draw gizmos
    void OnDrawGizmosSelected()
    {
        if (vectors == null) return;

        if (outlineType == OutlineType.Fire)
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
        else if (outlineType == OutlineType.Water)
        {
            Gizmos.color = GetWaveColor();

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    if (!IsInWaterMask(x, y)) continue;

                    Vector3 pos = new Vector3(x * scale, y * scale, 0) + transform.position;
                    Gizmos.DrawLine(pos, pos + (Vector3)vectors[x, y] * scale * 0.5f);
                }
            }
        }
        else if (outlineType == OutlineType.Air)
        {
            Gizmos.color = GetWindColor();

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    if (!IsInAirFlowZone(x, y)) continue;

                    Vector3 pos = new Vector3(x * scale, y * scale, 0) + transform.position;

                    // Simulate swirling effect
                    float swirlX = Mathf.Sin((x + Time.time * windSpeed) * windFrequency) * windStrength;
                    float swirlY = Mathf.Cos((y + Time.time * windSpeed) * windFrequency) * windStrength;

                    // Add turbulence
                    swirlX += Random.Range(-airTurbulence, airTurbulence);
                    swirlY += Random.Range(-airTurbulence, airTurbulence);

                    Vector3 swirlVector = new Vector3(swirlX, swirlY, 0).normalized;

                    Gizmos.DrawLine(pos, pos + swirlVector * scale * 0.5f);
                }
            }
        }
    }


    #region Fire Element Code

    // Fire settings (hidden from Inspector) - uses editor to display
    [HideInInspector] public float fireScale = 9f;
    [HideInInspector] public float flickerSpeed = 0f;
    [HideInInspector] public float flickerAmount = .3f;
    [HideInInspector] public float fireBaseWidth = 0f;
    [HideInInspector] public float fireTipWidth = -2f;
    [HideInInspector] public Color startColor = Color.red;
    [HideInInspector] public Color middleColor = new Color(1f, 0.647f, 0f);
    [HideInInspector] public Color endColor = Color.yellow;

    // Returns a color that flickers between the start and end color for fire effect.
    public Color GetFlickeringColor()
    {
        float t = Mathf.PingPong(Time.time * 0.5f, 1.0f);
        return Color.Lerp(startColor, middleColor, Mathf.PingPong(Time.time * 0.2f, 1.0f));
    }

    // Checks if a specific position is within the "fire mask" region.
    // Checks if a specific position is within the "fire mask" region.
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

    #endregion // End of Fire Element Code


    #region Water Element Code

    // Water settings (hidden from Inspector) - uses editor to display
    [HideInInspector] public float waveAmplitude = 13f;
    [HideInInspector] public float waveFrequency = 0.1f;
    [HideInInspector] public float waveSpeed = 15f;
    [HideInInspector] public float waterLevel = 0f; // vertical center of the wave
    [HideInInspector] public Color waterStartColor = Color.cyan;
    [HideInInspector] public Color waterMiddleColor = new Color(0f, 0.5f, 1f);
    [HideInInspector] public Color waterEndColor = Color.blue;

    // Returns a dynamic wave color between cyan and blue.
    public Color GetWaveColor()
    {
        float t = Mathf.PingPong(Time.time * 0.5f, 1.0f);
        return Color.Lerp(waterStartColor, waterMiddleColor, Mathf.PingPong(Time.time * 0.2f, 1.0f));
    }

    // Checks if a specific position is within the "water mask" (splash zone).
    public bool IsInWaterMask(int x, int y)
    {
        float wave = Mathf.Sin((x + Time.time * waveSpeed) * waveFrequency) * waveAmplitude;
        return Mathf.Abs(y - waterLevel) <= wave;
    }

    #endregion // End of Water Element Code


    #region Air Element Code

// Air settings (hidden from Inspector) - uses editor to display
    [HideInInspector] public float windStrength = 1f; // The intensity of the wind flow (like waveAmplitude)
    [HideInInspector] public float windFrequency = 1f; // How frequently the air swirls (like waveFrequency)
    [HideInInspector] public float windSpeed = 1f; // Speed at which the wind moves across the field (like waveSpeed)
    [HideInInspector] public float airTurbulence = 1f; // Vertical center of the wind flow (like waterLevel)
    [HideInInspector] public float airBaseWidth = 1f; // Base width of the air flow
    [HideInInspector] public float airTipWidth = 1f; // Tip width of the air flow
    [HideInInspector] public Color airStartColor = Color.white; // Start color for the air element (wind)
    [HideInInspector] public Color airMiddleColor = new Color(0.5f, 0.8f, 1f); // Light blue for gentle wind
    [HideInInspector] public Color airEndColor = Color.gray; // End color for the air element

// Returns a dynamic wind color between white and gray.
    public Color GetWindColor()
    {
        float t = Mathf.PingPong(Time.time * 0.5f, 1.0f);
        return Color.Lerp(airStartColor, airMiddleColor, Mathf.PingPong(Time.time * 0.2f, 1.0f));
    }

// Checks if a specific position is within the "air flow zone" (similar to fire splash zone).
    public bool IsInAirFlowZone(int x, int y)
    {
        int midX = cols / 2;
        int baseY = 5; // Air base height
        int height = 25; // Air flow height range

        // Check if the position is outside the vertical range of the air flow zone
        if (y < baseY || y > baseY + height) return false;

        // Calculate the wind effect along the Y-axis and apply wind turbulence
        float t = (y - baseY) / (float)height;
        float flicker = Mathf.Sin(y * 0.5f + Time.time * windFrequency) * windStrength;
        float noise = Mathf.PerlinNoise(x * 0.15f, y * 0.1f + Time.time * 0.3f) * 3f;

        // Calculate width of the air flow at each Y level
        float width = (Mathf.Lerp(airBaseWidth, airTipWidth, t) + flicker + noise) * windStrength;

        // Check if the X position is within the air flow width
        return Mathf.Abs(x - midX) <= width;
    }

    #endregion // End of Air Element Code
}