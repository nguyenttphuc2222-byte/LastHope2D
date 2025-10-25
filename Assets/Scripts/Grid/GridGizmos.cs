using UnityEngine;

[ExecuteAlways]
public class GridGizmos : MonoBehaviour
{
    public Color lineColor = new Color(1, 1, 1, 0.2f);

    void OnDrawGizmos()
    {
        if (GridManager.I == null) return;
        Gizmos.color = lineColor;

        var (W, H) = GridManager.I.Size();
        float s = GridManager.I.cellSize;
        Vector2 min = GridManager.I.Min();

        // Vẽ đường dọc
        for (int x = 0; x <= W; x++)
        {
            float wx = min.x + x * s;
            Gizmos.DrawLine(new Vector3(wx, min.y, 0), new Vector3(wx, min.y + H * s, 0));
        }
        // Vẽ đường ngang
        for (int y = 0; y <= H; y++)
        {
            float wy = min.y + y * s;
            Gizmos.DrawLine(new Vector3(min.x, wy, 0), new Vector3(min.x + W * s, wy, 0));
        }
    }
}