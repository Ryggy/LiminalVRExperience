using UnityEngine;
using Liminal.SDK.VR;
using Liminal.SDK.VR.Input;

public class FlowFieldHandMovement : MonoBehaviour
{
    private IVRInputDevice RightHandInput => VRDevice.Device?.PrimaryInputDevice;
    public GameObject rightHand;
    public GameObject leftHand;
    private Vector3 _prevRightHandPos;
    private Vector3 _prevLeftHandPos;
    private Vector3 _rightHandVelocity;
    private Vector3 _leftHandVelocity;
    public bool up;
    public bool down;
    public bool left;
    public bool right;
 // Thresholds for similarity
    private const float DirectionSimilarityThreshold = 0.85f; // dot product, 1 = identical
    private const float SpeedSimilarityThreshold = 0.3f; // adjust as needed

    void Start()
    {
        if (rightHand != null)
            _prevRightHandPos = rightHand.transform.position;
        if (leftHand != null)
            _prevLeftHandPos = leftHand.transform.position;
    }

    void Update()
    {
        float deltaTime = Time.deltaTime;

        Vector3 rightHandDirection = Vector3.zero;
        Vector3 leftHandDirection = Vector3.zero;
        float rightHandSpeed = 0f;
        float leftHandSpeed = 0f;

        // Right Hand
        if (rightHand != null)
        {
            Vector3 currentRightPos = rightHand.transform.position;
            _rightHandVelocity = (currentRightPos - _prevRightHandPos) / deltaTime;
            _prevRightHandPos = currentRightPos;

            rightHandDirection = _rightHandVelocity.normalized;
            rightHandSpeed = _rightHandVelocity.magnitude;
        }

        // Left Hand
        if (leftHand != null)
        {
            Vector3 currentLeftPos = leftHand.transform.position;
            _leftHandVelocity = (currentLeftPos - _prevLeftHandPos) / deltaTime;
            _prevLeftHandPos = currentLeftPos;

            leftHandDirection = _leftHandVelocity.normalized;
            leftHandSpeed = _leftHandVelocity.magnitude;
        }

        // Check if both hands are moving similarly
        bool handsMovingSimilarly = false;
        if (rightHand != null && leftHand != null)
        {
            float directionDot = Vector3.Dot(rightHandDirection, leftHandDirection);
            float speedDiff = Mathf.Abs(rightHandSpeed - leftHandSpeed);

            handsMovingSimilarly = (directionDot > DirectionSimilarityThreshold) && (speedDiff < SpeedSimilarityThreshold);

            if (handsMovingSimilarly)
            {
                // Use the average direction for rounding
                Vector3 avgDirection = ((rightHandDirection + leftHandDirection) * 0.5f).normalized;
                SetDirectionFlags(avgDirection);

                Debug.Log($"Hands moving similarly. Rounded Direction: {GetDirectionName(avgDirection)}");
            }
            else
            {
                ResetDirectionFlags();
            }
        }
        else
        {
            ResetDirectionFlags();
        }
    }

    private void SetDirectionFlags(Vector3 direction)
    {
        // Reset all
        up = down = left = right = false;

        // Only consider Up, Down, Left, Right
        Vector3[] axes = { Vector3.up, Vector3.down, Vector3.left, Vector3.right };
        float maxDot = -1f;
        int bestAxis = -1;

        for (int i = 0; i < axes.Length; i++)
        {
            float dot = Vector3.Dot(direction, axes[i]);
            if (dot > maxDot)
            {
                maxDot = dot;
                bestAxis = i;
            }
        }

        // Set flags based on dominant axis
        switch (bestAxis)
        {
            case 0: up = true; break;    // Up
            case 1: down = true; break;  // Down
            case 2: left = true; break;  // Left
            case 3: right = true; break; // Right
        }
    }

    private void ResetDirectionFlags()
    {
        up = down = left = right = false;
    }

    private string GetDirectionName(Vector3 direction)
    {
        Vector3[] axes = { Vector3.up, Vector3.down, Vector3.left, Vector3.right };
        string[] axisNames = { "Up", "Down", "Left", "Right" };
        float maxDot = -1f;
        int bestAxis = -1;

        for (int i = 0; i < axes.Length; i++)
        {
            float dot = Vector3.Dot(direction, axes[i]);
            if (dot > maxDot)
            {
                maxDot = dot;
                bestAxis = i;
            }
        }
        return axisNames[bestAxis];
    }
}
