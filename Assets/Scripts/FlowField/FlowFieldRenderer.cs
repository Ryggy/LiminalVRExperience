using UnityEngine;
using System.Collections.Generic;

public class FlowFieldRenderer : MonoBehaviour
{
    public FlowField flowField;
    public GameObject linePrefab; // Prefab for LineRenderer
    public float arrowScale = 0.5f;

    private List<LineRenderer> arrows = new List<LineRenderer>();

    void Start()
    {
        GenerateArrows();
    }

    void Update()
    {
        UpdateArrows();
    }

    void GenerateArrows()
    {
        if (flowField == null || linePrefab == null) return;

        // Clear old arrows if any
        foreach (var arrow in arrows)
        {
            Destroy(arrow.gameObject);
        }
        arrows.Clear();

        for (int x = 0; x < flowField.width; x++)
        {
            for (int y = 0; y < flowField.height; y++)
            {
                GameObject arrowObj = Instantiate(linePrefab, transform);
                LineRenderer lr = arrowObj.GetComponent<LineRenderer>();

                if (lr)
                {
                    lr.positionCount = 3; // Start, end, and arrowhead
                    lr.startWidth = 0.05f;
                    lr.endWidth = 0.05f;
                    arrows.Add(lr);
                }
            }
        }
    }

    void UpdateArrows()
    {
        if (flowField == null) return;

        int index = 0;

        for (int x = 0; x < flowField.width; x++)
        {
            for (int y = 0; y < flowField.height; y++)
            {
                if (index >= arrows.Count) return;

                Vector3 position = new Vector3(x * flowField.cellSize, y * flowField.cellSize, 0);
                Vector2 direction = flowField.GetFlowDirection(x, y) * arrowScale;
                Vector3 endPosition = position + (Vector3)direction;

                LineRenderer lr = arrows[index];

                lr.SetPosition(0, position);
                lr.SetPosition(1, endPosition);
                
                // Arrowhead (simple 'V' shape)
                Vector3 arrowTip1 = endPosition + (Quaternion.Euler(0, 0, 135) * (Vector3)direction * 0.3f);
                lr.SetPosition(2, arrowTip1);

                index++;
            }
        }
    }
}