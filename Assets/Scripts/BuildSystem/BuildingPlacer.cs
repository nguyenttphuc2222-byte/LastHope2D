using UnityEngine;

[System.Serializable]
public class BuildOption
{
    [Tooltip("Tên hiển thị (để debug trong log).")]
    public string id;

    [Tooltip("Phím hotkey để chọn loại building này (ví dụ KeyCode.Alpha1).")]
    public KeyCode hotkey = KeyCode.None;

    [Tooltip("Prefab của building (phải là prefab chứa BuildingBase).")]
    public BuildingBase prefab;
}

public class BuildingPlacer : MonoBehaviour
{
    [Header("Build options")]
    public BuildOption[] options;

    [Tooltip("Chỉ số loại building đang được chọn trong mảng options.")]
    public int selectedIndex = 0;

    [Header("Build Mode")]
    [Tooltip("Chỉ dùng để xem trạng thái hiện tại (B để bật/tắt).")]
    public bool buildModeEnabled = false;

    [Header("Preview")]
    [Tooltip("Độ lệch Z để preview không bị trùng với sprite thật.")]
    public float previewZOffset = -0.1f;

    public Color validColor = new Color(0f, 1f, 0f, 0.35f);
    public Color invalidColor = new Color(1f, 0f, 0f, 0.35f);

    [Header("Khác")]
    public LayerMask placementBlockedMask;

    private Camera mainCam;

    // preview runtime
    private BuildingBase previewInstance;
    private SpriteRenderer[] previewRenderers;

    private void Start()
    {
        mainCam = Camera.main;

        if (options == null || options.Length == 0)
        {
            Debug.LogWarning("BuildingPlacer: chưa có build option nào được cấu hình.");
        }
        else
        {
            selectedIndex = Mathf.Clamp(selectedIndex, 0, options.Length - 1);
        }

        CreatePreviewForCurrentOption();

        // Vừa vào game: build mode OFF, nên ẩn preview
        SetPreviewVisible(false);
    }

    private void Update()
    {

        // Toggle build mode bằng phím B
        if (Input.GetKeyDown(KeyCode.B))
        {
            buildModeEnabled = !buildModeEnabled;
            SetPreviewVisible(buildModeEnabled);
            Debug.Log(buildModeEnabled ? "Build mode: ON" : "Build mode: OFF");
        }

        // Nếu chưa bật build mode thì bỏ qua toàn bộ logic xây
        if (!buildModeEnabled)
        {
            return;
        }

        HandleHotkeys();
        UpdatePreview();

        // Chuột phải để đặt building chỉ khi build mode đang bật
        if (Input.GetMouseButtonDown(1))
        {
            if (GridManager.Instance == null || mainCam == null) return;

            Vector3 worldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0f;

            Vector2Int cell = GridManager.Instance.WorldToCell(worldPos);
            TryPlaceBuildingAtCell(cell);
        }
    }

    private void HandleHotkeys()
    {
        if (options == null || options.Length == 0) return;

        // --- BẮT PHÍM THEO KÝ TỰ (robust) ---
        string s = Input.inputString;    // các ký tự gõ trong frame này
        if (!string.IsNullOrEmpty(s))
        {
            // nếu trong chuỗi có '1' -> chọn option 0 (Wall)
            if (s.Contains("1") && options.Length > 0)
            {
                SelectBuildOption(0);
                return;
            }

            // nếu có '2' -> chọn option 1 (Turret)
            if (s.Contains("2") && options.Length > 1)
            {
                SelectBuildOption(1);
                return;
            }
        }

        // --- FALLBACK: vẫn cho phép gán hotkey khác trong Inspector (Q, E, 3,4,5...) ---

        for (int i = 0; i < options.Length; i++)
        {
            BuildOption opt = options[i];
            if (opt == null) continue;

            if (opt.hotkey != KeyCode.None && Input.GetKeyDown(opt.hotkey))
            {
                SelectBuildOption(i);
                return;
            }
        }
    }


    private void SelectBuildOption(int index)
    {
        index = Mathf.Clamp(index, 0, options.Length - 1);
        selectedIndex = index;

        BuildOption opt = GetCurrentOption();
        if (opt != null)
        {
            Debug.Log($"[BuildingPlacer] Chọn loại building: {opt.id} (index {index})");
        }

        CreatePreviewForCurrentOption();
        SetPreviewVisible(buildModeEnabled);
    }



    private BuildOption GetCurrentOption()
    {
        if (options == null || options.Length == 0) return null;
        if (selectedIndex < 0 || selectedIndex >= options.Length) return null;
        return options[selectedIndex];
    }

