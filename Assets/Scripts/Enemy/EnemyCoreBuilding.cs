using UnityEngine;

public class EnemyCoreBuilding : BuildingBase
{
    public static EnemyCoreBuilding Instance { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        if (Instance != null && Instance != this)
        {
            Debug.LogError("Có nhiều EnemyCoreBuilding trong scene!");
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // Đăng ký EnemyCore với GridManager giống như các building khác
        if (GridManager.Instance != null)
        {
            Vector2Int anchorCell = GridManager.Instance.WorldToCell(transform.position);
            GridManager.Instance.PlaceBuilding(this, anchorCell);
        }
        else
        {
            Debug.LogWarning("EnemyCoreBuilding: Không tìm thấy GridManager.");
        }
    }

    protected override void OnDestroyed()
    {
        // Báo thắng game
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.OnEnemyCoreDestroyed();
        }

        base.OnDestroyed();
    }
}
