using UnityEngine;
using Liminal.SDK.VR;
using Liminal.SDK.VR.Input;
using TMPro;
public class FlowFieldInputVR : MonoBehaviour
{
    public FlowFieldTest flowField;
    public int influenceRadius = 5;

    private Vector3 startWorldPos;
    private bool isDragging = false;

    private IVRInputDevice rightHandInput;
    public TextMeshProUGUI debugText; 

    public Camera mainCamera;
    private bool useMouseInput = false;
    
    private void Start()
    {
        rightHandInput = VRDevice.Device?.PrimaryInputDevice;
    
        // Try to find the main camera, even if it's disabled
        if (mainCamera == null)
        {
            // This finds both active and inactive cameras
            Camera[] allCameras = Resources.FindObjectsOfTypeAll<Camera>();
            foreach (Camera cam in allCameras)
            {
                if (cam.CompareTag("MainCamera"))
                {
                    mainCamera = cam;
                    break;
                }
            }
        }
        // Detect if VR device is unavailable
        if (rightHandInput == null || rightHandInput.Pointer == null)
        {
            useMouseInput = true;

            if (mainCamera != null)
                mainCamera.gameObject.SetActive(true); // <<< Turn ON the camera GameObject itself
        }
        else
        {
            if (mainCamera != null)
                mainCamera.gameObject.SetActive(false); // <<< Optionally turn it OFF when VR detected
        }
    }

    void Update()
    {
        if (flowField == null)
            return;

        string message = "";

        if (useMouseInput)
        {
            HandleMouseInput(ref message);
        }
        else
        {
            HandleVRInput(ref message);
        }

        if (debugText != null)
        {
            debugText.text = message;
        }
    }

    private void HandleMouseInput(ref string message)
    {
        if (mainCamera == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        bool hitSomething = Physics.Raycast(ray, out hit);
        if (hitSomething)
        {
            Vector3 hitPos = hit.point;
            message += $"Mouse Hit: {hitPos:F2}\n";

            int gridX = Mathf.FloorToInt(hitPos.x / flowField.scale);
            int gridY = Mathf.FloorToInt(hitPos.y / flowField.scale);

            if (gridX >= 0 && gridX < flowField.cols && gridY >= 0 && gridY < flowField.rows)
            {
                message += $"Grid Position: ({gridX}, {gridY})\n";
            }
            else
            {
                message += "Grid Position: Out of bounds\n";
            }

            if (Input.GetMouseButtonDown(0))
            {
                startWorldPos = hitPos;
                isDragging = true;
                message += "Mouse Down\n";
            }

            if (isDragging)
            {
                Vector3 currentWorldPos = hitPos;
                Vector3 dragDirection = currentWorldPos - startWorldPos;

                if (dragDirection.sqrMagnitude > 0.001f)
                {
                    Vector2 gridPos = WorldToGrid(startWorldPos);
                    ApplyForceToArea(gridPos, dragDirection.normalized);
                    startWorldPos = currentWorldPos;
                    message += "Dragging\n";
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                message += "Mouse Up\n";
            }
        }
    }

    private void HandleVRInput(ref string message)
    {
        if (rightHandInput == null || rightHandInput.Pointer == null)
            return;

        var raycastResult = rightHandInput.Pointer.CurrentRaycastResult;

        if (raycastResult.isValid)
        {
            Vector3 hitPos = raycastResult.worldPosition;
            message += $"Pointer Hit: {hitPos:F2}\n";

            int gridX = Mathf.FloorToInt(hitPos.x / flowField.scale);
            int gridY = Mathf.FloorToInt(hitPos.y / flowField.scale);

            if (gridX >= 0 && gridX < flowField.cols && gridY >= 0 && gridY < flowField.rows)
            {
                message += $"Grid Position: ({gridX}, {gridY})\n";
            }
            else
            {
                message += "Grid Position: Out of bounds\n";
            }

            if (rightHandInput.GetButtonDown(VRButton.Trigger))
            {
                startWorldPos = hitPos;
                isDragging = true;
                message += "Trigger Down\n";
            }

            if (isDragging)
            {
                Vector3 currentWorldPos = hitPos;
                Vector3 dragDirection = currentWorldPos - startWorldPos;

                if (dragDirection.sqrMagnitude > 0.001f)
                {
                    Vector2 gridPos = WorldToGrid(startWorldPos);
                    ApplyForceToArea(gridPos, dragDirection.normalized);
                    startWorldPos = currentWorldPos;
                    message += "Dragging\n";
                }
            }

            if (rightHandInput.GetButtonUp(VRButton.Trigger))
            {
                isDragging = false;
                message += "Trigger Up\n";
            }
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