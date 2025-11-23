using UnityEngine;

public class OreBuilding : BuildingBase
{
    [Header("Mining base stats (Level 1)")]
    [Tooltip("Số ore tạo mỗi chu kỳ ở level 1.")]
    public int baseOrePerCycle = 1;

    [Tooltip("Thời gian một chu kỳ đào ở level 1 (giây).")]
    public float baseCycleTime = 2f;

    [Header("Upgrade scaling")]
    [Tooltip("Mỗi level cộng thêm bao nhiêu ore / chu kỳ.")]
    public int orePerLevelBonus = 1;

    [Tooltip("Mỗi level đào nhanh hơn bao nhiêu. Ví dụ 0.25 = nhanh hơn 25% mỗi level.")]
    public float cycleSpeedBonusPerLevel = 0.25f;

    [Header("Storage")]
    [Tooltip("Sức chứa cơ bản của kho drill ở level 1.")]
    public int baseStorage = 20;  // sức chứa tối đa

    [Tooltip("Mỗi level cộng thêm bao nhiêu sức chứa.")]
    public int storagePerLevelBonus = 10;

    [Tooltip("Số ore đang được lưu trong drill này.")]
    public int storedOre; // lượng ore đang chứa

    private float timer;
    private BuildingUpgrade upgrade;   // dùng level từ hệ upgrade hiện tại

    protected override void Awake()
    {
        // Gọi Awake() của BuildingBase để currentHealth = maxHealth, v.v.
        base.Awake();
        // Nếu BuildingBase có Awake(), và bạn override, nhớ gọi base.Awake() ở đây.
        upgrade = GetComponent<BuildingUpgrade>();
    }

    private void Update()
    {
        float cycleTime = GetCurrentCycleTime();
        if (cycleTime <= 0f) return;

        int maxStorage = GetCurrentMaxStorage();
        if (storedOre >= maxStorage)
        {
            // Kho đã đầy -> không đào thêm
            return;
        }

        timer += Time.deltaTime;
        if (timer >= cycleTime)
        {
            timer -= cycleTime;

            int amount = GetCurrentOrePerCycle();
            if (amount <= 0) return;

            int freeSpace = maxStorage - storedOre;
            int toStore = Mathf.Min(amount, freeSpace);

            if (toStore > 0)
            {
                storedOre += toStore;
                // Debug.Log($"[OreBuilding] +{toStore} ore (stored = {storedOre}/{maxStorage})");
            }
        }
    }

    // ------- Helper tính level / stat hiện tại -------

    private int GetCurrentLevel()
    {
        if (upgrade == null) return 1;
        return Mathf.Max(1, upgrade.currentLevel);
    }

    private int GetCurrentOrePerCycle()
    {
        int level = GetCurrentLevel();
        return Mathf.Max(0, baseOrePerCycle + orePerLevelBonus * (level - 1));
    }

    private float GetCurrentCycleTime()
    {
        int level = GetCurrentLevel();
        float speedFactor = 1f + cycleSpeedBonusPerLevel * (level - 1);
        if (speedFactor < 0.05f) speedFactor = 0.05f;
        return baseCycleTime / speedFactor;
    }

    public int GetCurrentMaxStorage()
    {
        int level = GetCurrentLevel();
        return Mathf.Max(0, baseStorage + storagePerLevelBonus * (level - 1));
    }

    // ------- API cho tương lai: Conveyor / Player lấy ore -------

    /// <summary>Lấy ra tối đa 'amount' ore khỏi drill, trả về số thực tế lấy được.</summary>
    public int TakeOre(int amount)
    {
        if (amount <= 0 || storedOre <= 0) return 0;
        int taken = Mathf.Min(amount, storedOre);
        storedOre -= taken;
        return taken;
    }

    /// <summary>Lấy hết ore đang có trong drill, trả về số đã lấy.</summary>
    public int TakeAllOre()
    {
        int taken = storedOre;
        storedOre = 0;
        return taken;
    }
}
