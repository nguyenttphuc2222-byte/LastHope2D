using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveSystem
{
    private static string FilePath =>
        Path.Combine(Application.persistentDataPath, "savegame.json");

    /// <summary>
    /// Cờ báo: khi load MapTest thì apply save.
    /// Được set bằng RequestLoadOnNextScene() và clear bằng ClearLoadRequest().
    /// </summary>
    public static bool LoadOnNextScene { get; private set; }

    public static bool HasSaveFile()
    {
        return File.Exists(FilePath);
    }

    public static void RequestLoadOnNextScene()
    {
        LoadOnNextScene = true;
    }

    public static void ClearLoadRequest()
    {
        LoadOnNextScene = false;
    }

    /// <summary>
    /// Xoá file save trên ổ đĩa + clear luôn cờ LoadOnNextScene.
    /// Dùng cho nút Restart (chơi lại từ đầu, không dùng save cũ).
    /// </summary>
    public static void DeleteSave()
    {
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
            Debug.Log($"SaveSystem: Deleted save file at {FilePath}");
        }

        // Đảm bảo không còn yêu cầu load save ở lần vào scene kế tiếp
        LoadOnNextScene = false;
    }

    // ================= SAVE =================

    public static void SaveGame()
    {
        SaveData data = new SaveData();
        data.sceneName = SceneManager.GetActiveScene().name;

        // 1) Time
        if (GameFlowManager.Instance != null)
        {
            data.elapsedTime = GameFlowManager.Instance.ElapsedTime;
        }

        // 2) Wave
        WaveManager wm = Object.FindFirstObjectByType<WaveManager>();
        if (wm != null)
        {
            data.currentWave = wm.CurrentWave;
            data.isWaveActive = wm.IsWaveActive;
            data.waveTimer = wm.CurrentTimer;
        }

        // 3) Core
        CoreBuilding core = CoreBuilding.Instance;
        if (core != null)
        {
            Vector3 p = core.transform.position;
            data.core = new CoreSaveData
            {
                posX = p.x,
                posY = p.y,
                posZ = p.z,
                currentHealth = core.currentHealth,
                maxHealth = core.maxHealth,
                oreAmount = core.oreAmount
            };
        }

        // 4) Player
        PlayerClickMover player = Object.FindFirstObjectByType<PlayerClickMover>();
        if (player != null)
        {
            Vector3 p = player.transform.position;
            data.player = new PlayerSaveData
            {
                posX = p.x,
                posY = p.y,
                posZ = p.z
            };
        }

        // 5) Buildings
        BuildingBase[] allBuildings =
            Object.FindObjectsByType<BuildingBase>(FindObjectsSortMode.None);

        foreach (var b in allBuildings)
        {
            if (b == null) continue;

            // Core và EnemyCore KHÔNG lưu ở danh sách buildings
            if (b is CoreBuilding || b is EnemyCoreBuilding)
                continue;

            BuildingSaveData bs = new BuildingSaveData();
            bs.prefabName = b.gameObject.name.Replace("(Clone)", "");
            bs.currentHealth = b.currentHealth;
            bs.anchorX = b.AnchorCell.x;
            bs.anchorY = b.AnchorCell.y;

            var up = b.GetComponent<BuildingUpgrade>();
            if (up != null) bs.upgradeLevel = up.currentLevel;

            var ore = b.GetComponent<OreBuilding>();
            if (ore != null) bs.storedOre = ore.storedOre;

            var conv = b.GetComponent<ConveyorBuilding>();
            if (conv != null) bs.conveyorDir = (int)conv.direction;

            data.buildings.Add(bs);
        }


        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(FilePath, json);
        Debug.Log($"SaveSystem: Saved game to {FilePath}");
    }

    // ================= LOAD RAW JSON =================

    public static SaveData LoadFromDisk()
    {
        if (!File.Exists(FilePath))
        {
            Debug.LogWarning("SaveSystem: No save file found.");
            return null;
        }

        string json = File.ReadAllText(FilePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        return data;
    }
}
