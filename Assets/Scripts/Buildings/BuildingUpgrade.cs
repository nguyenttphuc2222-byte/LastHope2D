using UnityEngine;

[RequireComponent(typeof(BuildingBase))]
public class BuildingUpgrade : MonoBehaviour
{
    [Header("Level")]
    public int currentLevel = 1;
    public int maxLevel = 3;

    // Chi phí ore cho từng lần nâng cấp:
    // index 0 = từ lv1 -> lv2
    // index 1 = từ lv2 -> lv3 ...
    [Header("Ore cost per level-up")]
    public int[] oreCostPerLevel;

    [Header("Bonus stats per level-up")]
    public int[] extraMaxHealthPerLevel;       // cộng thêm máu
    public int[] extraBulletDamagePerLevel;    // cộng thêm damage (nếu là Turret)

    private BuildingBase building;
    private TurretBuilding turret;

    public bool CanUpgrade => currentLevel < maxLevel;

    public int NextLevel => Mathf.Min(currentLevel + 1, maxLevel);

    public int UpgradeCost
    {
        get
        {
            int index = currentLevel - 1; // lv1->2 dùng index 0
            if (oreCostPerLevel == null ||
                index < 0 || index >= oreCostPerLevel.Length)
                return 0;
            return oreCostPerLevel[index];
        }
    }

    private void Awake()
    {
        building = GetComponent<BuildingBase>();
        turret = GetComponent<TurretBuilding>();
    }

    public bool TryUpgrade()
    {
        if (!CanUpgrade) return false;

        CoreBuilding core = CoreBuilding.Instance;
        if (core == null) return false;

        int cost = UpgradeCost;

        // check ore
        if (cost > 0 && !core.TrySpendOre(cost))
        {
            // dùng lại feedback cũ
            if (BuildFeedbackUI.Instance != null)
                BuildFeedbackUI.Instance.ShowNotEnoughOre(Input.mousePosition);
            return false;
        }

        // tăng level
        int oldLevel = currentLevel;
        currentLevel++;

        int stepIndex = oldLevel - 1; // 1->2 = 0, 2->3 = 1,...

        // tăng máu
        if (building != null &&
            extraMaxHealthPerLevel != null &&
            stepIndex >= 0 && stepIndex < extraMaxHealthPerLevel.Length)
        {
            int addHp = extraMaxHealthPerLevel[stepIndex];
            building.maxHealth += addHp;
            building.currentHealth += addHp;
            if (building.currentHealth > building.maxHealth)
                building.currentHealth = building.maxHealth;

            var hb = building.GetComponentInChildren<HealthBar>();
            if (hb != null)
                hb.SetValue((float)building.currentHealth / building.maxHealth);
        }

        // nếu là turret thì buff thêm damage
        if (turret != null &&
            extraBulletDamagePerLevel != null &&
            stepIndex >= 0 && stepIndex < extraBulletDamagePerLevel.Length)
        {
            turret.bulletDamage += extraBulletDamagePerLevel[stepIndex];
        }

        return true;
    }
}
