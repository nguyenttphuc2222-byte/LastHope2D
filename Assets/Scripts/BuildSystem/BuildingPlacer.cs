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

    [Tooltip("Giữ chuột phải để kéo xây hàng loạt? (nên bật cho Wall, Conveyor).")]
    public bool allowDragBuild = false;
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

    public Color validColor = new Color(0f, 1f, 0f, 1f);
    public Color invalidColor = new Color(1f, 0f, 0f, 1f);

    [Header("Khác")]
    public LayerMask placementBlockedMask;

    private Camera mainCam;

    // preview runtime
    private BuildingBase previewInstance;
    private SpriteRenderer[] previewRenderers;

    // ---------- Conveyor rotation ----------
    private ConveyorDirection previewConveyorDir = ConveyorDirection.Right;
    private static ConveyorDirection lastConveyorDir = ConveyorDirection.Right;

    // ---------- Drag-build ----------
    private bool isDragBuilding = false;
    private Vector2Int lastPlacedCell;

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
        SetPreviewVisible(false); // vừa vào game: OFF
    }

    private void Update()
    {
        // Toggle build mode bằng B
        if (Input.GetKeyDown(KeyCode.B))
        {
            buildModeEnabled = !buildModeEnabled;
            SetPreviewVisible(buildModeEnabled);
            Debug.Log(buildModeEnabled ? "Build mode: ON" : "Build mode: OFF");
        }

        if (!buildModeEnabled)
        {
            // nếu tắt build mode thì đảm bảo trạng thái drag cũng tắt
            isDragBuilding = false;
            return;
        }

        HandleHotkeys();
        HandleConveyorRotateKey();
        UpdatePreview();

        HandleBuildMouseInput();
    }

    // =========================================
    //      INPUT CHUỘT PHẢI (BUILD / DRAG)
    // =========================================

    private void HandleBuildMouseInput()
    {
        if (GridManager.Instance == null || mainCam == null) return;

        // Bắt đầu đặt khi nhấn xuống chuột phải
        if (Input.GetMouseButtonDown(1))
        {
            Vector2Int cell = GetMouseCell();
            bool placed = TryPlaceBuildingAtCell(cell);

            // Nếu option hiện tại cho phép drag-build -> bật cờ
            BuildOption opt = GetCurrentOption();
            bool allowDrag =
                (opt != null && opt.allowDragBuild && opt.prefab != null);

            isDragBuilding = placed && allowDrag;
            lastPlacedCell = cell;
        }
        // Khi đang giữ chuột phải
        else if (Input.GetMouseButton(1))
        {
            if (!isDragBuilding) return;

            Vector2Int cell = GetMouseCell();
            if (cell != lastPlacedCell)
            {
                bool placed = TryPlaceBuildingAtCell(cell);
                if (placed)
                {
                    lastPlacedCell = cell;
                }
            }
        }
        // Nhả chuột phải -> tắt drag-build
        else if (Input.GetMouseButtonUp(1))
        {
            isDragBuilding = false;
        }
    }

    private Vector2Int GetMouseCell()
    {
        Vector3 worldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0f;
        return GridManager.Instance.WorldToCell(worldPos);
    }

    // =========================================
    //              HOTKEY & OPTION
    // =========================================

    private void HandleHotkeys()
    {
        if (options == null || options.Length == 0) return;

        string s = Input.inputString;
        if (!string.IsNullOrEmpty(s))
        {
            if (s.Contains("1") && options.Length > 0)
            {
                SelectBuildOption(0);
                return;
            }

            if (s.Contains("2") && options.Length > 1)
            {
                SelectBuildOption(1);
                return;
            }

            if (s.Contains("3") && options.Length > 2)
            {
                SelectBuildOption(2);
                return;
            }

            if (s.Contains("4") && options.Length > 3)
            {
                SelectBuildOption(3);
                return;
            }
        }

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

            // Nếu prefab là Conveyor -> dùng lại hướng cuối cùng
            if (opt.prefab != null && opt.prefab.GetComponent<ConveyorBuilding>() != null)
            {
                previewConveyorDir = lastConveyorDir;
            }
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

    // =========================================
    //                  PREVIEW
    // =========================================

    private void CreatePreviewForCurrentOption()
    {
        if (previewInstance != null)
        {
            Destroy(previewInstance.gameObject);
            previewInstance = null;
            previewRenderers = null;
        }

        BuildOption opt = GetCurrentOption();
        if (opt == null || opt.prefab == null) return;

        previewInstance = Instantiate(opt.prefab);
        previewInstance.gameObject.name = opt.prefab.name + "_Preview";

        // Tắt logic
        MonoBehaviour[] behaviours = previewInstance.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var mb in behaviours)
        {
            if (mb == null) continue;
            mb.enabled = false;
        }

        // Tắt collider / physics
        Collider2D[] cols = previewInstance.GetComponentsInChildren<Collider2D>();
        foreach (var c in cols) c.enabled = false;

        Rigidbody2D[] bodies = previewInstance.GetComponentsInChildren<Rigidbody2D>();
        foreach (var rb in bodies) rb.simulated = false;

        // Lấy sprite renderer (kể cả disable)
        previewRenderers = previewInstance.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in previewRenderers)
        {
            if (sr == null) continue;
            sr.enabled = true;
        }
        SetPreviewColor(invalidColor);

        // Nếu là conveyor -> xoay theo hướng hiện tại
        if (opt.prefab.GetComponent<ConveyorBuilding>() != null)
        {
            ApplyConveyorRotation(previewInstance.transform, previewConveyorDir);
        }
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

        BuildingBase prefabBase = opt.prefab;

        bool canPlace = GridManager.Instance.CanPlaceBuilding(prefabBase, cell);

        bool isDrill = prefabBase.GetComponent<OreBuilding>() != null;
        bool hasOreUnder = HasOreUnderBuilding(prefabBase, cell);

        if (isDrill)
        {
            canPlace = canPlace && hasOreUnder;
        }
        else
        {
            if (hasOreUnder)
                canPlace = false;
        }

        Vector3 centerWorld = GetBuildingWorldCenter(prefabBase, cell);
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
            previewInstance.gameObject.SetActive(visible);
    }

    // =========================================
    //            PLACE BUILDING (1 ô)
    // =========================================

    private bool TryPlaceBuildingAtCell(Vector2Int cell)
    {
        BuildOption opt = GetCurrentOption();
        if (opt == null || opt.prefab == null)
        {
            Debug.LogWarning("BuildingPlacer: chưa chọn prefab để đặt.");
            return false;
        }

        BuildingBase prefabBase = opt.prefab;

        bool canPlace = GridManager.Instance.CanPlaceBuilding(prefabBase, cell);

        bool isDrill = prefabBase.GetComponent<OreBuilding>() != null;
        bool hasOreUnder = HasOreUnderBuilding(prefabBase, cell);

        if (isDrill)
        {
            canPlace = canPlace && hasOreUnder;
        }
        else
        {
            if (hasOreUnder)
                canPlace = false;
        }

        if (!canPlace)
        {
            if (isDrill && !hasOreUnder && BuildFeedbackUI.Instance != null)
            {
                BuildFeedbackUI.Instance.ShowMessage(
                    "Drill must be placed on ore",
                    Input.mousePosition
                );
            }

            Debug.Log("Không thể đặt " + opt.id + " tại cell " + cell);
            return false;
        }

        CoreBuilding core = CoreBuilding.Instance;
        int cost = prefabBase.buildCostOre;

        if (core != null && cost > 0)
        {
            if (!core.TrySpendOre(cost))
            {
                if (BuildFeedbackUI.Instance != null)
                {
                    BuildFeedbackUI.Instance.ShowNotEnoughOre(Input.mousePosition);
                }

                Debug.Log(
                    $"Không đủ ore để xây {opt.id}. Cần {cost}, Core chỉ có {core.oreAmount}"
                );
                return false;
            }
        }

        BuildingBase b = Instantiate(prefabBase);
        GridManager.Instance.PlaceBuilding(b, cell);

        ConveyorBuilding conveyor = b.GetComponent<ConveyorBuilding>();
        if (conveyor != null)
        {
            conveyor.direction = previewConveyorDir;
            ApplyConveyorRotation(conveyor.transform, previewConveyorDir);
        }

        var buildEffect = b.GetComponent<BuildEffect>();
        if (buildEffect != null)
        {
            buildEffect.PlayPop();
        }

        Debug.Log(
            $"Đặt {opt.id} tại cell {cell}, tốn {cost} ore. Core còn {core?.oreAmount}"
        );

        return true;
    }

    // =========================================
    //           ORE CHECK (cho Drill)
    // =========================================

    private bool HasOreUnderBuilding(BuildingBase prefabBase, Vector2Int anchorCell)
    {
        if (GridManager.Instance == null) return false;

        GridManager gm = GridManager.Instance;
        Vector2Int size = prefabBase.size;

        for (int dx = 0; dx < size.x; dx++)
        {
            for (int dy = 0; dy < size.y; dy++)
            {
                Vector2Int cell = new Vector2Int(anchorCell.x + dx, anchorCell.y + dy);
                if (!gm.IsInBounds(cell)) continue;

                Vector3 center = gm.CellToWorldCenter(cell);

                Collider2D[] hits = Physics2D.OverlapCircleAll(center, gm.cellSize * 0.3f);
                foreach (var h in hits)
                {
                    if (h != null && h.GetComponentInParent<OreNode>() != null)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    // =========================================
    //          CONVEYOR ROTATION HELPERS
    // =========================================

    private void HandleConveyorRotateKey()
    {
        BuildOption opt = GetCurrentOption();
        if (opt == null || opt.prefab == null) return;
        if (opt.prefab.GetComponent<ConveyorBuilding>() == null) return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            previewConveyorDir = NextDirection(previewConveyorDir);
            lastConveyorDir = previewConveyorDir;

            if (previewInstance != null)
            {
                ApplyConveyorRotation(previewInstance.transform, previewConveyorDir);
            }
        }
    }

    private ConveyorDirection NextDirection(ConveyorDirection dir)
    {
        switch (dir)
        {
            case ConveyorDirection.Right: return ConveyorDirection.Up;
            case ConveyorDirection.Up: return ConveyorDirection.Left;
            case ConveyorDirection.Left: return ConveyorDirection.Down;
            case ConveyorDirection.Down: return ConveyorDirection.Right;
            default: return ConveyorDirection.Right;
        }
    }

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
