using System.Collections.Generic;
using UnityEngine;
using Valve.VR.Extras;

public class FlowFieldTest : MonoBehaviour
{
    // Grid settings (hidden from Inspector) uses editor to display
    [HideInInspector] public int cols = 60, rows = 40;
    [HideInInspector] public float scale = 10f;
    [HideInInspector] public float increment = 0.1f;
    [HideInInspector] private float zOffset = 0f;  // Used for animation in the flow field
    [HideInInspector] private Vector2[,] vectors;  // Stores flow field vectors

    // Fire settings (hidden from Inspector) - uses editor to display
    [HideInInspector] public bool showFlowField = true;
    [HideInInspector] public float fireScale = 9f;
    [HideInInspector] public float flickerSpeed = 0f;
    [HideInInspector] public float flickerAmount = 0f;
    [HideInInspector] public float fireBaseWidth = 0f;
    [HideInInspector] public float fireTipWidth = -2f;
    [HideInInspector] public Color startColor = Color.red;
    [HideInInspector] public Color middleColor = new Color(1f, 0.647f, 0f); // Orange color
    [HideInInspector] public Color endColor = Color.yellow;
    
    // Outline type setting (hidden from Inspector) uses editor to display
    [HideInInspector] public OutlineType outlineType = OutlineType.Fire;

    public enum OutlineType { Normal, Fire, Water, Earth, Air }

    // Reference to the behavior class (changes depending on outline type)
    private ElementBase behavior;
    
    // Dictionary of available behaviors for different outline types
    private Dictionary<OutlineType, ElementBase> behaviors;

    // Initialize behavior dictionary
    void Awake()
    {
        behaviors = new Dictionary<OutlineType, ElementBase>()
        {
            { OutlineType.Normal, new NormalElement(this) },
            { OutlineType.Fire, new FireElement(this) },
            // { OutlineType.Water, new WaterElement(this) },  // New Water behavior
            // { OutlineType.Earth, new EarthElement(this) },  // New Earth behavior
            // { OutlineType.Air, new AirElement(this) }       // New Air behavior
        };
    }
    // Initialize vectors and generate flow field
    void Start()
    {
        vectors = new Vector2[cols, rows];  // Initialize the grid of vectors
        behavior = behaviors[outlineType];   // Set the behavior based on the selected outline type
        GenerateField();  // Generate the flow field based on the behavior
    }

    // Generate the flow field based on the selected behavior
    void GenerateField()
    {
        behavior.GenerateField(vectors, cols, rows, scale, increment, zOffset);
        zOffset += 0.004f;  // Increment the zOffset for animation
    }

    // Draw the flow field gizmos in the scene view
    void OnDrawGizmos()
    {
        if (!showFlowField || vectors == null) return;  // If flow field isn't shown, return early

        Gizmos.color = Color.black;  // Set the gizmo color to black

        // Draw lines representing the vectors in the flow field
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                // Calculate the position relative to the object transform
                Vector3 pos = new Vector3(x * scale, y * scale, 0) + transform.position;
                Gizmos.DrawLine(pos, pos + (Vector3)vectors[x, y] * scale);
            }
        }

        // Call the behavior's OnDrawGizmos method for custom gizmo drawing
        behavior?.DrawGizmos(vectors, cols, rows, scale, this);
    }

    // Draw selected gizmos (e.g., for fire behavior)
    void OnDrawGizmosSelected()
    {
        if (outlineType == OutlineType.Fire && vectors != null)
        {
            Gizmos.color = GetFlickeringColor();  // Get the flickering fire color

            // Draw lines for fire particles
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    if (!IsInFireMask(x, y)) continue;  // Skip if outside the fire mask

                    Vector3 pos = new Vector3(x * scale, y * scale, 0) + transform.position;
                    Gizmos.DrawLine(pos, pos + (Vector3)vectors[x, y] * scale * 0.5f);
                }
            }
        }
    }

    // Get the color that flickers between the start and end colors
    public Color GetFlickeringColor()
    {
        float t = Mathf.PingPong(Time.time * 0.5f, 1.0f);  // Create a flickering effect based on time
        return Color.Lerp(startColor, endColor, t);  // Interpolate between start and end colors
    }

    // Check if the given position is within the fire mask
    public bool IsInFireMask(int x, int y)
    {
        int midX = cols / 2;  // Middle of the grid in the x-axis
        int baseY = 5;        // Base y-position for the fire
        int height = 25;      // Height of the fire mask

        // If outside the fire region, return false
        if (y < baseY || y > baseY + height) return false;

        float t = (y - baseY) / (float)height;  // Normalize the y-position in the fire mask
        float flicker = Mathf.Sin(y * 0.5f + Time.time * flickerSpeed) * flickerAmount;  // Flickering effect
        float noise = Mathf.PerlinNoise(x * 0.15f, y * 0.1f + Time.time * 0.3f) * 3f;  // Perlin noise for randomness
        float width = (Mathf.Lerp(fireBaseWidth, fireTipWidth, t) + flicker + noise) * fireScale;  // Calculate the width of the fire

        // Return true if within the fire mask width
        return Mathf.Abs(x - midX) <= width;
    }

    // Get the flow force at a specific position
    public Vector2 GetForce(Vector2 position)
    {
        // Convert the position to the local grid coordinates
        Vector2 localPosition = position - new Vector2(transform.position.x, transform.position.y);

        // Calculate the grid coordinates for the position
        int x = Mathf.Clamp(Mathf.FloorToInt(localPosition.x / scale), 0, cols - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(localPosition.y / scale), 0, rows - 1);

        return vectors[x, y];  // Return the flow vector at the given grid position
    }

    // Set the flow force at a specific position
    public void SetForce(Vector2 position, Vector2 force)
    {
        vectors[(int)position.x, (int)position.y] = force;  // Set the force at the specified grid coordinates
    }
}