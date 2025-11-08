using UnityEngine;

public class BuildingPlacer : MonoBehaviour
{
    [Header("Test placement")]
    public BuildingBase orePrefab;  // gán prefab OreBuilding ở Inspector
    public LayerMask placementBlockedMask; // nếu muốn thêm check bằng raycast (optional)

    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            TryPlaceOreAtMouse();
        }
    }

    private void TryPlaceOreAtMouse()
    {
        if (GridManager.Instance == null || orePrefab == null) return;

        Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector2Int cell = GridManager.Instance.WorldToCell(mouseWorld);

        // Khi đặt building, cell này là anchor (góc dưới - trái) của building
        // Với Ore size = (1,1) thì đơn giản.
        // Nếu sau này building 2×2, 3×2... vẫn dùng cell này làm anchor dưới - trái.

        // Tạo instance tạm để check size
        BuildingBase temp = orePrefab;

        if (!GridManager.Instance.CanPlaceBuilding(temp, cell))
        {
            Debug.Log("Không thể đặt Ore tại cell " + cell);
            return;
        }

        BuildingBase b = Instantiate(orePrefab);
        GridManager.Instance.PlaceBuilding(b, cell);
        Debug.Log("Đặt Ore tại cell " + cell);
    }
}
