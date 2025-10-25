using UnityEngine;

[ExecuteAlways]
public class MapBounds2D : MonoBehaviour
{
    public GridManager grid;            // kéo thả GridManager vào đây
    public bool autoApplyToGrid = true; // bật để tự copy min/max sang Grid

    [Header("Nguồn lấy bounds (chọn 1)")]
    public SpriteRenderer spriteRenderer; // nếu map là Sprite
    public Collider2D sourceCollider;         // nếu map có BoxCollider2D

    [Header("Output (để xem trong Inspector)")]
    public Vector2 mapMinOut;
    public Vector2 mapMaxOut;
    public Vector2 sizeOut;

    Bounds GetBounds()
    {
        if (spriteRenderer != null) return spriteRenderer.bounds;
        if (sourceCollider != null) return sourceCollider.bounds; // <--- và ở đây
        return new Bounds(transform.position, Vector3.one * 10f);
    }

    void Update()
    {
        var b = GetBounds();
        mapMinOut = new Vector2(b.min.x, b.min.y);
        mapMaxOut = new Vector2(b.max.x, b.max.y);
        sizeOut = new Vector2(b.size.x, b.size.y);

        if (autoApplyToGrid && grid != null)
        {
            grid.mapMin = mapMinOut;
            grid.mapMax = mapMaxOut;
        }

        // Nhấn M để in ra Console nếu muốn
        if (Application.isPlaying && Input.GetKeyDown(KeyCode.M))
            Debug.Log($"Map bounds → min:{mapMinOut}  max:{mapMaxOut}  size:{sizeOut}");
    }

    void OnDrawGizmos()
    {
        var b = GetBounds();
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(b.center, b.size);
    }

    // Dùng menu chuột phải trên component để “áp vào Grid” một lần
    [ContextMenu("Apply To Grid Now")]
    void ApplyOnce()
    {
        var b = GetBounds();
        if (grid == null) return;
        grid.mapMin = new Vector2(b.min.x, b.min.y);
        grid.mapMax = new Vector2(b.max.x, b.max.y);
    }
}
