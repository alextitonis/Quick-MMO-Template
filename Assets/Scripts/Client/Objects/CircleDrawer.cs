using UnityEngine;

public class CircleDrawer : MonoBehaviour
{
    [SerializeField] float lineWidth = 1f;
    [SerializeField] float radius = .02f;

    [SerializeField] LineRenderer line;
    
    public void Draw()
    {
        line.enabled = true;
        int segments = 360;
        line.useWorldSpace = false;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.positionCount = segments + 1;

        int pointCount = segments + 1;
        Vector3[] points = new Vector3[pointCount];

        for (int i = 0; i < pointCount; i++)
        {
            float rad = Mathf.Deg2Rad * (i * 360f / segments);
            points[i] = new Vector3(Mathf.Sin(rad) * radius, 0, Mathf.Cos(rad) * radius);
        }

        line.SetPositions(points);
    }
    public void Clear()
    {
        line.positionCount = 0;
        line.SetPositions(new Vector3[0]);
        line.enabled = false;
    }
}