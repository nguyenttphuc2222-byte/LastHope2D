using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý grid ô vuông, occupancy và chuyển đổi world <-> cell.
/// Attach lên một GameObject (ví dụ GameManager).
/// </summary>
public class GridManager : MonoBehaviour
{
    public int width = 20;
    public int height = 20;
    public float cellSize = 1f;
    public Vector2 origin = Vector2.zero; // world coord của ô (0,0) ở bottom-left

    // occupancy: map cell -> placed GameObject
    Dictionary<Vector2Int, GameObject> occupancy = new Dictionary<Vector2Int, GameObject>();

    // --- chuyển đổi ---
    public Vector2Int WorldToCell(Vector2 world)
    {
        int x = Mathf.FloorToInt((world.x - origin.x) / cellSize);
        int y = Mathf.FloorToInt((world.y - origin.y) / cellSize);
        return new Vector2Int(x, y);
    }

    public Vector3 CellToWorldCenter(Vector2Int cell)
    {
        float x = origin.x + (cell.x + 0.5f) * cellSize;
        float y = origin.y + (cell.y + 0.5f) * cellSize;
        return new Vector3(x, y, 0f);
    }

    public bool IsInsideGrid(Vector2Int cell)
    {
        return cell.x >= 0 && cell.y >= 0 && cell.x < width && cell.y < height;
    }

    // Lấy danh sách cell nằm trong vùng (bottom-left = baseCell) kích thước (w,h)
    public List<Vector2Int> CellsForArea(Vector2Int baseCell, Vector2Int size)
    {
        var list = new List<Vector2Int>();
        for (int dx = 0; dx < size.x; dx++)
        {
            for (int dy = 0; dy < size.y; dy++)
            {
                list.Add(new Vector2Int(baseCell.x + dx, baseCell.y + dy));
            }
        }
        return list;
    }

    // Kiểm tra tất cả cell trong area đều trống và trong giới hạn
    public bool IsAreaFree(Vector2Int baseCell, Vector2Int size)
    {
        foreach (var c in CellsForArea(baseCell, size))
        {
            if (!IsInsideGrid(c)) return false;
            if (occupancy.ContainsKey(c)) return false;
        }
        return true;
    }

    // Đặt object tại area; trả false nếu không đặt được
    public bool PlaceObjectAt(GameObject go, Vector2Int baseCell, Vector2Int size)
    {
        if (!IsAreaFree(baseCell, size)) return false;
        foreach (var c in CellsForArea(baseCell, size)) occupancy[c] = go;
        return true;
    }

    // Xoá object (giải phóng các ô mà object chiếm)
    public void RemoveObject(GameObject go)
    {
        var keys = new List<Vector2Int>(occupancy.Keys);
        foreach (var k in keys)
        {
            if (occupancy[k] == go) occupancy.Remove(k);
        }
    }

    // Lấy danh sách ô mà object đang chiếm (nếu đã đặt)
    public List<Vector2Int> GetOccupiedCells(GameObject go)
    {
        var result = new List<Vector2Int>();
        foreach (var kv in occupancy) if (kv.Value == go) result.Add(kv.Key);
        return result;
    }

    // Editor/Debug: vẽ grid lines
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.grey;
        for (int x = 0; x <= width; x++)
        {
            Vector3 a = new Vector3(origin.x + x * cellSize, origin.y, 0f);
            Vector3 b = new Vector3(origin.x + x * cellSize, origin.y + height * cellSize, 0f);
            Gizmos.DrawLine(a, b);
        }
        for (int y = 0; y <= height; y++)
        {
            Vector3 a = new Vector3(origin.x, origin.y + y * cellSize, 0f);
            Vector3 b = new Vector3(origin.x + width * cellSize, origin.y + y * cellSize, 0f);
            Gizmos.DrawLine(a, b);
        }
    }
}