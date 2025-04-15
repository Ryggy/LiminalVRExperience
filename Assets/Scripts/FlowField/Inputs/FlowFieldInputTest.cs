using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowFieldInputTest : MonoBehaviour
{
    public FlowFieldTest flowField;

    private Vector3 startMouseWorldPos;
    private bool isDragging = false;
    [Range(1, 10)]
    public int influenceRadius = 1;

    // Store previous drag direction 
    private Vector2 previousDragDirection = Vector2.zero;
    public float smoothFactor = 0.1f; // Controls the smoothing 

    void Update()
    {
        // Start dragging
        if (Input.GetMouseButtonDown(0))
        {
            startMouseWorldPos = GetMouseWorldPosition();
            isDragging = true;
            previousDragDirection = Vector2.zero; // Reset direction when starting drag
        }

        if (isDragging)
        {
            Vector3 currentMouseWorldPos = GetMouseWorldPosition();
            Vector3 dragDirection = currentMouseWorldPos - startMouseWorldPos;

            if (dragDirection.sqrMagnitude > 0.001f) 
            {
                Vector2 gridPos = WorldToGrid(startMouseWorldPos);
                
                // Smooth the direction to avoid rapid changes
                Vector2 smoothedDirection = Vector2.Lerp(previousDragDirection, dragDirection.normalized, smoothFactor);

                ApplyForceToArea(gridPos, smoothedDirection);
                previousDragDirection = smoothedDirection; 
                startMouseWorldPos = currentMouseWorldPos; // continuous drag
            }
        }

        // Stop drag
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    // Raycast 
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
    
        float distance;
        Plane plane = new Plane(Vector3.forward, Vector3.zero); 
        if (plane.Raycast(ray, out distance))
        {
            return ray.GetPoint(distance);
        }

        return Vector3.zero; 
    }

    // Converts world position 
    private Vector2 WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / flowField.scale);
        int y = Mathf.FloorToInt(worldPos.y / flowField.scale);

        // Clamp 
        x = Mathf.Clamp(x, 0, flowField.cols - 1);
        y = Mathf.Clamp(y, 0, flowField.rows - 1);

        return new Vector2(x, y);
    }


    private void ApplyForceToArea(Vector2 centerGridPos, Vector2 direction)
    {
        int centerX = (int)centerGridPos.x;
        int centerY = (int)centerGridPos.y;

        for (int y = -influenceRadius; y <= influenceRadius; y++)
        {
            for (int x = -influenceRadius; x <= influenceRadius; x++)
            {
                int targetX = centerX + x;
                int targetY = centerY + y;

         
                if (x * x + y * y <= influenceRadius * influenceRadius)
                {
                    // Clamp 
                    if (targetX >= 0 && targetX < flowField.cols && targetY >= 0 && targetY < flowField.rows)
                    {
                        flowField.SetForce(new Vector2(targetX, targetY), direction);
                    }
                }
            }
        }
    }
}