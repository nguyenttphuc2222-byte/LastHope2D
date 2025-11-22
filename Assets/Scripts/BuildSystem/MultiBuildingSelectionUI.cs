using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class MultiBuildingSelectionUI : MonoBehaviour
{
    [Header("Refs")]
    public Camera mainCamera;
    public BuildingPlacer buildingPlacer;

    [Header("Selection Box UI")]
    [Tooltip("Image (RectTransform) để vẽ khung kéo chuột.")]
    public RectTransform selectionBox;   // một Image trong Canvas

    [Header("Multi-Panel UI")]
    public GameObject panel;
    public TextMeshProUGUI nameText;          // Tên công trình hoặc 'Mixed'
    public TextMeshProUGUI countText;         // Số lượng đã chọn
    public TextMeshProUGUI upgradeCostText;   // Tổng ore cần để upgrade
    public TextMeshProUGUI repairCostText;    // Tổng ore cần để repair
    public TextMeshProUGUI sellRefundText;    // Tổng ore nhận khi sell

    public Button upgradeAllButton;
    public Button repairAllButton;
    public Button sellAllButton;

    // runtime
    private bool isDragging = false;
    private Vector2 dragStartScreen;

    public bool IsDragging => isDragging;

    // Canvas chứa selectionBox (để convert Screen -> Local)
    private Canvas selectionCanvas;
    private RectTransform canvasRect;


    private readonly List<BuildingBase> selectedBuildings = new List<BuildingBase>();

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (selectionBox != null)
        {
            selectionBox.gameObject.SetActive(false);

            selectionCanvas = selectionBox.GetComponentInParent<Canvas>();
            if (selectionCanvas != null)
                canvasRect = selectionCanvas.GetComponent<RectTransform>();
        }


        if (panel != null)
            panel.SetActive(false);

        if (upgradeAllButton != null)
            upgradeAllButton.onClick.AddListener(OnClickUpgradeAll);

        if (repairAllButton != null)
            repairAllButton.onClick.AddListener(OnClickRepairAll);

        if (sellAllButton != null)
            sellAllButton.onClick.AddListener(OnClickSellAll);
    }

    private void Update()
    {
        // Nếu đang build mode thì không cho multi-select
        if (buildingPlacer != null && buildingPlacer.buildModeEnabled)
        {
            CancelDrag();
            return;
        }

        HandleDragInput();
    }

    private void HandleDragInput()
    {
        // Bắt đầu kéo: chuột trái down, không đè lên UI
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            isDragging = true;
            dragStartScreen = Input.mousePosition;

            if (selectionBox != null)
            {
                selectionBox.gameObject.SetActive(true);
                UpdateSelectionBoxVisual(Input.mousePosition);
            }
        }

        // Đang kéo
        if (isDragging && Input.GetMouseButton(0))
        {
            if (selectionBox != null)
            {
                UpdateSelectionBoxVisual(Input.mousePosition);
            }
        }

        // Thả chuột -> kết thúc chọn
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;

            if (selectionBox != null)
                selectionBox.gameObject.SetActive(false);

            PerformSelection(dragStartScreen, Input.mousePosition);
        }
    }

    private void CancelDrag()
    {
        isDragging = false;
        if (selectionBox != null)
            selectionBox.gameObject.SetActive(false);
    }

    // Cập nhật size + vị trí khung UI theo 2 điểm màn hình (Screen -> Local Canvas)
    private void UpdateSelectionBoxVisual(Vector2 currentMousePos)
    {
        if (selectionBox == null || canvasRect == null || selectionCanvas == null)
            return;

        // Nếu Canvas là Screen Space Overlay thì camera = null
        Camera uiCam = (selectionCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            ? null
            : selectionCanvas.worldCamera;

        Vector2 startLocal;
        Vector2 endLocal;

        // Convert 2 điểm screen sang local của CHÍNH Canvas
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, dragStartScreen, uiCam, out startLocal);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, currentMousePos, uiCam, out endLocal);

        // Lấy min / max trong không gian local canvas
        Vector2 min = Vector2.Min(startLocal, endLocal);
        Vector2 max = Vector2.Max(startLocal, endLocal);

        Vector2 size = max - min;          // rộng / cao của box
        Vector2 center = (min + max) * 0.5f; // tâm box

        // anchoredPosition = TÂM của box (không phải min nữa)
        selectionBox.anchoredPosition = center;
        selectionBox.sizeDelta = size;
    }



    // Thực hiện chọn tất cả BuildingBase trong vùng kéo
    private void PerformSelection(Vector2 screenStart, Vector2 screenEnd)
    {
        ClearSelection(); // bỏ chọn cũ

        if (mainCamera == null) return;

        // Chuyển vùng màn hình -> vùng thế giới
        Vector3 w1 = mainCamera.ScreenToWorldPoint(screenStart);
        Vector3 w2 = mainCamera.ScreenToWorldPoint(screenEnd);

        Vector2 min = new Vector2(Mathf.Min(w1.x, w2.x), Mathf.Min(w1.y, w2.y));
        Vector2 max = new Vector2(Mathf.Max(w1.x, w2.x), Mathf.Max(w1.y, w2.y));

        // Nếu kéo rất nhỏ (gần click 1 điểm) thì thôi (tránh chọn linh tinh)
        const float minSelectSize = 0.1f;
        if ((max - min).magnitude < minSelectSize)
        {
            HidePanel();
            return;
        }

        Collider2D[] hits = Physics2D.OverlapAreaAll(min, max);
        if (hits == null || hits.Length == 0)
        {
            HidePanel();
            return;
        }

        foreach (var h in hits)
        {
            if (h == null) continue;
            BuildingBase b = h.GetComponentInParent<BuildingBase>();
            if (b == null) continue;
            if (b.currentHealth <= 0) continue; // đã chết thì bỏ

            if (!selectedBuildings.Contains(b))
                selectedBuildings.Add(b);
        }

        // Highlight các building được chọn
        foreach (var b in selectedBuildings)
        {
            if (b == null) continue;
            var hl = b.GetComponent<BuildingSelectionHighlight>();
            if (hl != null)
            {
                hl.SetHighlighted(true);
            }
        }

        if (selectedBuildings.Count > 0)
        {
            UpdatePanel();
        }
        else
        {
            HidePanel();
        }
    }

    private void ClearSelection()
    {
        // Tắt highlight cũ
        foreach (var b in selectedBuildings)
        {
            if (b == null) continue;
            var hl = b.GetComponent<BuildingSelectionHighlight>();
            if (hl != null)
            {
                hl.SetHighlighted(false);
            }
        }
        selectedBuildings.Clear();
    }

    private void HidePanel()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    // ---------- UI PANEL TỔNG HỢP ----------

    private void UpdatePanel()
    {
        if (panel == null) return;

        // dọn list null (building đã bị Destroy)
        selectedBuildings.RemoveAll(b => b == null);

        if (selectedBuildings.Count == 0)
        {
            HidePanel();
            return;
        }

        panel.SetActive(true);

        // 1) Tên công trình: nếu 1 loại -> hiện tên, nếu nhiều -> "Mixed"
        string displayNameSummary = BuildDisplayNameSummary(selectedBuildings);
        if (nameText != null)
        {
            nameText.text = displayNameSummary;
        }

        // 2) Số lượng
        if (countText != null)
        {
            countText.text = $"Selected: {selectedBuildings.Count}";
        }

        // 3) Tổng cost
        int totalUpgradeCost = 0;
        int totalRepairCost = 0;
        int totalSellRefund = 0;

        foreach (var b in selectedBuildings)
        {
            if (b == null) continue;

            // Upgrade
            var up = b.GetComponent<BuildingUpgrade>();
            if (up != null && up.CanUpgrade && !(b is CoreBuilding))
            {
                totalUpgradeCost += up.UpgradeCost;
            }

            // Repair / Sell
            var rs = b.GetComponent<BuildingRepairSell>();
            if (rs != null)
            {
                totalRepairCost += rs.RepairCost;

                // Core chỉ được sửa, không được bán
                if (!(b is CoreBuilding))
                {
                    totalSellRefund += rs.SellRefund;
                }
            }
        }

        if (upgradeCostText != null)
            upgradeCostText.text = $"Upgrade total cost: {totalUpgradeCost} ore";

        if (repairCostText != null)
            repairCostText.text = $"Repair total cost: {totalRepairCost} ore";

        if (sellRefundText != null)
            sellRefundText.text = $"Sell total refund: {totalSellRefund} ore";

        // 4) Enable/Disable nút
        if (upgradeAllButton != null)
            upgradeAllButton.interactable = (totalUpgradeCost > 0);

        if (repairAllButton != null)
            repairAllButton.interactable = (totalRepairCost > 0);

        if (sellAllButton != null)
            sellAllButton.interactable = (totalSellRefund > 0);
    }

    private string BuildDisplayNameSummary(List<BuildingBase> buildings)
    {
        // Lấy displayName giống BuildingInfoUI
        Dictionary<string, int> counts = new Dictionary<string, int>();

        foreach (var b in buildings)
        {
            if (b == null) continue;

            string displayName = b.gameObject.name;
            var meta = b.GetComponent<BuildingDisplayName>();
            if (meta != null && !string.IsNullOrEmpty(meta.displayName))
            {
                displayName = meta.displayName;
            }
            else
            {
                displayName = displayName.Replace("(Clone)", "");
            }

            if (!counts.ContainsKey(displayName))
                counts[displayName] = 0;

            counts[displayName]++;
        }

        if (counts.Count == 1)
        {
            // chỉ 1 loại
            var kv = counts.First();
            return $"{kv.Key} (x{kv.Value})";
        }
        else
        {
            // nhiều loại
            return $"Mixed ({counts.Count} types)";
        }
    }

    // ---------- BATCH ACTIONS ----------

    private void OnClickUpgradeAll()
    {
        if (selectedBuildings.Count == 0) return;

        foreach (var b in selectedBuildings)
        {
            if (b == null) continue;
            if (b is CoreBuilding) continue; // Core không upgrade

            var up = b.GetComponent<BuildingUpgrade>();
            if (up == null) continue;

            // TryUpgrade() tự check ore & trừ Core
            up.TryUpgrade();
        }

        // Sau khi upgrade, refresh panel (cost, level,...)
        UpdatePanel();
    }

    private void OnClickRepairAll()
    {
        if (selectedBuildings.Count == 0) return;

        foreach (var b in selectedBuildings)
        {
            if (b == null) continue;

            var rs = b.GetComponent<BuildingRepairSell>();
            if (rs == null) continue;

            rs.TryRepairFull();   // Core được phép sửa
        }

        UpdatePanel();
    }

    private void OnClickSellAll()
    {
        if (selectedBuildings.Count == 0) return;

        foreach (var b in selectedBuildings)
        {
            if (b == null) continue;

            // Core chỉ sửa, không bán
            if (b is CoreBuilding)
                continue;

            var rs = b.GetComponent<BuildingRepairSell>();
            if (rs == null) continue;

            rs.TrySell();
        }

        // Xoá các building đã bị phá
        selectedBuildings.RemoveAll(b => b == null || b.currentHealth <= 0);

        // Tắt highlight (vì nhiều cái đã destroy)
        foreach (var b in selectedBuildings)
        {
            if (b == null) continue;
            var hl = b.GetComponent<BuildingSelectionHighlight>();
            if (hl != null)
                hl.SetHighlighted(true); // vẫn giữ highlight cho những cái còn sống
        }

        UpdatePanel();
    }
}
