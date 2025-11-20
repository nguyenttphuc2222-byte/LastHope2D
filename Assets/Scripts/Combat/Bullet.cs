using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 10;
    public float lifeTime = 3f;

    private Rigidbody2D rb;
    private Vector2 moveDir;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Init(Vector2 direction, int dmg)
    {
        moveDir = direction.normalized;
        damage = dmg;

        // Xoay sprite cho đầu đạn hướng theo moveDir
        float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
        // Nếu sprite gốc đang "hướng lên" (0,1) thì trục up cần quay tới angle
        transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
        // Nếu sprite gốc đang "hướng sang phải" (1,0) thì bỏ -90f:
        // transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }


    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveDir * speed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyBasic enemy = other.GetComponent<EnemyBasic>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
