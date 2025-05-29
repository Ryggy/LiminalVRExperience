using UnityEngine;

public class FlowFieldInput : MonoBehaviour
{
    public FlowFieldTest flowField;

    private Vector3 startMousePosition;
    private Vector3 currentMousePosition;
    private bool isDragging = false;

    void Update()
    {
        // Detect mouse button down (start dragging)
        if (Input.GetMouseButtonDown(0))
        {
            startMousePosition = Input.mousePosition;
            isDragging = true;
            Debug.Log("Mouse Button Down. Start Position: " + startMousePosition);
        }

        // While dragging, continuously track mouse movement
        if (isDragging)
        {
            currentMousePosition = Input.mousePosition;
            
            // Convert screen space to world space with a correct z value
            // Using Camera.main.WorldToScreenPoint to get the world position correctly
            Vector3 worldStartPosition = Camera.main.ScreenToWorldPoint(new Vector3(startMousePosition.x, startMousePosition.y, Camera.main.nearClipPlane));
            Vector3 worldCurrentPosition = Camera.main.ScreenToWorldPoint(new Vector3(currentMousePosition.x, currentMousePosition.y, Camera.main.nearClipPlane));
            
            // Set z to 0 for 2D positioning (no depth for 2D games)
            worldStartPosition.z = 0;
            worldCurrentPosition.z = 0;

            
            // Calculate the drag direction
            Vector3 dragDirection = worldCurrentPosition - worldStartPosition;

            // Debug the drag direction
            Debug.Log("Drag Direction: " + dragDirection);
            
            // Convert the world positions to grid coordinates
            Vector2 gridStartPosition = WorldToGrid(worldStartPosition);
            Vector2 gridCurrentPosition = WorldToGrid(worldCurrentPosition);

            // Set the force at the grid start position to match the drag direction
            flowField.SetForce(gridStartPosition, dragDirection.normalized);

            // Optionally update the starting mouse position for continuous dragging
            startMousePosition = currentMousePosition;
        }

        // Detect mouse button up (stop dragging)
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    // Convert world position to grid coordinates based on flow field's scale
    private Vector2 WorldToGrid(Vector3 worldPosition)
    {
        float gridX = Mathf.Floor(worldPosition.x / flowField.scale);
        float gridY = Mathf.Floor(worldPosition.y / flowField.scale);

        // Debug the conversion from world to grid position
        Debug.Log("World Position: " + worldPosition + " => Grid Position: (" + gridX + ", " + gridY + ")");
        
        return new Vector2(gridX, gridY);
    }
}
