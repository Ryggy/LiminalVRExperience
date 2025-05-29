using UnityEngine;
using Liminal.SDK.VR;
using Liminal.SDK.VR.Input;
using TMPro;

public class FlowFieldsInputAdvancedVR : MonoBehaviour
{
    [Header("Flow Field Settings")] public FlowFieldTest flowField;
    public int influenceRadius = 1;
    public float smoothFactor = 10f;

    [Header("Debugging")] public TextMeshProUGUI debugText;
    public bool showDebug = true;

    public Camera mainCamera;

    public GameController gameController;

    private Vector3 _startWorldPos;
    private bool _isDragging = false;
    private Vector2 _previousDragDirection = Vector2.zero;
    
    private IVRInputDevice RightHandInput => VRDevice.Device?.PrimaryInputDevice;
    public GameObject rightHand;
    public GameObject leftHand;

    private void Start()
    {
        if (flowField == null)
            flowField = GetComponent<FlowFieldTest>(); 
    }

    private void Update()
    {
        if (flowField == null)
            return;

        string message = "";

        VRInput(ref message);

        if (debugText != null && showDebug)
            debugText.text = message;
        
    }


    private void VRInput(ref string message)
    {
        if (mainCamera == null)
            return;
        
        if (Physics.Raycast(rightHand.transform.position, transform.forward, out RaycastHit hit))
        {
            Vector3 hitPos = hit.point;
            message += $"Mouse Hit: {hitPos:F2}\n";

            if (TryWorldToGrid(hitPos, out Vector2 gridPos))
            {
                message += $"Grid Position: ({(int)gridPos.x}, {(int)gridPos.y})\n";
                
                if (RightHandInput.GetButtonDown(VRButton.Trigger))
                {
                    _startWorldPos = hitPos;
                    _isDragging = true;
                    _previousDragDirection = Vector2.zero;

                    gameController.playerIsInteracting = true;
                }

                else
                {
                    gameController.playerIsInteracting = false;
                }

                if (_isDragging)
                {
                    Vector3 currentWorldPos = hitPos;
                    Vector2 dragDirection = (currentWorldPos - _startWorldPos);
                    if (dragDirection.sqrMagnitude > 0.001f)
                    {
                        Vector2 smoothed = Vector2.Lerp(_previousDragDirection, dragDirection.normalized,
                            Time.deltaTime * smoothFactor);
                        ApplyForceToArea(gridPos, smoothed);
                        _previousDragDirection = smoothed;
                        _startWorldPos = currentWorldPos;
                        message += "Dragging\n";
                    }
                }
            }
            else
            {
                message += "Grid Position: Out of bounds\n";
                gameController.playerIsInteracting = false;
            }
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
        int cx = (int)centerGridPos.x;
        int cy = (int)centerGridPos.y;

        for (int y = -influenceRadius; y <= influenceRadius; y++)
        {
            for (int x = -influenceRadius; x <= influenceRadius; x++)
            {
                if (x * x + y * y > influenceRadius * influenceRadius)
                    continue;

                int tx = cx + x;
                int ty = cy + y;

                if (tx >= 0 && tx < flowField.cols && ty >= 0 && ty < flowField.rows)
                    flowField.SetForce(new Vector2(tx, ty), direction);
            }
        }
    }


    private void OnDrawGizmos()
    {
        if (flowField == null) return;

        Gizmos.color = Color.yellow;
        Vector3 center = flowField.transform.position + new Vector3(flowField.cols * flowField.scale / 2f,
            flowField.rows * flowField.scale / 2f, 0f);
        Vector3 size = new Vector3(flowField.cols * flowField.scale, flowField.rows * flowField.scale, 0.1f);
        Gizmos.DrawWireCube(center, size);

        if (Application.isPlaying && mainCamera != null)
        {
            if (Physics.Raycast(rightHand.transform.position, transform.forward, out RaycastHit hit))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(hit.point, .2f);
            }
        }
    }
}