    private void CreatePreviewForCurrentOption()
    {
        // Xoá preview cũ
        if (previewInstance != null)
        {
            Destroy(previewInstance.gameObject);
            previewInstance = null;
            previewRenderers = null;
        }

        BuildOption opt = GetCurrentOption();
        if (opt == null || opt.prefab == null) return;

        // Tạo object preview từ prefab
        previewInstance = Instantiate(opt.prefab);
        previewInstance.gameObject.name = opt.prefab.name + "_Preview";

        // Vô hiệu hoá collider / physics để không va chạm
        Collider2D[] cols = previewInstance.GetComponentsInChildren<Collider2D>();
        foreach (var c in cols)
        {
            c.enabled = false;
        }

        Rigidbody2D[] bodies = previewInstance.GetComponentsInChildren<Rigidbody2D>();
        foreach (var rb in bodies)
        {
            rb.simulated = false;
        }

        previewRenderers = previewInstance.GetComponentsInChildren<SpriteRenderer>();
        SetPreviewColor(invalidColor);
    }

    private void UpdatePreview()
    {
        if (previewInstance == null || GridManager.Instance == null || mainCam == null)
            return;

        Vector3 worldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0f;
        Vector2Int cell = GridManager.Instance.WorldToCell(worldPos);

        BuildOption opt = GetCurrentOption();
        if (opt == null || opt.prefab == null)
        {
            SetPreviewVisible(false);
            return;
        }

        bool canPlace = GridManager.Instance.CanPlaceBuilding(opt.prefab, cell);

        Vector3 centerWorld = GetBuildingWorldCenter(opt.prefab, cell);
        previewInstance.transform.position = new Vector3(
            centerWorld.x,
            centerWorld.y,
            centerWorld.z + previewZOffset
        );

        SetPreviewColor(canPlace ? validColor : invalidColor);
    }

    private Vector3 GetBuildingWorldCenter(BuildingBase prefabBase, Vector2Int anchorCell)
    {
        GridManager gm = GridManager.Instance;
        Vector2Int size = prefabBase.size;

        float x = gm.origin.x + (anchorCell.x + size.x * 0.5f) * gm.cellSize;
        float y = gm.origin.y + (anchorCell.y + size.y * 0.5f) * gm.cellSize;

        return new Vector3(x, y, 0f);
    }

    private void SetPreviewColor(Color c)
    {
        if (previewRenderers == null) return;

        for (int i = 0; i < previewRenderers.Length; i++)
        {
            SpriteRenderer sr = previewRenderers[i];
            if (sr == null) continue;

            sr.color = c;
        }
    }

    private void SetPreviewVisible(bool visible)
    {
        if (previewInstance != null)
        {
            previewInstance.gameObject.SetActive(visible);
        }
    }

    private void TryPlaceBuildingAtCell(Vector2Int cell)
    {
        BuildOption opt = GetCurrentOption();
        if (opt == null || opt.prefab == null)
        {
            Debug.LogWarning("BuildingPlacer: chưa chọn prefab để đặt.");
            return;
        }

        BuildingBase prefabBase = opt.prefab;

        // 1) Check vị trí hợp lệ theo GridManager
        if (!GridManager.Instance.CanPlaceBuilding(prefabBase, cell))
        {
            Debug.Log("Không thể đặt " + opt.id + " tại cell " + cell);
            return;
        }

        // 2) Check tài nguyên từ Core
        CoreBuilding core = CoreBuilding.Instance;
        int cost = prefabBase.buildCostOre;

        if (core != null && cost > 0)
        {
            if (!core.TrySpendOre(cost))
            {
                // GỌI UI báo "Not enough ore" ngay tại vị trí chuột
                if (BuildFeedbackUI.Instance != null)
                {
                    BuildFeedbackUI.Instance.ShowNotEnoughOre(Input.mousePosition);
                }

                Debug.Log(
                    $"Không đủ ore để xây {opt.id}. Cần {cost}, Core chỉ có {core.oreAmount}"
                );
                return;
            }
        }


        // 3) Thực sự đặt building
        BuildingBase b = Instantiate(prefabBase);
        GridManager.Instance.PlaceBuilding(b, cell);

        // Pop effect khi đặt thành công
        var buildEffect = b.GetComponent<BuildEffect>();
        if (buildEffect != null)
        {
            buildEffect.PlayPop();
        }

        Debug.Log(
            $"Đặt {opt.id} tại cell {cell}, tốn {cost} ore. Core còn {core?.oreAmount}"
        );

    }
}
