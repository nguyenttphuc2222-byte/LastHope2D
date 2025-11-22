using UnityEngine;

[System.Serializable]
public class WallLevelStats
{
    [Tooltip("Ore cần để nâng từ level này lên level kế tiếp.")]
    public int upgradeCostOre = 10;

    [Tooltip("Max HP ở level này.")]
    public int maxHealth = 200;
}

[RequireComponent(typeof(BuildingBase))]
public class WallUpgrade : BuildingUpgradeBase
{
    [Header("Wall Levels")]
    public WallLevelStats[] levels;

    private BuildingBase buildingBase;

    public override int MaxLevel => (levels != null) ? levels.Length : 0;

    private void Awake()
    {
        buildingBase = GetComponent<BuildingBase>();

        if (levels == null || levels.Length == 0)
        {
            Debug.LogWarning($"{name}: WallUpgrade chưa cấu hình levels.");
        }

        // clamp level và apply stats ban đầu
        currentLevel = Mathf.Clamp(currentLevel, 1, MaxLevel > 0 ? MaxLevel : 1);
        ApplyLevelStats(currentLevel);
    }

    public override int GetUpgradeCost()
    {
        if (IsMaxLevel || levels == null || levels.Length == 0)
            return -1;

        // cost để nâng từ level hiện tại -> level tiếp theo
        // (tuỳ cách hiểu, ở đây mình cho mỗi LevelStats mô tả chính level đó)
        int nextIndex = Mathf.Clamp(currentLevel, 1, MaxLevel) - 1;
        return levels[nextIndex].upgradeCostOre;
    }

    protected override void ApplyLevelStats(int level)
    {
        if (levels == null || levels.Length == 0) return;

        int idx = Mathf.Clamp(level, 1, MaxLevel) - 1;
        WallLevelStats s = levels[idx];

        if (buildingBase != null)
        {
            buildingBase.maxHealth = s.maxHealth;
            buildingBase.currentHealth = s.maxHealth;   // cho tự hồi full khi nâng cấp
        }
    }
}
