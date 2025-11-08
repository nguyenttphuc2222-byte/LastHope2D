using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Vector2 mapMin = new Vector2(-10f, -10f); // giới hạn map
    public Vector2 mapMax = new Vector2(10f, 10f);

    private Vector3 targetPos;

    void Start()
    {
        targetPos = transform.position;
    }

    void Update()
    {

        // Click chuột trái => đặt target
        if (Input.GetMouseButtonDown(0))//0 là trái, 1 là phải
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;

            // Clamp vị trí trong giới hạn map
            float clampedX = Mathf.Clamp(mouseWorld.x, mapMin.x, mapMax.x);
            float clampedY = Mathf.Clamp(mouseWorld.y, mapMin.y, mapMax.y);
            targetPos = new Vector3(clampedX, clampedY, 0);
        }

        // Di chuyển tới target
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
    }
}
