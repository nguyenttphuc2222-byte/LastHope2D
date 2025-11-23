using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyBasic : MonoBehaviour
{
    [Header("Stats")]
    public float moveSpeed = 3f;
    public int maxHealth = 50;
    public int damagePerHit = 10;
    public float attackInterval = 1f;
    public float stopDistance = 0.1f;

    [Header("Scanning")]
    public float scanRadius = 10f; // Tầm quét mục tiêu

    [Header("Effects & Audio")]
    public Color flashColor = new Color(1f, 0.5f, 0.5f, 1f);
    public float flashDuration = 0.1f;
    public AudioClip deathSound;
    [Range(0f, 1f)] public float soundVolume = 0.8f;

    protected int currentHealth;
    protected Rigidbody2D rb;
    protected SpriteRenderer sr;
    protected Color originalColor;
    protected BuildingBase currentTarget;
    protected float attackTimer;
    protected bool isDead = false;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;
        currentHealth = maxHealth;
    }

    public void BuffStats(float multiplier)
    {
        if (multiplier <= 1f) return;
        maxHealth = Mathf.RoundToInt(maxHealth * multiplier);
        currentHealth = maxHealth;
        damagePerHit = Mathf.RoundToInt(damagePerHit * multiplier);
    }

    protected virtual void FixedUpdate()
    {
        if (isDead || CoreBuilding.Instance == null) return;

        // 1. Tìm mục tiêu
        FindTarget();
        if (currentTarget == null) return;

        // 2. Tính khoảng cách
        Vector2 dir = currentTarget.transform.position - transform.position;
        float dist = dir.magnitude;

        // 3. Di chuyển nếu còn xa
        if (dist > stopDistance)
        {
            Vector2 step = dir.normalized * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + step);
        }

        // 4. Tấn công khi vào tầm (cho phép dư padding để tránh lỗi collider đẩy ra)
        const float attackRangePadding = 0.5f;   // có thể tăng/giảm tuỳ map
        if (dist <= stopDistance + attackRangePadding)
        {
            AttackLogic();
        }
    }



    // --- LOGIC TÌM MỤC TIÊU (MẶC ĐỊNH) ---
    // Quy tắc Basic: Nhắm Core. Nếu đã có mục tiêu (do va chạm) thì giữ nguyên.
    protected virtual void FindTarget()
    {
        if (currentTarget != null && currentTarget.currentHealth > 0) return;
        currentTarget = CoreBuilding.Instance;
    }

    // --- LOGIC TẤN CÔNG (MẶC ĐỊNH) ---
    protected virtual void AttackLogic()
    {
        attackTimer -= Time.fixedDeltaTime;
        if (attackTimer <= 0f)
        {
            attackTimer = attackInterval;
            if (currentTarget != null)
            {
                currentTarget.TakeDamage(damagePerHit);
            }
        }
    }

    // --- LOGIC VA CHẠM (MẶC ĐỊNH) ---
    // Basic & Tank: Nếu va vào Building -> Chuyển mục tiêu sang Building đó để phá đường
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        BuildingBase b = collision.collider.GetComponent<BuildingBase>();

        // Nếu va vào 1 công trình (Tường, Trụ...)
        if (b != null && b.currentHealth > 0)
        {
            // Nếu đang nhắm Core (ở xa) mà bị chặn -> Đánh cái chặn đường trước
            if (currentTarget == CoreBuilding.Instance)
            {
                currentTarget = b;
            }
        }
    }

    // (Các hàm TakeDamage, Die giữ nguyên như cũ)
    public void TakeDamage(int amount)
    {
        if (amount <= 0 || currentHealth <= 0) return;
        currentHealth -= amount;
        if (gameObject.activeInHierarchy) StartCoroutine(FlashRoutine());
        if (currentHealth <= 0) Die();
    }

    protected IEnumerator FlashRoutine()
    {
        if (sr != null) { sr.color = flashColor; yield return new WaitForSeconds(flashDuration); sr.color = originalColor; }
    }

    protected void Die()
    {
        isDead = true;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        this.enabled = false;
        if (deathSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfx(deathSound, soundVolume);
        }

        StartCoroutine(DeathEffectRoutine());
    }

    protected IEnumerator DeathEffectRoutine()
    {
        float duration = 0.2f;
        float timer = 0f;
        Vector3 startScale = transform.localScale;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, timer / duration);
            yield return null;
        }
        Destroy(gameObject);
    }
}