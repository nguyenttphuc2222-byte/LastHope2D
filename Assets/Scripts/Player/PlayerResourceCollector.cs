using UnityEngine;

public class PlayerResourceCollector : MonoBehaviour
{
    [Header("Mining")]
    [Tooltip("Khoảng cách tối đa để tương tác với Ore/Core.")]
    public float interactRange = 1.2f;

    [Tooltip("Thời gian giữa 2 lần đào (giây).")]
    public float mineInterval = 0.4f;

    [Header("Carried resources")]
    [Tooltip("Số ore player đang mang theo.")]
    public int carriedOre;

    private float mineTimer;

    private void Update()
    {
        mineTimer -= Time.deltaTime;

        // Giữ E để đào quặng khi đứng gần mỏ
        if (Input.GetKey(KeyCode.E))
        {
            TryMineOre();
        }

        // Bấm F một lần để nộp quặng vào Core khi đứng gần
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryDepositToCore();
        }
    }

    private void TryMineOre()
    {
        if (mineTimer > 0f) return;

        // Tìm OreNode gần nhất trong phạm vi
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRange);
        OreNode nearest = null;
        float bestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            OreNode node = hit.GetComponent<OreNode>();
            if (node == null) continue;

            float d = Vector2.Distance(transform.position, node.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                nearest = node;
            }
        }

        if (nearest == null)
        {
            // Không có mỏ nào trong phạm vi
            return;
        }

        int mined = nearest.Mine(1); // mỗi lần đào 1 ore
        if (mined > 0)
        {
            carriedOre += mined;
            mineTimer = mineInterval;
            Debug.Log($"Đào được {mined} ore. Đang mang theo: {carriedOre}");
        }
    }

    private void TryDepositToCore()
    {
        if (carriedOre <= 0) return;

        if (CoreBuilding.Instance == null)
        {
            Debug.LogWarning("Không tìm thấy Core để nộp quặng.");
            return;
        }

        float dist = Vector2.Distance(transform.position, CoreBuilding.Instance.transform.position);
        if (dist > interactRange)
        {
            Debug.Log("Đứng gần Core hơn để nộp quặng (ấn F).");
            return;
        }

        CoreBuilding.Instance.AddOre(carriedOre);
        Debug.Log($"Nộp {carriedOre} ore vào Core.");
        carriedOre = 0;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
