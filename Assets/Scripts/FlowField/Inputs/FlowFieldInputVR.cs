using UnityEngine;
using Liminal.SDK.VR;
using Liminal.SDK.VR.Avatars;
using Liminal.SDK.VR.Input;

public class FlowFieldInputVR : MonoBehaviour
{
    public FlowFieldTest flowField;
    public float maxRaycastDistance = 10f;
    public int influenceRadius = 1;

    private Vector3 startWorldPos;
    private bool isDragging = false;

    private IVRInputDevice inputDevice => VRDevice.Device.PrimaryInputDevice;
    private Transform handTransform => VRAvatar.Active.PrimaryHand.Transform;

    void Update()
    {
        if (inputDevice == null || handTransform == null)
            return;

        // Start drag
        if (inputDevice.GetButtonDown(VRButton.Trigger))
        {
            if (TryGetPointerWorldPosition(out startWorldPos))
            {
                isDragging = true;
            }
        }

        // Dragging
        if (isDragging)
        {
            if (TryGetPointerWorldPosition(out Vector3 currentWorldPos))
            {
                Vector3 dragDirection = currentWorldPos - startWorldPos;

                if (dragDirection.sqrMagnitude > 0.001f)
                {
                    Vector2 gridPos = WorldToGrid(startWorldPos);
                    ApplyForceToArea(gridPos, dragDirection.normalized);
                    startWorldPos = currentWorldPos;
                }
            }
        }

        // Stop drag
        if (inputDevice.GetButtonUp(VRButton.Trigger))
        {
            isDragging = false;
        }
    }

    private bool TryGetPointerWorldPosition(out Vector3 worldPos)
    {
        Ray ray = new Ray(handTransform.position, handTransform.forward);
        Plane plane = new Plane(Vector3.forward, 0f); // Assuming Z = 0 plane

        if (plane.Raycast(ray, out float distance))
        {
            worldPos = ray.GetPoint(distance);
            return true;
        }

        worldPos = Vector3.zero;
        return false;
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
