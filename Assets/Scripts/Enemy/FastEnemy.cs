using UnityEngine;

public class FastEnemy : EnemyBasic
{
    protected override void Awake()
    {
        base.Awake();
        moveSpeed *= 1.5f;
        maxHealth = Mathf.RoundToInt(maxHealth * 0.6f);
    }

    protected override void FindTarget()
    {
        // Nếu đang nhắm đúng mục tiêu ưu tiên (Resource hoặc Core) thì thôi không tìm nữa
        if (currentTarget != null && (currentTarget.CompareTag("Resource") || currentTarget.CompareTag("Core")))
        {
            if (currentTarget.currentHealth > 0) return;
        }

        // 1. Quét tìm Resource gần nhất trong scanRadius
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, scanRadius);
        float minDist = Mathf.Infinity;
        BuildingBase potentialTarget = null;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Resource")) // Chỉ quan tâm Resource
            {
                float d = Vector2.Distance(transform.position, hit.transform.position);
                if (d < minDist)
                {
                    minDist = d;
                    potentialTarget = hit.GetComponent<BuildingBase>();
                }
            }
        }

        // 2. Chọn mục tiêu
        if (potentialTarget != null)
            currentTarget = potentialTarget;
        else
            currentTarget = CoreBuilding.Instance; // Không có Resource thì lao vào Core
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        // FAST ENEMY LOGIC: "Bỏ qua các công trình khác"
        // Override lại hàm này và để trống -> Không chuyển mục tiêu khi va chạm Tường/Trụ.
        // Nó sẽ cố gắng đẩy qua (nhờ physics) thay vì dừng lại đánh.
    }
}