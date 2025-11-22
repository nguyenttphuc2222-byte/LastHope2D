using UnityEngine;

/// Hướng của băng chuyền theo grid
public enum ConveyorDirection
{
    Right,
    Up,
    Left,
    Down
}

/// Băng chuyền 1 ô, chứa tối đa 1 "item logic"
/// Hiện tại chỉ hỗ trợ tài nguyên Ore, không hiển thị item visual.
public class ConveyorBuilding : BuildingBase, IResourceSink
{
    [Header("Conveyor")]
    public ConveyorDirection direction = ConveyorDirection.Right;

    [Tooltip("Khoảng thời gian để cố gắng đẩy item sang ô kế tiếp (giây).")]
    public float moveInterval = 0.25f;

    [Tooltip("Loại tài nguyên mà belt cho phép (tạm thời chỉ dùng Ore).")]
    public ResourceType allowedType = ResourceType.Ore;

    // item hiện tại belt đang giữ (0 hoặc 1 stack)
    [SerializeField] private ResourceStack carriedItem;
    private float moveTimer;

    public bool HasItem => !carriedItem.IsEmpty;

    public ResourceStack CarriedItem => carriedItem;

    private GridManager gm;

    protected virtual void Start()
    {
        gm = GridManager.Instance;
    }

    private void Update()
    {
        if (!HasItem || gm == null) return;

        moveTimer -= Time.deltaTime;
        if (moveTimer <= 0f)
        {
            moveTimer += moveInterval;
            TryMoveForward();
        }
    }

    // ---- IResourceSink implementation (đầu vào của belt) ----

    public bool CanAccept(ResourceStack stack)
    {
        if (stack.IsEmpty) return false;   // stack rỗng
        if (HasItem) return false;         // belt đang giữ item rồi
        if (stack.type != allowedType && allowedType != ResourceType.None)
            return false;

        return true;
    }



    public int Accept(ResourceStack stack)
    {
        if (!CanAccept(stack)) return 0;

        int amount = Mathf.Max(1, stack.amount);
        // vì thiết kế mỗi belt chỉ chứa 1 item, nên cưỡng ép amount = 1
        carriedItem = new ResourceStack(stack.type, 1);
        moveTimer = moveInterval;
        return 1;
    }

    // ---- Logic đẩy item sang ô kế tiếp ----

    private void TryMoveForward()
    {
        if (gm == null || carriedItem.IsEmpty) return;

        Vector2Int myCell = gm.WorldToCell(transform.position);
        Vector2Int dir = DirToVector(direction);
        Vector2Int nextCell = myCell + dir;

        if (!gm.IsInBounds(nextCell))
            return;

        Vector3 nextCenter = gm.CellToWorldCenter(nextCell);

        // Tìm tất cả collider tại ô kế tiếp
        Collider2D[] hits = Physics2D.OverlapPointAll(nextCenter);
        if (hits == null || hits.Length == 0) return;

        // Ưu tiên tìm conveyor khác trước, sau đó mới các building khác
        IResourceSink bestSink = null;
        foreach (var h in hits)
        {
            if (h == null) continue;
            IResourceSink sink = h.GetComponentInParent<IResourceSink>();
            if (sink == null) continue;

            if (sink.CanAccept(carriedItem))
            {
                bestSink = sink;
                break;
            }
        }

        if (bestSink == null)
            return;

        int accepted = bestSink.Accept(carriedItem);
        if (accepted >= carriedItem.amount)
        {
            carriedItem = ResourceStack.Empty;
        }
        else
        {
            // lý thuyết sẽ không xảy ra vì amount = 1
            carriedItem.amount -= accepted;
        }
    }

    private Vector2Int DirToVector(ConveyorDirection dir)
    {
        switch (dir)
        {
            default:
            case ConveyorDirection.Right: return Vector2Int.right;
            case ConveyorDirection.Up: return Vector2Int.up;
            case ConveyorDirection.Left: return Vector2Int.left;
            case ConveyorDirection.Down: return Vector2Int.down;
        }
    }
}
