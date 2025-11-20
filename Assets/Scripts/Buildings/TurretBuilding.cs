using UnityEngine;

public class TurretBuilding : BuildingBase
{
    [Header("Turret")]
    public float range = 4f;
    public float fireRate = 1f;     // viên / giây
    public int bulletDamage = 15;
    public Bullet bulletPrefab;

    [Tooltip("Transform của nòng súng, sẽ xoay về phía enemy.")]
    public Transform barrelTransform;

    [Tooltip("Điểm spawn đạn, nên là con của barrel.")]
    public Transform firePoint;

    [Tooltip("Tốc độ quay nòng (độ / giây). Đặt rất lớn nếu muốn quay gần như tức thì.")]
    public float rotateSpeed = 720f;

    private float fireCooldown;

    private void Update()
    {
        fireCooldown -= Time.deltaTime;

        // 1) Tìm target (nếu có)
        EnemyBasic target = FindTarget();

        // 2) Xoay nòng về phía target trước
        RotateBarrelTowards(target);

        // 3) Nếu có target & cooldown xong -> bắn
        if (target != null && fireCooldown <= 0f)
        {
            Shoot();
        }
    }

    private EnemyBasic FindTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range);
        EnemyBasic closest = null;
        float bestDistSqr = float.MaxValue;

        foreach (var h in hits)
        {
            EnemyBasic e = h.GetComponent<EnemyBasic>();
            if (e == null) continue;

            float dSqr = (e.transform.position - transform.position).sqrMagnitude;
            if (dSqr < bestDistSqr)
            {
                bestDistSqr = dSqr;
                closest = e;
            }
        }

        return closest;
    }

    private void RotateBarrelTowards(EnemyBasic target)
    {
        if (barrelTransform == null) return;

        if (target == null)
            return; // không có enemy trong range -> giữ nguyên hướng hiện tại

        Vector3 dir = (target.transform.position - barrelTransform.position).normalized;

        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // Nếu sprite của nòng gốc hướng lên (0,1) thì:
        targetAngle -= 90f;

        Quaternion targetRot = Quaternion.AngleAxis(targetAngle, Vector3.forward);

        // Quay mượt
        barrelTransform.rotation = Quaternion.RotateTowards(
            barrelTransform.rotation,
            targetRot,
            rotateSpeed * Time.deltaTime
        );
    }

    private void Shoot()
    {
        if (bulletPrefab == null) return;

        Vector3 origin = firePoint != null ? firePoint.position : barrelTransform.position;

        // Hướng bắn lấy từ hướng nòng sau khi đã quay (đảm bảo "nòng trước, đạn sau")
        Vector2 dir = barrelTransform.up; // nếu sprite nòng hướng lên; nếu hướng phải thì dùng barrelTransform.right

        Bullet b = Instantiate(bulletPrefab, origin, barrelTransform.rotation);
        b.Init(dir, bulletDamage);

        fireCooldown = 1f / fireRate;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
