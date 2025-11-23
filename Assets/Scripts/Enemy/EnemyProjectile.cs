using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 6f;
    public int damage = 5;
    
    private Vector3 moveDir;
    private float lifeTime = 5f;

    public void Init(Vector3 targetPosition, int dmg)
    {
        damage = dmg;
        // Tính hướng bắn
        moveDir = (targetPosition - transform.position).normalized;
        
        // Xoay đạn theo hướng bắn
        float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        // Tự hủy sau 5s nếu không trúng gì
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // Bay thẳng theo hướng đã định
        transform.position += moveDir * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Chỉ gây dame cho các công trình (dựa theo Tag)
        if (collision.CompareTag("Defense") || collision.CompareTag("Core") || 
            collision.CompareTag("Wall") || collision.CompareTag("Resource") || collision.CompareTag("Building"))
        {
            BuildingBase building = collision.GetComponent<BuildingBase>();
            if (building != null)
            {
                building.TakeDamage(damage);
            }
            // Trúng mục tiêu thì biến mất
            Destroy(gameObject); 
        }
    }
}