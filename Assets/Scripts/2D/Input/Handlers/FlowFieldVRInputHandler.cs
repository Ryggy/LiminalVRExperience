using UnityEngine;
using Liminal.SDK.VR;
using Liminal.SDK.VR.Input;

public class FlowFieldVRInputHandler : MonoBehaviour
{
    public FlowField2DVolume flowField;
    private IFlowFieldInteractor _interactor = new FlowFieldInteractor2D();
    
    [Range(1, 5)] public int influenceRadius = 1;
    [Range(0.1f, 10f)] public float smoothFactor = 10f;
    [Range(0f, 5f)] public float decayRate = 0.1f;
    private IVRInputDevice RightHandInput => VRDevice.Device?.PrimaryInputDevice;
    private Vector3 _startWorldPos;
    private bool _isDragging = false;
    private Vector2 _previousDragDirection;

    private void Start()
    {
        if (flowField == null)
        {
            flowField = GetComponent<FlowField2DVolume>();
        }
    }
    
    private void Update()
    {
        if (RightHandInput?.Pointer == null) return;

        var raycastResult = RightHandInput.Pointer.CurrentRaycastResult;
        if (!raycastResult.isValid) return;

        Vector3 hitPos = raycastResult.worldPosition;

        if (RightHandInput.GetButtonDown(VRButton.Trigger))
        {
            _startWorldPos = hitPos;
            _isDragging = true;
            _previousDragDirection = Vector2.zero;
            AudioManager.Instance?.PlayInteractSound();
        }

        if (RightHandInput.GetButtonUp(VRButton.Trigger))
        {
            _isDragging = false;
        }

        if (_isDragging)
        {
            Vector3 currentWorldPos = hitPos;
            Vector2 dragDir = currentWorldPos - _startWorldPos;
            if (dragDir.sqrMagnitude > 0.001f)
            {
                Vector2 smoothed = Vector2.Lerp(_previousDragDirection, dragDir.normalized, Time.deltaTime * smoothFactor);
                _interactor?.ApplyForce(WorldToGrid(currentWorldPos), smoothed, influenceRadius, flowField);
                _previousDragDirection = smoothed;
                _startWorldPos = currentWorldPos;
            }
        }
        
        GameController.Instance.playerIsInteracting = _isDragging;
        flowField.ApplyDecay(decayRate, Time.deltaTime);
    }

    private Vector2 WorldToGrid(Vector3 worldPos)
    {
        // Should be replaced by something injected or shared
        Vector2 localPos = worldPos - (Vector3)transform.position;
        int x = Mathf.FloorToInt(localPos.x / 10f);
        int y = Mathf.FloorToInt(localPos.y / 10f);
        return new Vector2(x, y);
    }
}
