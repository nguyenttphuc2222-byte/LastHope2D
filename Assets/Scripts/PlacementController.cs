using UnityEngine;

public class PlacementController : MonoBehaviour
{
    public Camera cam;

    Placeable dragging;          // đang nhấc object nào?
    Vector2Int prevAnchor;       // lưu vị trí cũ để hoàn tác nếu cần

    void Start() { if (cam == null) cam = Camera.main; }

    void Update()
    {
        if (!BuildMode.Active) return;

        // Nhấn chuột trái: nếu chưa kéo -> thử "pick" object; nếu đang kéo -> thử "drop"
        if (Input.GetMouseButtonDown(0))
        {
            if (dragging == null) TryPick();
            else TryDrop();
        }

        // Chuột phải: hủy thao tác, trả object về chỗ cũ nếu đang kéo
        if (Input.GetMouseButtonDown(1) && dragging != null)
        {
            CancelDrag();
        }

        // Trong khi đang kéo: bám theo chuột và snap + preview
        if (dragging != null) FollowMouseAndPreview();
    }

    void TryPick()
    {
        // Raycast 2D tìm Placeable dưới trỏ chuột (ưu tiên kéo object đã có sẵn)
        var world = cam.ScreenToWorldPoint(Input.mousePosition); world.z = 0;
        var hit = Physics2D.OverlapPoint(world);
        Placeable p = hit ? hit.GetComponent<Placeable>() : null;

        if (p != null)
        {
            dragging = p;
            prevAnchor = p.anchorCell;

            if (p.isPlaced)
            {
                // Bỏ chiếm chỗ tạm thời để di chuyển tự do
                p.MarkOccupied(false);
                p.isPlaced = false;
            }
        }
        // Nếu không trúng object: bạn cũng có thể thiết kế chế độ "spawn prefab đang chọn"
        // Ở MVP này, mình tập trung vào kéo object có sẵn trong scene.
    }

    void TryDrop()
    {
        // Tính lại anchorCell theo chuột + kích thước
        Vector3 world = cam.ScreenToWorldPoint(Input.mousePosition); world.z = 0;
        Vector2Int anchor = GridManager.I.SnapAnchorCell(world, dragging.sizeX, dragging.sizeY);

        bool ok = GridManager.I.RectFree(anchor.x, anchor.y, dragging.sizeX, dragging.sizeY);
        if (ok)
        {
            dragging.MoveToAnchor(anchor);
            dragging.MarkOccupied(true);
            dragging.isPlaced = true;
            dragging.EndPreview();
            dragging = null;
        }
        else
        {
            // Không hợp lệ: vẫn giữ ở trạng thái kéo (cho thử chỗ khác),
            // hoặc nếu bạn muốn thì tự động trả về chỗ cũ:
            // dragging.MoveToAnchor(prevAnchor);
            // dragging.MarkOccupied(true);
            // dragging.isPlaced = true; dragging.EndPreview(); dragging = null;
        }
    }

    void CancelDrag()
    {
        // Trả về vị trí cũ (nếu object từng được đặt trước đó)
        if (dragging != null && prevAnchor != default)
        {
            dragging.MoveToAnchor(prevAnchor);
            dragging.MarkOccupied(true);
            dragging.isPlaced = true;
            dragging.EndPreview();
        }
        dragging = null;
    }

    void FollowMouseAndPreview()
    {
        Vector3 world = cam.ScreenToWorldPoint(Input.mousePosition); world.z = 0;
        Vector2Int anchor = GridManager.I.SnapAnchorCell(world, dragging.sizeX, dragging.sizeY);

        bool ok = GridManager.I.RectFree(anchor.x, anchor.y, dragging.sizeX, dragging.sizeY);
        dragging.PreviewValid(ok);
        dragging.MoveToAnchor(anchor); // Di theo chuột nhưng vẫn snap
    }
}