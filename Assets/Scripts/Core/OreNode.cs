using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class OreNode : MonoBehaviour
{
    [Tooltip("Tổng số quặng trong mỏ. Nếu < 0 nghĩa là vô hạn.")]
    public int totalOre = -1;

    private void Start()
    {
        SnapToGrid();
    }

#if UNITY_EDITOR
    // Cho tiện khi chỉnh trong Scene: mỗi lần move prefab sẽ tự snap
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            SnapToGrid();
        }
    }
#endif

    private void SnapToGrid()
    {
        if (GridManager.Instance == null) return;

        GridManager gm = GridManager.Instance;
        Vector2Int cell = gm.WorldToCell(transform.position);
        transform.position = gm.CellToWorldCenter(cell);
    }

    public int Mine(int amount)
    {
        if (amount <= 0) return 0;

        if (totalOre < 0)
        {
            // Mỏ vô hạn
            return amount;
        }

        int mined = Mathf.Min(amount, totalOre);
        totalOre -= mined;

        if (totalOre == 0)
        {
            Debug.Log("Mỏ quặng đã cạn.");
            // TODO: đổi sprite / disable collider / Destroy(gameObject) tùy ý
        }

        return mined;
    }
}
