using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;   // <-- thêm dòng này

public class BuildingInfoUI : MonoBehaviour
{
    [Header("Refs")]
    public Camera mainCamera;
    public BuildingPlacer buildingPlacer;

    [Header("UI")]
    public GameObject panel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI oreText;   // chỉ dùng cho Core

    [Header("Upgrade UI")]
    public Button upgradeButton;              // nút Upgrade
    public TextMeshProUGUI upgradeLabel;      // chữ trên nút (hoặc text bên cạnh)

    [Header("Repair / Sell UI")]
    public TextMeshProUGUI actionHintsText;


    // building đang được theo dõi
    private BuildingBase selectedBuilding;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (panel != null)
            panel.SetActive(false);

        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OnClickUpgrade);
    }

    private void Update()
    {
        // 1) Right-click để chọn / bỏ chọn building (chỉ khi build mode OFF)
        if (Input.GetMouseButtonDown(1))
        {
            // Nếu đang ở build mode thì bỏ qua
            if (buildingPlacer != null && buildingPlacer.buildModeEnabled)
                return;

            // Nếu chuột đang nằm trên UI (panel, nút Upgrade, …) thì cũng bỏ qua
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject())
                return;

            HandleRightClick();
        }

        // 2) Nếu đang có building được theo dõi, cập nhật info liên tục
        if (selectedBuilding != null && panel != null && panel.activeSelf)
        {
            RefreshSelectedInfo();
        }

        if (selectedBuilding != null && panel != null && panel.activeSelf)
        {
            RefreshSelectedInfo();
            HandleRepairSellHotkeys();   // ← bỏ hẳn check pointerOnUI
        }
        else if (selectedBuilding == null && panel != null && panel.activeSelf)
        {
            HidePanel();
        }


        // 3) Nếu object đã bị Destroy (Unity trả về null) -> tắt panel
        else if (selectedBuilding == null && panel != null && panel.activeSelf)
        {
            HidePanel();
        }
    }

    private void HandleRightClick()
    {
        if (mainCamera == null) return;

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 point = worldPos;

        // DÙNG OverlapPointAll THAY VÌ OverlapPoint
        Collider2D[] hits = Physics2D.OverlapPointAll(point);

        BuildingBase building = null;

        foreach (var h in hits)
        {
            if (h == null) continue;

            building = h.GetComponentInParent<BuildingBase>();
            if (building != null)
            {
                break; // tìm thấy building đầu tiên là đủ
            }
        }

        if (building == null)
        {
            ClearSelection();
            return;
        }

        SelectBuilding(building);
    }


    private void SelectBuilding(BuildingBase building)
    {
        selectedBuilding = building;

        if (panel == null) return;

        // Đặt panel gần vị trí chuột tại thời điểm click
        panel.SetActive(true);
        RectTransform rt = panel.transform as RectTransform;
        if (rt != null)
        {
            Vector2 mousePos = Input.mousePosition;
            mousePos += new Vector2(10f, -10f); // lệch nhẹ để không che con trỏ
            rt.position = mousePos;
        }

        // Cập nhật nội dung lần đầu
        RefreshSelectedInfo();
    }

    private void ClearSelection()
    {
        selectedBuilding = null;
        HidePanel();
    }

    private void HidePanel()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private void RefreshSelectedInfo()
    {
        // building đã bị Destroy
        if (selectedBuilding == null)
        {
            ClearSelection();
            return;
        }

        // 1) Tên hiển thị
        string displayName = selectedBuilding.gameObject.name;

        BuildingDisplayName meta = selectedBuilding.GetComponent<BuildingDisplayName>();
        if (meta != null && !string.IsNullOrEmpty(meta.displayName))
        {
            displayName = meta.displayName;
        }
        else
        {
            // fallback: bỏ "(Clone)" nếu có
            displayName = displayName.Replace("(Clone)", "");
        }

        if (nameText != null)
            nameText.text = displayName;

        // 2) HP
        if (hpText != null)
            hpText.text = $"HP: {selectedBuilding.currentHealth} / {selectedBuilding.maxHealth}";

        // 3) Ore / Resource info: Core, Drill (OreBuilding), Conveyor
        if (oreText != null)
        {
            // Lấy component cùng GameObject với selectedBuilding
            CoreBuilding core = selectedBuilding.GetComponent<CoreBuilding>();
            OreBuilding drill = selectedBuilding.GetComponent<OreBuilding>();
            ConveyorBuilding conveyor = selectedBuilding.GetComponent<ConveyorBuilding>();

            if (core != null)
            {
                oreText.gameObject.SetActive(true);
                oreText.text = $"Ore: {core.oreAmount}";
            }
            else if (drill != null)
            {
                oreText.gameObject.SetActive(true);
                int maxStorage = drill.GetCurrentMaxStorage();   // dùng hàm trong OreBuilding
                oreText.text = $"Stored ore: {drill.storedOre} / {maxStorage}";
            }
            else if (conveyor != null)
            {
                oreText.gameObject.SetActive(true);

                if (conveyor.HasItem && !conveyor.CarriedItem.IsEmpty)
                {
                    var stack = conveyor.CarriedItem;
                    // Nếu sau này có nhiều loại resource, text này vẫn ổn
                    oreText.text = $"Ore: {stack.type} x{stack.amount}";
                }
                else
                {
                    oreText.text = "Ore: (empty)";
                }
            }
            else
            {
                oreText.gameObject.SetActive(false);
            }
        }




        // 4) Phần Upgrade
        UpdateUpgradeSection();
        UpdateRepairSellSection();

    }

    private void UpdateRepairSellSection()
    {
        if (actionHintsText == null)
            return;

        if (selectedBuilding == null)
        {
            actionHintsText.gameObject.SetActive(false);
            return;
        }

        BuildingRepairSell rs = selectedBuilding.GetComponent<BuildingRepairSell>();
        if (rs == null)
        {
            actionHintsText.gameObject.SetActive(false);
            return;
        }

        string msg = "";

        // Hint repair
        if (rs.canRepair && rs.IsDamaged && rs.RepairCost > 0)
        {
            msg += $"R – Repair (-{rs.RepairCost} ore)";
        }

        // Hint sell
        if (rs.canSell && rs.SellRefund > 0)
        {
            if (msg.Length > 0) msg += "\n";
            msg += $"X – Sell (+{rs.SellRefund} ore)";
        }

        if (string.IsNullOrEmpty(msg))
        {
            actionHintsText.gameObject.SetActive(false);
        }
        else
        {
            actionHintsText.gameObject.SetActive(true);
            actionHintsText.text = msg;
        }
    }

    private void HandleRepairSellHotkeys()
    {
        BuildingRepairSell rs = selectedBuilding.GetComponent<BuildingRepairSell>();
        if (rs == null) return;

        // R – Repair
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (rs.TryRepairFull())
            {
                RefreshSelectedInfo();
            }
        }

        // X – Sell
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (rs.TrySell())
            {
                ClearSelection(); // building đã bị destroy -> tắt panel
            }
        }
    }


    private void UpdateUpgradeSection()
    {
        if (upgradeButton == null)
            return;

        if (selectedBuilding == null)
        {
            upgradeButton.gameObject.SetActive(false);
            return;
        }

        BuildingUpgrade up = selectedBuilding.GetComponent<BuildingUpgrade>();
        if (up == null || !up.CanUpgrade)
        {
            upgradeButton.gameObject.SetActive(false);
            return;
        }

        upgradeButton.gameObject.SetActive(true);

        if (upgradeLabel != null)
        {
            int cost = up.UpgradeCost;
            upgradeLabel.text = cost > 0
                ? $"Upgrade (Lv {up.currentLevel} → {up.NextLevel})\nCost: {cost} ore"
                : $"Upgrade (Lv {up.currentLevel} → {up.NextLevel})";
        }
    }

    public void OnClickUpgrade()
    {
        if (selectedBuilding == null) return;

        BuildingUpgrade up = selectedBuilding.GetComponent<BuildingUpgrade>();
        if (up == null) return;

        if (up.TryUpgrade())
        {
            // upgrade thành công -> refresh lại info (HP, level, cost mới,…)
            RefreshSelectedInfo();
        }
    }
}
