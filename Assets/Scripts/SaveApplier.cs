using UnityEngine;

public class SaveApplier : MonoBehaviour
{
    private void Start()
    {
        // Chỉ apply nếu có flag yêu cầu load
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

        // 2) Xoá toàn bộ building runtime (trừ Core & EnemyCore)
        BuildingBase[] existing =
            Object.FindObjectsByType<BuildingBase>(FindObjectsSortMode.None);

        foreach (var b in existing)
        {
            if (b == null) continue;

            // GIỮ lại cả Core và EnemyCore (được đặt sẵn trong scene)
            if (b is CoreBuilding || b is EnemyCoreBuilding)
                continue;

            if (GridManager.Instance != null)
                GridManager.Instance.ClearBuilding(b, b.AnchorCell);

            Object.Destroy(b.gameObject);
        }

        // 3) Core
        CoreBuilding core = CoreBuilding.Instance;
        if (core != null && data.core != null)
        {
            core.transform.position = new Vector3(
                data.core.posX, data.core.posY, data.core.posZ);

            core.maxHealth = data.core.maxHealth;
            core.currentHealth = Mathf.Clamp(
                data.core.currentHealth, 0, core.maxHealth);

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
                if (bs == null || string.IsNullOrEmpty(bs.prefabName))
                    continue;

                // EnemyCore là công trình đặt sẵn trong scene → không spawn lại
                if (bs.prefabName == "EnemyCore" || bs.prefabName.Contains("EnemyCore"))
                    continue;

                BuildingBase prefab = FindBuildingPrefabByName(placer, bs.prefabName);
                if (prefab == null)
                {
                    Debug.LogWarning(
                        $"SaveApplier: Không tìm thấy prefab building '{bs.prefabName}'");
                    continue;
                }

                BuildingBase b = Object.Instantiate(prefab);

                // Đặt vào grid đúng ô anchor
                Vector2Int anchor = new Vector2Int(bs.anchorX, bs.anchorY);
                if (GridManager.Instance != null)
                {
                    GridManager.Instance.PlaceBuilding(b, anchor);
                }
                else
                {
                    // fallback (hiếm khi dùng)
                    b.transform.position = new Vector3(anchor.x, anchor.y, 0f);
                }

                // HP
                b.currentHealth = Mathf.Clamp(bs.currentHealth, 0, b.maxHealth);

                // Upgrade level
                var up = b.GetComponent<BuildingUpgrade>();
                if (up != null && bs.upgradeLevel > 1)
                {
                    up.currentLevel = bs.upgradeLevel;
                    // Nếu bạn có hàm up.ApplyStats(); thì gọi ở đây
                }

                // Drill – stored ore
                var ore = b.GetComponent<OreBuilding>();
                if (ore != null)
                {
                    ore.storedOre = bs.storedOre;
                }

                // Conveyor – direction + xoay sprite
                var conv = b.GetComponent<ConveyorBuilding>();
                if (conv != null)
                {
                    conv.direction = (ConveyorDirection)bs.conveyorDir;
                    ApplyConveyorRotation(conv.transform, conv.direction);
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

    // Tìm prefab trong mảng BuildOption theo tên
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

    // Hàm xoay Conveyor giống logic trong BuildingPlacer
    private void ApplyConveyorRotation(Transform t, ConveyorDirection dir)
    {
        float angle = 0f;
        switch (dir)
        {
            case ConveyorDirection.Right: angle = 0f; break;
            case ConveyorDirection.Up: angle = 90f; break;
            case ConveyorDirection.Left: angle = 180f; break;
            case ConveyorDirection.Down: angle = 270f; break;
        }
        t.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
