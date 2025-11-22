using UnityEngine;

/// Gắn script này lên Drill (cùng GameObject với OreBuilding + OreBuildingResourceIO)
/// Mỗi lần tick, nó thử đẩy ore sang 1 trong 4 ô xung quanh theo thứ tự ưu tiên:
/// (-1,0) trái → (0,1) trên → (1,0) phải → (0,-1) dưới.
[RequireComponent(typeof(OreBuildingResourceIO))]
public class DrillToConveyorOutput : MonoBehaviour
{
    public ResourceType outputType = ResourceType.Ore;

    [Tooltip("Khoảng thời gian giữa các lần thử đẩy ore (giây).")]
    public float outputInterval = 0.5f;

    [Tooltip("Số ore / lần xuất (nên để = 1).")]
    public int amountPerTick = 1;

    private OreBuildingResourceIO sourceIO;
    private GridManager gm;
    private float timer;

    // Thứ tự ưu tiên 4 hướng (theo grid)
    private static readonly Vector2Int[] kDirs =
    {
        new Vector2Int(-1, 0), // trái
        new Vector2Int(0,  1), // trên
        new Vector2Int(1,  0), // phải
        new Vector2Int(0, -1)  // dưới
    };

    private void Awake()
    {
        sourceIO = GetComponent<OreBuildingResourceIO>();
        gm = GridManager.Instance;
    }

    private void Update()
    {
        if (gm == null || sourceIO == null) return;

        // Drill không có đủ ore thì khỏi thử đẩy
        if (!sourceIO.HasResource(outputType, amountPerTick)) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        timer = outputInterval;
        TryOutputOnce();
    }

    private void TryOutputOnce()
    {
        if (!sourceIO.HasResource(outputType, amountPerTick)) return;
        if (gm == null) return;

        Vector2Int myCell = gm.WorldToCell(transform.position);

        // Duyệt 4 hướng theo thứ tự ưu tiên
        foreach (var dir in kDirs)
        {
            Vector2Int targetCell = myCell + dir;
            if (!gm.IsInBounds(targetCell)) continue;

            Vector3 targetCenter = gm.CellToWorldCenter(targetCell);

            // Tìm các collider ở tâm ô đó
            Collider2D[] hits = Physics2D.OverlapPointAll(targetCenter);
            if (hits == null || hits.Length == 0) continue;

            IResourceSink sink = FindAcceptingSink(hits);
            if (sink == null) continue;

            // Đã có nơi nhận → rút ore từ Drill và gửi sang
            ResourceStack stack = sourceIO.Take(outputType, amountPerTick);
            if (stack.IsEmpty) return;

            int accepted = sink.Accept(stack);

            // Nếu sau này amountPerTick > 1 và accepted < stack.amount
            // thì có thể hoàn trả phần dư về Drill ở đây.
            // Với amountPerTick = 1 thì không cần xử lý thêm.

            // Đã gửi được cho 1 hướng → dừng lại
            return;
        }
    }

    private IResourceSink FindAcceptingSink(Collider2D[] hits)
    {
        foreach (var h in hits)
        {
            if (h == null) continue;

            var candidate = h.GetComponentInParent<IResourceSink>();
            if (candidate == null) continue;

            // Hỏi thử xem chấp nhận không
            var dummy = new ResourceStack(outputType, amountPerTick);
            if (candidate.CanAccept(dummy))
            {
                return candidate;
            }
        }

        return null;
    }
}
