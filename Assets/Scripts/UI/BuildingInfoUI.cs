using UnityEngine;
using TMPro;

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

    // building đang được theo dõi
    private BuildingBase selectedBuilding;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (panel != null)
            panel.SetActive(false);
    }

    private void Update()
    {
        // 1) Right-click để chọn / bỏ chọn building (chỉ khi build mode OFF)
        if (Input.GetMouseButtonDown(1))
        {
            if (buildingPlacer != null && buildingPlacer.buildModeEnabled)
                return;

            HandleRightClick();
        }

        // 2) Nếu đang có building được theo dõi, cập nhật info liên tục
        if (selectedBuilding != null && panel != null && panel.activeSelf)
        {
            RefreshSelectedInfo();
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

        Collider2D hit = Physics2D.OverlapPoint(point);
        if (hit == null)
        {
            ClearSelection();
            return;
        }

        BuildingBase building = hit.GetComponentInParent<BuildingBase>();
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

        // 3) Ore (chỉ với Core)
        CoreBuilding core = selectedBuilding as CoreBuilding;
        if (oreText != null)
        {
            if (core != null)
            {
                oreText.gameObject.SetActive(true);
                oreText.text = $"Ore: {core.oreAmount}";
            }
            else
            {
                oreText.gameObject.SetActive(false);
            }
        }
    }
}
