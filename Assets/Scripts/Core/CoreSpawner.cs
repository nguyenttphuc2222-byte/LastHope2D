using UnityEngine;

public class CoreSpawner : MonoBehaviour
{
    [Header("Core Setup")]
    public CoreBuilding corePrefab;

    [Tooltip("Cell anchor (góc dưới-trái) của Core. Nếu để (-1, -1) thì auto đặt giữa map.")]
    public Vector2Int coreAnchorCell = new Vector2Int(-1, -1);

    private void Start()
    {
        if (GridManager.Instance == null)
        {
            Debug.LogError("CoreSpawner: Không tìm thấy GridManager trong scene.");
            return;
        }

        if (corePrefab == null)
        {
            Debug.LogError("CoreSpawner: Chưa gán corePrefab.");
            return;
        }

        // Nếu chưa set tay, thì auto đặt gần giữa map
        Vector2Int anchor = coreAnchorCell;
        if (anchor.x < 0 || anchor.y < 0)
        {
            int x = GridManager.Instance.width / 2 - corePrefab.size.x / 2;
            int y = GridManager.Instance.height / 2 - corePrefab.size.y / 2;
            anchor = new Vector2Int(x, y);
        }

        // Kiểm tra xem ô này có hợp lệ không (không trùng tường, không ra ngoài map)
        if (!GridManager.Instance.CanPlaceBuilding(corePrefab, anchor))
        {
            Debug.LogError($"CoreSpawner: Không thể đặt Core tại cell {anchor}. Kiểm tra map block / kích thước.");
            return;
        }

        // Spawn Core + đăng ký với GridManager
        CoreBuilding coreInstance = Instantiate(corePrefab);
        GridManager.Instance.PlaceBuilding(coreInstance, anchor);
    }
}
