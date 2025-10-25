using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager I;

    [Header("Grid")]
    public Vector2 mapMin = new Vector2(-10, -10);
    public Vector2 mapMax = new Vector2(10, 10);
    public float cellSize = 1f;

    int width, height;
    bool[,] occupied;         // true = đã có vật thể chiếm ô

    void Awake()
    {
        I = this;
        width = Mathf.RoundToInt((mapMax.x - mapMin.x) / cellSize);
        height = Mathf.RoundToInt((mapMax.y - mapMin.y) / cellSize);
        occupied = new bool[width, height];
    }

    // ---- Chuyển đổi toạ độ
    public Vector2Int WorldToCell(Vector3 world)
    {
        int x = Mathf.FloorToInt((world.x - mapMin.x) / cellSize);
        int y = Mathf.FloorToInt((world.y - mapMin.y) / cellSize);
        return new Vector2Int(x, y);
    }
    public Vector3 CellToWorldCenter(int x, int y)
    {
        float wx = mapMin.x + (x + 0.5f) * cellSize;
        float wy = mapMin.y + (y + 0.5f) * cellSize;
        return new Vector3(wx, wy, 0);
    }

    // ---- Phạm vi & kiểm tra
    public bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < width && y < height;

    public bool RectFree(int x, int y, int w, int h)
    {
        for (int ix = 0; ix < w; ix++)
            for (int iy = 0; iy < h; iy++)
            {
                int cx = x + ix, cy = y + iy;
                if (!InBounds(cx, cy) || occupied[cx, cy]) return false;
            }
        return true;
    }

    public void SetRect(int x, int y, int w, int h, bool value)
    {
        for (int ix = 0; ix < w; ix++)
            for (int iy = 0; iy < h; iy++)
            {
                int cx = x + ix, cy = y + iy;
                if (InBounds(cx, cy)) occupied[cx, cy] = value;
            }
    }

    // Lấy cell neo (góc trái-dưới) gần nhất theo con trỏ, có xét kích thước
    public Vector2Int SnapAnchorCell(Vector3 worldPos, int sizeX, int sizeY)
    {
        var c = WorldToCell(worldPos);
        // Neo góc trái dưới: clamp để toàn bộ (sizeX×sizeY) nằm trong map
        c.x = Mathf.Clamp(c.x, 0, width - sizeX);
        c.y = Mathf.Clamp(c.y, 0, height - sizeY);
        return c;
    }

    // Cho make gizmos
    public (int w, int h) Size() => (width, height);
    public Vector2 Min() => mapMin;
}
