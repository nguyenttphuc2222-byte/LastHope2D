using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Settings")]
    public int width = 32;
    public int height = 18;
    public float cellSize = 1f;
    public Vector2 origin = Vector2.zero; // Góc dưới - trái map trong world

    // Ô map bị block (tường, đá, vách núi, v.v.)
    private bool[,] terrainBlocked;
    // Công trình chiếm ô
    private BuildingBase[,] buildingGrid;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple GridManager instances detected, destroying this one.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        terrainBlocked = new bool[width, height];
        buildingGrid = new BuildingBase[width, height];
    }

    #region Cell / World conversion
    public Vector2Int WorldToCell(Vector3 worldPosition)
    {
        Vector2 local = (Vector2)worldPosition - origin;
        int x = Mathf.FloorToInt(local.x / cellSize);
        int y = Mathf.FloorToInt(local.y / cellSize);
        return new Vector2Int(x, y);
    }

    public Vector3 CellToWorldCenter(Vector2Int cell)
    {
        // Trung tâm ô
        float x = origin.x + (cell.x + 0.5f) * cellSize;
        float y = origin.y + (cell.y + 0.5f) * cellSize;
        return new Vector3(x, y, 0f);
    }
    #endregion

    #region Bounds
    public bool IsInBounds(Vector2Int cell)
    {
        return cell.x >= 0 && cell.y >= 0 && cell.x < width && cell.y < height;
    }

    public Rect GetWorldBounds()
    {
        return new Rect(origin, new Vector2(width * cellSize, height * cellSize));
    }

    private void OnDrawGizmos()
    {
        // Vẽ khung map
        Gizmos.color = Color.green;
        Vector3 bottomLeft = new Vector3(origin.x, origin.y, 0f);
        Vector3 size = new Vector3(width * cellSize, height * cellSize, 0f);
        Gizmos.DrawWireCube(bottomLeft + size * 0.5f, size);
    }
    #endregion

    #region Terrain Block
    public void SetTerrainBlocked(Vector2Int cell, bool blocked)
    {
        if (!IsInBounds(cell)) return;
        terrainBlocked[cell.x, cell.y] = blocked;
    }

    public bool IsTerrainBlocked(Vector2Int cell)
    {
        if (!IsInBounds(cell)) return true; // ngoài map coi như blocked
        return terrainBlocked[cell.x, cell.y];
    }
    #endregion

    #region Buildings
    public bool IsCellOccupiedByBuilding(Vector2Int cell)
    {
        if (!IsInBounds(cell)) return true; // ngoài map coi như occupied
        return buildingGrid[cell.x, cell.y] != null;
    }

    public bool CanPlaceBuilding(BuildingBase building, Vector2Int anchorCell)
    {
        Vector2Int size = building.size;

        for (int dx = 0; dx < size.x; dx++)
        {
            for (int dy = 0; dy < size.y; dy++)
            {
                Vector2Int cell = new Vector2Int(anchorCell.x + dx, anchorCell.y + dy);
                if (!IsInBounds(cell))
                {
                    return false; // vượt ra ngoài map
                }

                if (IsTerrainBlocked(cell))
                {
                    return false; // ô thuộc block của map
                }

                if (IsCellOccupiedByBuilding(cell))
                {
                    return false; // ô đã có công trình khác
                }
            }
        }

        return true;
    }

    public void PlaceBuilding(BuildingBase building, Vector2Int anchorCell)
    {
        Vector2Int size = building.size;

        // Đánh dấu chiếm ô
        for (int dx = 0; dx < size.x; dx++)
        {
            for (int dy = 0; dy < size.y; dy++)
            {
                Vector2Int cell = new Vector2Int(anchorCell.x + dx, anchorCell.y + dy);
                if (IsInBounds(cell))
                {
                    buildingGrid[cell.x, cell.y] = building;
                }
            }
        }

        // Đặt vị trí world của gameObject về trung tâm vùng building
        Vector2Int topRightCell = new Vector2Int(anchorCell.x + size.x, anchorCell.y + size.y);
        Vector3 bottomLeftWorld = CellToWorldCenter(anchorCell) - new Vector3(cellSize * 0.5f, cellSize * 0.5f, 0);
        Vector3 topRightWorld = CellToWorldCenter(topRightCell - Vector2Int.one) + new Vector3(cellSize * 0.5f, cellSize * 0.5f, 0);

        Vector3 center = (bottomLeftWorld + topRightWorld) * 0.5f;
        building.transform.position = new Vector3(center.x, center.y, building.transform.position.z);

        building.OnPlaced(anchorCell);
    }

    public void ClearBuilding(BuildingBase building, Vector2Int anchorCell)
    {
        Vector2Int size = building.size;

        for (int dx = 0; dx < size.x; dx++)
        {
            for (int dy = 0; dy < size.y; dy++)
            {
                Vector2Int cell = new Vector2Int(anchorCell.x + dx, anchorCell.y + dy);
                if (IsInBounds(cell) && buildingGrid[cell.x, cell.y] == building)
                {
                    buildingGrid[cell.x, cell.y] = null;
                }
            }
        }
    }
    #endregion
}
