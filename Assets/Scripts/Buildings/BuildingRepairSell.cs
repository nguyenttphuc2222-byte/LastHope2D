using UnityEngine;

[RequireComponent(typeof(BuildingBase))]
public class BuildingRepairSell : MonoBehaviour
{
    /*
     * NOTE VỀ "GIÁ CƠ BẢN":
     *  - Script này dùng BuildingBase.buildCostOre như là **giá kinh tế cơ bản** (BasePrice)
     *    để tính:
     *      + Chi phí repair
     *      + Số ore hoàn lại khi bán
     *  - Với hầu hết building (Wall, Turret, Drill): buildCostOre vừa là giá xây trong
     *    BuildMode, vừa là BasePrice.
     *  - Với Core: player KHÔNG tự xây trong BuildMode, nhưng vẫn gán buildCostOre = 50
     *    để làm BasePrice cho repair (và bán nếu bạn cho phép).
     */

    [Header("Flags")]
    public bool canRepair = true;
    public bool canSell = true;

    [Header("Repair")]
    [Tooltip("Chi phí full repair = BasePrice * fullRepairCostFactor * %HP đã mất. BasePrice = buildCostOre của building.")]
    public float fullRepairCostFactor = 0.5f;   // 0.5 = mất hết máu thì sửa full tốn 50% BasePrice
    public int minRepairCost = 1;

    [Header("Sell")]
    [Tooltip("Số ore hoàn lại = BasePrice * sellRefundFactor. BasePrice = buildCostOre của building.")]
    public float sellRefundFactor = 0.5f;       // 0.5 = bán được 50% BasePrice
    public int minSellRefund = 0;

    private BuildingBase building;

    /// <summary>Giá kinh tế cơ bản, đang dùng cho repair / sell.</summary>
    private int BasePrice => (building == null) ? 0 : building.buildCostOre;

    public bool IsDamaged =>
        building != null &&
        building.currentHealth > 0 &&
        building.currentHealth < building.maxHealth;

    public int RepairCost
    {
        get
        {
            if (!canRepair || building == null) return 0;
            if (!IsDamaged) return 0;
            if (BasePrice <= 0) return 0;

            int missingHP = building.maxHealth - building.currentHealth;
            float damagePercent = missingHP / (float)building.maxHealth;

            int cost = Mathf.CeilToInt(BasePrice * fullRepairCostFactor * damagePercent);
            return Mathf.Max(minRepairCost, cost);
        }
    }

    public int SellRefund
    {
        get
        {
            if (!canSell || building == null) return 0;
            if (BasePrice <= 0) return 0;

            int refund = Mathf.RoundToInt(BasePrice * sellRefundFactor);
            return Mathf.Max(minSellRefund, refund);
        }
    }

    private void Awake()
    {
        building = GetComponent<BuildingBase>();
    }

    // --- REPAIR ---

    public bool TryRepairFull()
    {
        if (!canRepair || building == null) return false;
        if (!IsDamaged) return false;

        CoreBuilding core = CoreBuilding.Instance;
        if (core == null) return false;

        int cost = RepairCost;
        if (cost <= 0) return false;

        if (!core.TrySpendOre(cost))
        {
            if (BuildFeedbackUI.Instance != null)
                BuildFeedbackUI.Instance.ShowNotEnoughOre(Input.mousePosition);
            return false;
        }

        // Heal lên full
        building.Heal(building.maxHealth - building.currentHealth);
        return true;
    }

    // --- SELL ---

    public bool TrySell()
    {
        if (!canSell || building == null) return false;
        if (building.currentHealth <= 0) return false;

        int refund = SellRefund;
        CoreBuilding core = CoreBuilding.Instance;
        if (core != null && refund > 0)
        {
            core.AddOre(refund);
        }

        // Phá building thông qua hệ thống damage hiện tại
        building.TakeDamage(building.currentHealth);
        return true;
    }
}
