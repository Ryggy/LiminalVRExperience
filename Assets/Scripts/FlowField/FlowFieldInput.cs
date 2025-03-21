using UnityEngine;
using UnityEngine.Serialization;

public class FlowFieldInput : MonoBehaviour
{
    public FlowField flowField;
    public Camera mainCam;
    public float modificationIntensity = 0.1f;

    void Update()
    {
        if (Input.GetMouseButton(0)) // Left-click 
        {

            Vector2 mouseWorldPos = mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCam.transform.position.z));
            
            Debug.Log($"Mouse Button Pressed at position {mouseWorldPos}");
            int gridX = Mathf.FloorToInt(mouseWorldPos.x / flowField.cellSize);
            int gridY = Mathf.FloorToInt(mouseWorldPos.y / flowField.cellSize);

            flowField.ModifyScalarValue(gridX, gridY, modificationIntensity);
        }
        
        if (Input.GetMouseButton(1)) // Right-click 
        {

            Vector2 mouseWorldPos = mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCam.transform.position.z));
            
            Debug.Log($"Mouse Button Pressed at position {mouseWorldPos}");
            int gridX = Mathf.FloorToInt(mouseWorldPos.x / flowField.cellSize);
            int gridY = Mathf.FloorToInt(mouseWorldPos.y / flowField.cellSize);

            flowField.ModifyScalarValue(gridX, gridY, -modificationIntensity);
        }
    }
}