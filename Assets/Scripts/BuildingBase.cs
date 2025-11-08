using UnityEngine;

public class BuildingBase : MonoBehaviour
{
    [Tooltip("Kích thước tính theo block (ô grid). Ví dụ tường = (1,1), turret = (2,2).")]
    public Vector2Int size = Vector2Int.one;

    // Ô anchor (góc dưới - trái) của building trên grid
    public Vector2Int AnchorCell { get; private set; }

    public virtual void OnPlaced(Vector2Int anchorCell)
    {
        AnchorCell = anchorCell;
    }

    // Nếu sau này cần di chuyển building, thể thêm OnMoved, OnRemoved...
}
