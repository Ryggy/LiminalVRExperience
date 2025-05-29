using System.Collections.Generic;
using UnityEngine;
using Liminal.SDK.VR;
using Liminal.SDK.VR.Input;
using TMPro;

public class FlowFieldInputVR : MonoBehaviour
{
    public GameController gameController;

    [Header("Flow Field Settings")]
    public FlowField2DVolume  flowField;
    public int influenceRadius = 1;
    public float smoothFactor = 10f;
    [Range(0f, 5f)] public float decayRate = 0.1f;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public List<AudioClip> inputSFXClips = new List<AudioClip>();
    [Range(0.8f, 1.2f)] public float minPitch = 0.95f;
    [Range(0.8f, 1.2f)] public float maxPitch = 1.05f;
    [Tooltip("If enabled, plays two audio clips simultaneously.")]
    public bool playTwoClips = false;


    [Tooltip("If enabled, only plays the input sound once for the entire session.")]
    public bool playOnlyOnceEver = false;
    public bool playOnlyOncePerPress = false;
    private bool _hasPlayedThisPress = false;
    private bool _hasPlayedOnceEver = false;
    
    [Header("Debugging")]
    public TextMeshProUGUI debugText;
    public bool showDebug = true;

    [Header("Camera Control Settings")]
    [Tooltip("Enable this to allow mouse interaction. Requires tagged 'NonVRCamera'.")]
    public bool useMouseInput = false;
    public Camera mainCamera;

    private Vector3 _startWorldPos;
    private bool _isDragging = false;
    private Vector2 _previousDragDirection = Vector2.zero;

    private IVRInputDevice RightHandInput => VRDevice.Device?.PrimaryInputDevice;

    private void Start()
    {
        if (flowField == null)
            flowField = GetComponent<FlowField2DVolume>();

        if (useMouseInput && mainCamera == null)
            FindNonVRCamera();
    }

    private void Update()
    {
        if (flowField == null)
            return;
        
        string message = "";
        
        if (RightHandInput?.Pointer != null)
        {
            HandleVRInput(ref message);
        }
        
        if (useMouseInput && mainCamera != null)
        {
            HandleMouseInput(ref message);
        }
        
        flowField.ApplyDecay(decayRate, Time.deltaTime);
        
        if (debugText != null && showDebug)
        {
            debugText.text = message;
        }

        gameController.playerIsInteracting = _isDragging; 
    }

    private void HandleVRInput(ref string message)
    {
        var raycastResult = RightHandInput.Pointer.CurrentRaycastResult;

        if (raycastResult.isValid)
        {
            Vector3 hitPos = raycastResult.worldPosition;
            message += $"Pointer Hit: {hitPos:F2}\n";

            if (TryWorldToGrid(hitPos, out Vector2 gridPos))
            {
                message += $"Grid Pos: ({(int)gridPos.x}, {(int)gridPos.y})\n";

                if (RightHandInput.GetButtonDown(VRButton.Trigger))
                {
                    _startWorldPos = hitPos;
                    _isDragging = true;
                    _previousDragDirection = Vector2.zero;
                    message += "Trigger Down\n";

                    if (!playOnlyOncePerPress || !_hasPlayedThisPress)
                    {
                        PlayInputSFX();
                        _hasPlayedThisPress = true;
                    }
                }

                if (RightHandInput.GetButtonUp(VRButton.Trigger))
                {
                    _isDragging = false;
                    _hasPlayedThisPress = false;
                    message += "Trigger Up\n";
                }

                if (_isDragging)
                {
                    Vector3 currentWorldPos = hitPos;
                    Vector2 dragDirection = currentWorldPos - _startWorldPos;

                    if (dragDirection.sqrMagnitude > 0.001f)
                    {
                        Vector2 smoothed = Vector2.Lerp(_previousDragDirection, dragDirection.normalized, Time.deltaTime * smoothFactor);
                        ApplyForceToArea(gridPos, smoothed);
                        _previousDragDirection = smoothed;
                        _startWorldPos = currentWorldPos;
                        message += "Dragging\n";
                    }
                }

                if (RightHandInput.GetButtonUp(VRButton.Trigger))
                {
                    _isDragging = false;
                    message += "Trigger Up\n";
                }
            }
            else
            {
                message += "Grid Pos: Out of bounds\n";
            }
        }
        else
        {
            message += "Pointer not hitting any valid collider\n";
            gameController.playerIsInteracting = false;
        }
    }

