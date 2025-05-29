using UnityEngine;

public class FlowFieldMouseInputHandler : MonoBehaviour
{
    public FlowField2DVolume flowField;
    public Camera mainCamera;
    
    private IFlowFieldInteractor _interactor = new FlowFieldInteractor2D();

    [Range(1, 5)] public int influenceRadius = 1;
    [Range(0.1f, 10f)]public float smoothFactor = 10f;
    [Range(0f, 5f)] public float decayRate = 0.1f;
   
    private Vector3 _startWorldPos;
    private bool _isDragging = false;
    private Vector2 _previousDragDirection;

    private void Start()
    {
        if (flowField == null)
            flowField = GetComponent<FlowField2DVolume>();

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {
        // ensure this will not run within build
        if(!Application.isEditor) return;
        
        if (mainCamera == null || flowField == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 hitPos = hit.point;

            if (Input.GetMouseButtonDown(0))
            {
                _startWorldPos = hitPos;
                _isDragging = true;
                _previousDragDirection = Vector2.zero;
                AudioManager.Instance?.PlayInteractSound();
            }

            if (Input.GetMouseButtonUp(0))
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
    }

    private Vector2 WorldToGrid(Vector3 worldPos)
    {
        Vector2 localPos = worldPos - (Vector3)flowField.transform.position;
        int x = Mathf.FloorToInt(localPos.x / flowField.cellSize);
        int y = Mathf.FloorToInt(localPos.y / flowField.cellSize);
        return new Vector2(x, y);
    }
}