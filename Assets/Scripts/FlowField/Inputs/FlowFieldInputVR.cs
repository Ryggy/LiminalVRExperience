using UnityEngine;
using Liminal.SDK.VR;
using Liminal.SDK.VR.Input;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.XR;

public class FlowFieldInputVR : MonoBehaviour
{
    public FlowFieldTest flowField;
    public int influenceRadius = 5;

    private Vector3 startWorldPos;
    private bool isDragging = false;

    private IVRInputDevice rightHandInput;
    public TextMeshProUGUI debugText; 

    public Camera mainCamera;
    
    [Header("Camera Control Settings")]
    [Tooltip("Enable this toggle to find and activate the NonVRCamera. " +
             "Ensure this is turned ON before enabling mouse input for proper functionality.")]
    [FormerlySerializedAs("shouldFindCamera")] public bool NonVRCamera = false;  //  NEW: Manual toggle
    public bool useMouseInput = false;

    
 

    
    private void Start()
    {
        rightHandInput = VRDevice.Device?.PrimaryInputDevice;
    }

  void Update()
{
    // Ensure the flowField exists before proceeding
    if (flowField == null) return;

    string message = "";

    // Check if the camera toggle should be triggered
    if (NonVRCamera)
    {
        // If the camera is not already found, find it
        if (mainCamera == null)
        {
            FindNonVRCamera();
        }

        // If the camera is found, activate it
        if (mainCamera != null && !mainCamera.gameObject.activeSelf)
        {
            mainCamera.gameObject.SetActive(true);
            Debug.Log("NonVRCamera activated.");
        }
    }
    else
    {
        // If shouldFindCamera is false, deactivate the NonVRCamera
        if (mainCamera != null && mainCamera.gameObject.activeSelf)
        {
            mainCamera.gameObject.SetActive(false);
            Debug.Log("NonVRCamera deactivated.");
        }
    }

    // If input is active, either handle mouse input or VR input
    if (useMouseInput && mainCamera != null)
    {
        HandleMouseInput(ref message);  // Use the camera for mouse input
    }
    else if (XRSettings.isDeviceActive && rightHandInput?.Pointer != null)
    {
        if (mainCamera != null && mainCamera.gameObject.activeSelf)
        {
            mainCamera.gameObject.SetActive(false);  // Deactivate the camera for VR mode
            Debug.Log("NonVRCamera deactivated.");
        }

        HandleVRInput(ref message);  // Use VR input
    }

    // Update debug message
    if (debugText != null)
    {
        debugText.text = message;
    }
}

  private void FindNonVRCamera()
  {
      if (mainCamera != null) return;

      GameObject camHolder = GameObject.FindWithTag("NonVRCamera");
      if (camHolder != null)
      {
          mainCamera = camHolder.GetComponentInChildren<Camera>(true); // true = include inactive
          if (mainCamera != null)
          {
              camHolder.SetActive(true); // activate the whole object (camera and its holder)
              Debug.Log("Found and activated NonVRCamera.");
          }
          else
          {
              Debug.LogWarning("Tagged object found but no Camera component under it.");
          }
      }
      else
      {
          Debug.LogWarning("No GameObject tagged 'NonVRCamera' found.");
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
    
    
    void OnDrawGizmos()
    {
        if (flowField == null)
            return;

        // Set gizmo color
        Gizmos.color = Color.yellow;

        // Draw a wireframe box representing the flow field area
        Vector3 center = new Vector3(flowField.cols * flowField.scale / 2f, flowField.rows * flowField.scale / 2f, 0f);
        center += flowField.transform.position; // Offset by the flowField object's position
        Vector3 size = new Vector3(flowField.cols * flowField.scale, flowField.rows * flowField.scale, 0.1f);

        Gizmos.DrawWireCube(center, size);

        // Optional: Draw the current hit point if available
        if (Application.isPlaying && useMouseInput && mainCamera != null)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(hit.point, .2f);  // Draw a small sphere where the mouse is hitting
            }
        }
    }
}