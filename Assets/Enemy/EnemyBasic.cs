using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyBasic : MonoBehaviour
{
    [Header("Stats")]
    public float moveSpeed = 3f;
    public int maxHealth = 50;

    [Header("Attack")]
    public int damagePerHit = 10;
    public float attackInterval = 0.5f;
    public float stopDistance = 0.1f;   // khoảng cách dừng lại trước mục tiêu

    private int currentHealth;
    private Rigidbody2D rb;
    private BuildingBase currentTarget;
    private float attackTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    private void FixedUpdate()
    {
        if (CoreBuilding.Instance == null)
            return;

        // nếu target hiện tại chết thì bỏ, quay lại đánh Core
        if (currentTarget != null && currentTarget.currentHealth <= 0)
        {
            currentTarget = null;
        }

        // chọn target mặc định là Core nếu chưa có
        if (currentTarget == null)
        {
            currentTarget = CoreBuilding.Instance;
        }

        // di chuyển tới target
        Vector2 targetPos = currentTarget.transform.position;
        Vector2 dir = targetPos - rb.position;
        float dist = dir.magnitude;

        if (dist > stopDistance)
        {
            Vector2 step = dir.normalized * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + step);
        }

        // tấn công nếu đang trong tầm (trigger đang chạm) và cooldown xong
        if (currentTarget != null && dist <= stopDistance + 0.05f)
        {
            attackTimer -= Time.fixedDeltaTime;
            if (attackTimer <= 0f)
            {
                attackTimer = attackInterval;
                currentTarget.TakeDamage(damagePerHit);
            }
        }
    }

    // Khi chạm bất kỳ BuildingBase nào, nó trở thành target mới
    private void OnCollisionEnter2D(Collision2D collision)
    {
        BuildingBase b = collision.collider.GetComponent<BuildingBase>();
        if (b != null)
        {
            currentTarget = b;
            attackTimer = 0f; // đánh ngay
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        BuildingBase b = collision.collider.GetComponent<BuildingBase>();
        if (b != null && b == currentTarget)
        {
            currentTarget = null; // rời khỏi building -> quay lại core
        }
    }


    // để dùng sau này (turret / player bắn quái)
    public void TakeDamage(int amount)
    {
        if (amount <= 0 || currentHealth <= 0) return;

        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
