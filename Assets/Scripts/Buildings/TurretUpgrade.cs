using UnityEngine;

[System.Serializable]
public class TurretLevelStats
{
    [Tooltip("Ore cần để nâng từ level này lên level kế tiếp.")]
    public int upgradeCostOre = 30;

    [Header("Turret Stats")]
    public int maxHealth = 150;
    public float range = 4f;
    public float fireRate = 1f;     // viên/giây
    public int bulletDamage = 15;
}

[RequireComponent(typeof(TurretBuilding))]
public class TurretUpgrade : BuildingUpgradeBase
{
    [Header("Turret Levels")]
    public TurretLevelStats[] levels;

    private TurretBuilding turret;
    private BuildingBase buildingBase;

    public override int MaxLevel => (levels != null) ? levels.Length : 0;

    private void Awake()
    {
        turret = GetComponent<TurretBuilding>();
        buildingBase = GetComponent<BuildingBase>();

        if (levels == null || levels.Length == 0)
        {
            Debug.LogWarning($"{name}: TurretUpgrade chưa cấu hình levels.");
        }

        currentLevel = Mathf.Clamp(currentLevel, 1, MaxLevel > 0 ? MaxLevel : 1);
        ApplyLevelStats(currentLevel);
    }

    public override int GetUpgradeCost()
    {
        if (IsMaxLevel || levels == null || levels.Length == 0)
            return -1;

        int nextIndex = Mathf.Clamp(currentLevel, 1, MaxLevel) - 1;
        return levels[nextIndex].upgradeCostOre;
    }

    protected override void ApplyLevelStats(int level)
    {
        if (levels == null || levels.Length == 0) return;

        int idx = Mathf.Clamp(level, 1, MaxLevel) - 1;
        TurretLevelStats s = levels[idx];

        if (buildingBase != null)
        {
            buildingBase.maxHealth = s.maxHealth;
            buildingBase.currentHealth = s.maxHealth;
        }

        if (turret != null)
        {
            turret.range = s.range;
            turret.fireRate = s.fireRate;
            turret.bulletDamage = s.bulletDamage;
        }
    }
}
