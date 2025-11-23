using UnityEngine;

public class TankEnemy : EnemyBasic
{
    protected override void Awake()
    {
        base.Awake();
        
        // Chỉ số riêng cho Tank
        moveSpeed *= 0.7f;
        maxHealth *= 3;
        damagePerHit = Mathf.RoundToInt(damagePerHit * 1.5f);
    }

    // Không cần override logic nào khác vì Tank hoạt động giống Basic (gặp gì đánh nấy)
}