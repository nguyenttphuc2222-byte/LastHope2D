using UnityEngine;

/// <summary>
/// Gắn lên prefab công trình / core / ore.
/// sizeX,sizeY là kích thước (số ô) mà prefab chiếm. (bottom-left origin)
/// </summary>
[DisallowMultipleComponent]
public class Placeable : MonoBehaviour
{
    [Tooltip("Width in grid cells (X)")]
    public int sizeX = 1;
    [Tooltip("Height in grid cells (Y)")]
    public int sizeY = 1;

    // Flag nếu object đã được đặt chính thức trên grid
    [HideInInspector] public bool isPlaced = false;
    // baseCell lưu vị trí bottom-left cell object được đặt
    [HideInInspector] public Vector2Int baseCell;

    // Tự set scale để sprite khớp 1 cell = grid cellSize; gọi khi cần
    public void AlignToCell(GridManager grid)
    {
        if (grid == null) return;
        // set position so that bottom-left corner of object aligns with baseCell
        Vector3 worldBottomLeft = new Vector3(grid.origin.x + baseCell.x * grid.cellSize,
                                              grid.origin.y + baseCell.y * grid.cellSize, 0f);
        // center pos
        float cx = worldBottomLeft.x + (sizeX * grid.cellSize) / 2f;
        float cy = worldBottomLeft.y + (sizeY * grid.cellSize) / 2f;
        transform.position = new Vector3(cx, cy, transform.position.z);
        // adjust localScale so that sprite size covers sizeX,sizeY cells if needed
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            float spriteUnitWidth = sr.sprite.bounds.size.x;
            float spriteUnitHeight = sr.sprite.bounds.size.y;
            // scale so sprite bounds match grid area
            float scaleX = (sizeX * grid.cellSize) / spriteUnitWidth;
            float scaleY = (sizeY * grid.cellSize) / spriteUnitHeight;
            transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }
    }

    // For debugging: draw rectangle in scene
    void OnDrawGizmosSelected()
    {
        var grid = FindObjectOfType<GridManager>();
        if (grid == null) return;
        Gizmos.color = Color.cyan;
        Vector3 bl = new Vector3(grid.origin.x + baseCell.x * grid.cellSize, grid.origin.y + baseCell.y * grid.cellSize, 0f);
        Vector3 tr = new Vector3(grid.origin.x + (baseCell.x + sizeX) * grid.cellSize, grid.origin.y + (baseCell.y + sizeY) * grid.cellSize, 0f);
        Gizmos.DrawLine(bl, new Vector3(tr.x, bl.y, 0f));
        Gizmos.DrawLine(bl, new Vector3(bl.x, tr.y, 0f));
        Gizmos.DrawLine(tr, new Vector3(bl.x, tr.y, 0f));
        Gizmos.DrawLine(tr, new Vector3(tr.x, bl.y, 0f));
    }
}