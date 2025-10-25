using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Placeable : MonoBehaviour
{
    [Header("Kích thước tính theo ô")]
    public int sizeX = 1;
    public int sizeY = 1;

    [Header("Trạng thái")]
    public bool isPlaced;          // đã “thả” xuống map?
    public Vector2Int anchorCell;  // cell neo (góc trái-dưới)

    SpriteRenderer sr;
    Color baseColor;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        baseColor = sr.color;
    }

    public void PreviewValid(bool valid)
    {
        // Xanh = hợp lệ, Đỏ = không hợp lệ (độ trong suốt 60%)
        sr.color = valid ? new Color(0f, 1f, 0f, 0.6f) : new Color(1f, 0f, 0f, 0.6f);
    }
    public void EndPreview()
    {
        sr.color = baseColor;
    }

    // Ghi dấu các ô chiếm dụng (khi đặt xuống hoặc nhấc lên)
    public void MarkOccupied(bool value)
    {
        GridManager.I.SetRect(anchorCell.x, anchorCell.y, sizeX, sizeY, value);
    }

    // Cập nhật vị trí thế giới dựa trên anchor + kích thước
    public void MoveToAnchor(Vector2Int cellAnchor)
    {
        anchorCell = cellAnchor;
        // Tâm hình chữ nhật sizeX×sizeY nằm lệch nửa ô:
        float cx = anchorCell.x + sizeX / 2f - 0.5f;
        float cy = anchorCell.y + sizeY / 2f - 0.5f;
        Vector3 center = GridManager.I.CellToWorldCenter(Mathf.FloorToInt(cx), Mathf.FloorToInt(cy));
        // Cẩn thận: công thức trên quy đổi trực tiếp trung tâm hình chữ nhật theo cell size
        float wx = GridManager.I.Min().x + (anchorCell.x + sizeX / 2f) * GridManager.I.cellSize;
        float wy = GridManager.I.Min().y + (anchorCell.y + sizeY / 2f) * GridManager.I.cellSize;
        transform.position = new Vector3(wx, wy, 0);
    }
}