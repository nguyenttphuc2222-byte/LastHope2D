using UnityEngine;

public class RangedEnemy : EnemyBasic
{
    [Header("Ranged Settings")]
    public GameObject projectilePrefab; // Kéo Prefab viên đạn (EnemyBullet) vào đây
    public float shootRange = 5f;       // Tầm bắn (nên đặt khoảng 5-7)

    protected override void Awake()
    {
        base.Awake();
        
        // --- CHỈ SỐ RIÊNG ---
        moveSpeed *= 0.8f; // Đi chậm hơn Basic để có thời gian ngắm
        stopDistance = shootRange - 0.5f; // Dừng lại khi vào tầm bắn (xa hơn quái cận chiến)
    }

    // --- LOGIC TÌM MỤC TIÊU (ĐÃ SỬA) ---
    protected override void FindTarget()
    {
        // 1. Kiểm tra mục tiêu hiện tại:
        // Nếu đang nhắm vào DEFENSE (Trụ) và nó còn sống -> Giữ nguyên, trung thành với mục tiêu này.
        if (currentTarget != null && currentTarget.CompareTag("Defense"))
        {
            if (currentTarget.currentHealth > 0) return;
        }

        // 2. Nếu đang nhắm Core (hoặc chưa có mục tiêu), vẫn QUÉT TIẾP để xem có Trụ nào ngon hơn không
        // (Đây là đoạn fix lỗi: không return sớm nếu target là Core)
        
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, scanRadius);
        float minDist = Mathf.Infinity;
        BuildingBase potentialTarget = null;

        foreach (var hit in hits)
        {
            // Chỉ quan tâm đến Tag "Defense" (Trụ súng, Tường...)
            if (hit.CompareTag("Defense")) 
            {
                BuildingBase b = hit.GetComponent<BuildingBase>();
                
                // Kiểm tra kỹ: Building phải có script và còn máu
                if (b != null && b.currentHealth > 0)
                {
                    float d = Vector2.Distance(transform.position, hit.transform.position);
                    
                    // Tìm cái gần nhất
                    if (d < minDist)
                    {
                        minDist = d;
                        potentialTarget = b;
                    }
                }
            }
        }

        // 3. Quyết định mục tiêu cuối cùng
        if (potentialTarget != null)
        {
            // A! Tìm thấy Trụ rồi -> Bỏ Core, chuyển sang đánh Trụ
            currentTarget = potentialTarget;
        }
        else
        {
            // Không tìm thấy Trụ nào xung quanh
            // Nếu chưa có mục tiêu nào hết -> Thì đành phải đánh Core
            if (currentTarget == null) currentTarget = CoreBuilding.Instance;
        }
    }

    // --- LOGIC TẤN CÔNG: BẮN ĐẠN ---
    protected override void AttackLogic()
    {
        attackTimer -= Time.fixedDeltaTime;
        if (attackTimer <= 0f)
        {
            attackTimer = attackInterval;
            
            // Kiểm tra xem có Prefab đạn chưa
            if (currentTarget != null && projectilePrefab != null)
            {
                // 1. Sinh ra viên đạn tại vị trí của quái
                GameObject bullet = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                
                // 2. Cài đặt thông số cho đạn (bay đến đâu, damage bao nhiêu)
                EnemyProjectile script = bullet.GetComponent<EnemyProjectile>();
                if (script != null)
                {
                    script.Init(currentTarget.transform.position, damagePerHit);
                }
            }
        }
    }
}