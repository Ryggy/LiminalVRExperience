using UnityEngine;
using Liminal.SDK.VR;
using Liminal.SDK.VR.Input;
using TMPro;
using UnityEngine.EventSystems; // Only if you are using debug text!

[RequireComponent(typeof(FlowFieldTest))]
public class FlowFieldInputVR : MonoBehaviour
{
    [Header("Flow Field Settings")]
    public FlowFieldTest flowField;
    public int influenceRadius = 1;
    public float smoothFactor = 10f; // For drag smoothing

    [Header("Debugging")]
    public TextMeshProUGUI debugText; // Optional: assign a UI Text element
    public bool showDebug = true;

    private Vector3 _startWorldPos;
    private bool _isDragging = false;
    private Vector2 _previousDragDirection = Vector2.zero;

    private IVRInputDevice RightHandInput => VRDevice.Device?.PrimaryInputDevice;

    private void Start()
    {
        if (flowField == null)
            flowField = GetComponent<FlowFieldTest>();
    }

    private void Update()
    {
        if (RightHandInput == null || RightHandInput.Pointer == null)
            return;

        var raycastResult = RightHandInput.Pointer.CurrentRaycastResult;

        // Always update debug text if enabled
        if (showDebug)
            UpdateDebugInfo(raycastResult);

        // Handle input
        if (RightHandInput.GetButtonDown(VRButton.Trigger) && raycastResult.isValid)
        {
            _startWorldPos = raycastResult.worldPosition;
            _isDragging = true;
            _previousDragDirection = Vector2.zero;
        }

        if (_isDragging && raycastResult.isValid)
        {
            Vector3 currentWorldPos = raycastResult.worldPosition;
            Vector2 gridPos;

            if (TryWorldToGrid(_startWorldPos, out gridPos))
            {
                Vector3 dragDirection = currentWorldPos - _startWorldPos;

                if (dragDirection.sqrMagnitude > 0.001f)
                {
                    Vector2 smoothedDirection = Vector2.Lerp(_previousDragDirection, dragDirection.normalized, Time.deltaTime * smoothFactor);
                    ApplyForceToArea(gridPos, smoothedDirection);
                    _previousDragDirection = smoothedDirection;
                    _startWorldPos = currentWorldPos;
                }
            }
        }

        if (RightHandInput.GetButtonUp(VRButton.Trigger))
        {
            _isDragging = false;
        }
    }

    private bool TryWorldToGrid(Vector3 worldPos, out Vector2 gridPos)
    {
        Vector2 localPos = worldPos - (Vector3)flowField.transform.position;

        int x = Mathf.FloorToInt(localPos.x / flowField.scale);
        int y = Mathf.FloorToInt(localPos.y / flowField.scale);

        bool valid = x >= 0 && x < flowField.cols && y >= 0 && y < flowField.rows;

        gridPos = new Vector2(Mathf.Clamp(x, 0, flowField.cols - 1), Mathf.Clamp(y, 0, flowField.rows - 1));
        return valid;
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

    private void UpdateDebugInfo(RaycastResult raycastResult)
    {
        if (debugText == null) return;

        if (raycastResult.isValid)
        {
            Vector3 hitPos = raycastResult.worldPosition;
            Vector2 gridPos;
            bool validGrid = TryWorldToGrid(hitPos, out gridPos);

            debugText.text =
                $"Pointer Hit: {hitPos:F2}\n" +
                (validGrid ? $"Grid Pos: ({(int)gridPos.x}, {(int)gridPos.y})\n" : "Grid Pos: Out of bounds\n") +
                (_isDragging ? "Dragging\n" : (RightHandInput.GetButton(VRButton.Trigger) ? "Holding\n" : "Idle\n"));
        }
        else
        {
            debugText.text = "No valid pointer hit.";
        }
    }
}
