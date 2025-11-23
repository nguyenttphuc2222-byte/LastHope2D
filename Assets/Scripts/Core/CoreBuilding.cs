using UnityEngine;

public class CoreBuilding : BuildingBase
{
    public static CoreBuilding Instance { get; private set; }


    [Header("Resources")]
    [Tooltip("Tổng số quặng mà Core đang giữ.")]
    public int oreAmount; // đây là kho ore thật

    protected override void Awake()
    {
        base.Awake(); // set currentHealth = maxHealth

        if (Instance != null && Instance != this)
        {
            Debug.LogError("Có nhiều hơn một CoreBuilding trong scene!");
        }

        Instance = this;
    }

    public override void OnPlaced(Vector2Int anchorCell)
    {
        base.OnPlaced(anchorCell);
        Debug.Log($"Core placed at cell {anchorCell}");
    }

    public void AddOre(int amount)
    {
        oreAmount += amount;
        if (oreAmount < 0) oreAmount = 0;
        Debug.Log($"Core nhận thêm {amount} ore, tổng = {oreAmount}");
    }

    public bool TrySpendOre(int amount)
    {
        if (amount <= 0) return true;
        if (oreAmount < amount) return false;
        oreAmount -= amount;
        return true;
    }

    protected override void OnDestroyed()
    {
        // báo cho GameFlowManager trước khi huỷ
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.OnCoreDestroyed();
        }

        base.OnDestroyed();
    }

}
