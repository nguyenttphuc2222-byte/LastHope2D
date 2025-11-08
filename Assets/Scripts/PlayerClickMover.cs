using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerClickMover : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 targetPos;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        targetPos = rb.position;
    }

    private void Update()
    {
        // Click chuột trái => đặt target
        if (Input.GetMouseButtonDown(0)) // 0 = left
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;

            Vector2 target = mouseWorld;

            // Clamp vị trí trong giới hạn map từ GridManager
            if (GridManager.Instance != null)
            {
                Rect bounds = GridManager.Instance.GetWorldBounds();
                target.x = Mathf.Clamp(target.x, bounds.xMin, bounds.xMax);
                target.y = Mathf.Clamp(target.y, bounds.yMin, bounds.yMax);
            }

            targetPos = target;
        }
    }

    private void FixedUpdate()
    {
        Vector2 current = rb.position;

        if (Vector2.Distance(current, targetPos) > 0.01f)
        {
            Vector2 newPos = Vector2.MoveTowards(current, targetPos, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);
        }
        else
        {
            // Dừng lại khi đã tới
            rb.linearVelocity = Vector2.zero;
        }
    }
}
