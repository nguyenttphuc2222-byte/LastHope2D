using UnityEngine;

public class SaveApplier : MonoBehaviour
{
    private void Start()
    {
        if (!SaveSystem.LoadOnNextScene)
            return;

        SaveSystem.ClearLoadRequest();

        SaveData data = SaveSystem.LoadFromDisk();
        if (data == null)
            return;

        ApplySave(data);
    }

    private void ApplySave(SaveData data)
    {
        // 1) Time
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.ElapsedTime = data.elapsedTime;
        }

        // 2) Xoá toàn bộ building runtime (trừ Core)
        foreach (var b in Object.FindObjectsByType<BuildingBase>(FindObjectsSortMode.None))
        {
            if (b == null) continue;
            if (b is CoreBuilding) continue;

            Destroy(b.gameObject);
        }

        // 3) Core
        CoreBuilding core = CoreBuilding.Instance;
        if (core != null && data.core != null)
        {
            core.transform.position = new Vector3(
                data.core.posX, data.core.posY, data.core.posZ);
            core.maxHealth = data.core.maxHealth;
            core.currentHealth = Mathf.Clamp(data.core.currentHealth, 0, core.maxHealth);
            core.oreAmount = data.core.oreAmount;
        }

        // 4) Player
        PlayerClickMover player = Object.FindFirstObjectByType<PlayerClickMover>();
        if (player != null && data.player != null)
        {
            player.transform.position = new Vector3(
                data.player.posX, data.player.posY, data.player.posZ);
        }

        // 5) Rebuild buildings từ Save
        GridManager gm = GridManager.Instance;
        BuildingPlacer placer = Object.FindFirstObjectByType<BuildingPlacer>();

        if (gm != null && placer != null)
        {
            foreach (var bs in data.buildings)
            {
                BuildingBase prefab = FindBuildingPrefabByName(placer, bs.prefabName);
                if (prefab == null)
                {
                    Debug.LogWarning($"SaveApplier: Không tìm thấy prefab building '{bs.prefabName}'");
                    continue;
                }

                BuildingBase b = Instantiate(prefab);
                Vector2Int cell = new Vector2Int(bs.anchorX, bs.anchorY);
                gm.PlaceBuilding(b, cell);

                b.currentHealth = Mathf.Clamp(bs.currentHealth, 0, b.maxHealth);

                var up = b.GetComponent<BuildingUpgrade>();
                if (up != null && bs.upgradeLevel > 1)
                {
                    up.currentLevel = bs.upgradeLevel;
                    // nếu bạn có hàm ApplyStats() thì gọi ở đây
                }

                var ore = b.GetComponent<OreBuilding>();
                if (ore != null)
                {
                    ore.storedOre = bs.storedOre;
                }

                var conv = b.GetComponent<ConveyorBuilding>();
                if (conv != null)
                {
                    conv.direction = (ConveyorDirection)bs.conveyorDir;
                    // nếu có hàm ApplyRotation() thì gọi thêm
                }
            }
        }

        // 6) Wave
        WaveManager wm = Object.FindFirstObjectByType<WaveManager>();
        if (wm != null)
        {
            wm.LoadFromSave(data.currentWave, data.isWaveActive, data.waveTimer);
        }

        // 7) Pause + mở Pause menu
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.PauseAndShowMenu();
        }
    }

    private BuildingBase FindBuildingPrefabByName(BuildingPlacer placer, string prefabName)
    {
        if (placer.options == null) return null;

        foreach (var opt in placer.options)
        {
            if (opt == null || opt.prefab == null) continue;
            if (opt.prefab.name == prefabName)
                return opt.prefab;
        }
        return null;
    }
}
