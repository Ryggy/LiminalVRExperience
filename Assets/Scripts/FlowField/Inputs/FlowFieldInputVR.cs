using UnityEngine;
using Liminal.SDK.VR;
using Liminal.SDK.VR.Input;
using TMPro;
public class FlowFieldInputVR : MonoBehaviour
{
    public FlowFieldTest flowField;
    public int influenceRadius = 1;

    private Vector3 startWorldPos;
    private bool isDragging = false;

    private IVRInputDevice rightHandInput;
    
    public TextMeshProUGUI debugText; 

    private void Start()
    {
        rightHandInput = VRDevice.Device?.PrimaryInputDevice;
    }
    void Update()
    {
        if (rightHandInput == null || rightHandInput.Pointer == null)
            return;

        var raycastResult = rightHandInput.Pointer.CurrentRaycastResult;
        string message = "";

        if (raycastResult.isValid)
        {
            Vector3 hitPos = raycastResult.worldPosition;
            message += $"Pointer Hit: {hitPos:F2}\n";

            // Convert to grid position
            int gridX = Mathf.FloorToInt(hitPos.x / flowField.scale);
            int gridY = Mathf.FloorToInt(hitPos.y / flowField.scale);

            // Check bounds
            if (gridX >= 0 && gridX < flowField.cols && gridY >= 0 && gridY < flowField.rows)
            {
                message += $"Grid Position: ({gridX}, {gridY})\n";
            }
            else
            {
                message += "Grid Position: Out of bounds\n";
            }
        }

        if (rightHandInput.GetButtonDown(VRButton.Trigger))
        {
            message += "Trigger Down\n";

        }

        if (rightHandInput.GetButton(VRButton.Trigger))
        {
            message += "Dragging\n";
        }
        
        if (rightHandInput.GetButtonUp(VRButton.Trigger))
        {
            message += "Trigger Up\n";
        }

        if (debugText != null)
        {
            debugText.text = message;
        }
        
        // Start dragging
        if (rightHandInput.GetButtonDown(VRButton.Trigger) && raycastResult.isValid)
        {
            startWorldPos = raycastResult.worldPosition;
            isDragging = true;
        }

        // While dragging
        if (isDragging && raycastResult.isValid)
        {
            Vector3 currentWorldPos = raycastResult.worldPosition;
            Vector3 dragDirection = currentWorldPos - startWorldPos;

            if (dragDirection.sqrMagnitude > 0.001f)
            {
                Vector2 gridPos = WorldToGrid(startWorldPos);
                ApplyForceToArea(gridPos, dragDirection.normalized);
                startWorldPos = currentWorldPos;
            }
        }

        // Stop dragging
        if (rightHandInput.GetButtonUp(VRButton.Trigger))
        {
            isDragging = false;
        }
    }

    private Vector2 WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / flowField.scale);
        int y = Mathf.FloorToInt(worldPos.y / flowField.scale);
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
                if (x * x + y * y > influenceRadius * influenceRadius)
                    continue;

                int targetX = centerX + x;
                int targetY = centerY + y;

                if (targetX >= 0 && targetX < flowField.cols && targetY >= 0 && targetY < flowField.rows)
                {
                    flowField.SetForce(new Vector2(targetX, targetY), direction);
                }
            }
        }
    }
}