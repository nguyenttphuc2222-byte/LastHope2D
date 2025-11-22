using UnityEngine;

public abstract class BuildingUpgradeBase : MonoBehaviour
{
    [Header("Upgrade")]
    [Tooltip("Cấp hiện tại của building (bắt đầu từ 1).")]
    public int currentLevel = 1;

    /// <summary>Tổng số level (ví dụ 3).</summary>
    public abstract int MaxLevel { get; }

    /// <summary>Đã max level chưa.</summary>
    public bool IsMaxLevel => currentLevel >= MaxLevel;

    /// <summary>Cost ore để lên level tiếp theo. Nếu đã max: trả -1.</summary>
    public abstract int GetUpgradeCost();

    /// <summary>Gọi khi level thay đổi để áp stat mới.</summary>
    protected abstract void ApplyLevelStats(int level);

    /// <summary>Thử nâng cấp (đã trừ ore Core nếu đủ). Trả true nếu thành công.</summary>
    public bool TryUpgrade()
    {
        if (IsMaxLevel)
        {
            Debug.Log($"{name}: Đã max level, không thể upgrade.");
            return false;
        }

        int cost = GetUpgradeCost();
        CoreBuilding core = CoreBuilding.Instance;

        if (core != null && cost > 0)
        {
            if (!core.TrySpendOre(cost))
            {
                // Tái sử dụng feedback "Not enough ore" nếu có
                if (BuildFeedbackUI.Instance != null)
                {
                    BuildFeedbackUI.Instance.ShowNotEnoughOre(Input.mousePosition);
                }

                Debug.Log($"{name}: Không đủ ore để upgrade. Cần {cost}, Core có {core.oreAmount}");
                return false;
            }
        }

        currentLevel++;
        currentLevel = Mathf.Clamp(currentLevel, 1, MaxLevel);

        ApplyLevelStats(currentLevel);

        Debug.Log($"{name}: Upgrade lên level {currentLevel} (tốn {cost} ore).");
        return true;
    }
}