    private void HandleMouseInput(ref string message)
    {
        if (mainCamera == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 hitPos = hit.point;
            message += $"Mouse Hit: {hitPos:F2}\n";

            if (TryWorldToGrid(hitPos, out Vector2 gridPos))
            {
                message += $"Grid Position: ({(int)gridPos.x}, {(int)gridPos.y})\n";

                if (Input.GetMouseButtonDown(0))
                {
                    _startWorldPos = hitPos;
                    _isDragging = true;

                    _previousDragDirection = Vector2.zero;
                    message += "Mouse Down\n";

                    if (!playOnlyOncePerPress || !_hasPlayedThisPress)
                    {
                        PlayInputSFX();
                        _hasPlayedThisPress = true;
                    }
                }

                if (Input.GetMouseButtonUp(0))
                {
                    _isDragging = false;
                    _hasPlayedThisPress = false;
                    message += "Mouse Up\n";
                }

                if (_isDragging)
                {
                    Vector3 currentWorldPos = hitPos;
                    Vector2 dragDirection = (currentWorldPos - _startWorldPos);
                    if (dragDirection.sqrMagnitude > 0.001f)
                    {
                        Vector2 smoothed = Vector2.Lerp(_previousDragDirection, dragDirection.normalized, Time.deltaTime * smoothFactor);
                        ApplyForceToArea(gridPos, smoothed);
                        _previousDragDirection = smoothed;
                        _startWorldPos = currentWorldPos;
                        message += "Dragging\n";
                    }
                }

                if (Input.GetMouseButtonUp(0))
                {
                    _isDragging = false;
                    message += "Mouse Up\n";
                }
            }
            else
            {
                message += "Grid Position: Out of bounds\n";
            }
        }
    }

    private void PlayInputSFX()
    {
        if (_hasPlayedOnceEver && playOnlyOnceEver)
            return;

        if (audioSource == null || inputSFXClips == null || inputSFXClips.Count == 0)
            return;

        audioSource.pitch = Random.Range(minPitch, maxPitch);

        if (!playTwoClips || inputSFXClips.Count == 1)
        {
            // Play only one random clip
            var clip = inputSFXClips[Random.Range(0, inputSFXClips.Count)];
            audioSource.PlayOneShot(clip);
        }
        else
        {
            // Play two different clips at once
            var clip1Index = Random.Range(0, inputSFXClips.Count);
            var clip2Index = clip1Index;

            // Ensure second clip is different
            while (clip2Index == clip1Index && inputSFXClips.Count > 1)
                clip2Index = Random.Range(0, inputSFXClips.Count);

            audioSource.PlayOneShot(inputSFXClips[clip1Index]);
            audioSource.PlayOneShot(inputSFXClips[clip2Index]);
        }

        if (playOnlyOnceEver)
            _hasPlayedOnceEver = true;
    }
    
    private void OnEnable()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
    private void OnDisable()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
    
    private bool TryWorldToGrid(Vector3 worldPos, out Vector2 gridPos)
    {
        Vector2 localPos = worldPos - (Vector3)flowField.transform.position;
        int x = Mathf.FloorToInt(localPos.x / flowField.cellSize);
        int y = Mathf.FloorToInt(localPos.y / flowField.cellSize);

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
    
    private void FindNonVRCamera()
    {
        if (mainCamera != null) return;

        GameObject camHolder = GameObject.FindWithTag("NonVRCamera");
        if (camHolder != null)
        {
            mainCamera = camHolder.GetComponentInChildren<Camera>(true);
            if (mainCamera != null)
                camHolder.SetActive(true);
        }
    }

    private void OnDrawGizmos()
    {
        if (flowField == null) return;

        Gizmos.color = Color.yellow;
        Vector3 center = flowField.transform.position + new Vector3(flowField.cols * flowField.cellSize / 2f, flowField.rows * flowField.cellSize / 2f, 0f);
        Vector3 size = new Vector3(flowField.cols * flowField.cellSize, flowField.rows * flowField.cellSize, 0.1f);
        Gizmos.DrawWireCube(center, size);

        if (Application.isPlaying && useMouseInput && mainCamera != null)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(hit.point, .2f);
            }
        }
    }
}
