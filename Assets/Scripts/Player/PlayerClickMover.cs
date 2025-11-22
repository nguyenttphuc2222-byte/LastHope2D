using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerClickMover : MonoBehaviour
{
    public float moveSpeed = 15f;

    [Tooltip("Kéo chuột ngắn hơn (pixel) thì coi là click, dài hơn thì coi là drag (multi-select).")]
    public float clickMaxDragDistance = 10f;   // thử 10px, sau muốn thì tăng/giảm

    private Rigidbody2D rb;
    private Vector2 targetPos;

    // lưu vị trí mouse khi bắt đầu nhấn
    private Vector2 mouseDownScreenPos;
    private bool hasMouseDown = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        targetPos = rb.position;
    }

    private void Update()
    {
        // 1) Mouse DOWN: chỉ lưu vị trí, chưa move vội
        if (Input.GetMouseButtonDown(0)) // chuột trái
        {
            // nếu đang click lên UI thì bỏ qua
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            mouseDownScreenPos = Input.mousePosition;
            hasMouseDown = true;
        }

        // 2) Mouse UP: nếu kéo ngắn -> coi là click để move
        if (Input.GetMouseButtonUp(0) && hasMouseDown)
        {
            hasMouseDown = false;

            // thả chuột mà đang trên UI thì cũng bỏ qua
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Vector2 mouseUpScreenPos = Input.mousePosition;
            float sqrDragDist = (mouseUpScreenPos - mouseDownScreenPos).sqrMagnitude;

            // nếu kéo quá xa -> đây là drag (multi-select) -> KHÔNG di chuyển player
            if (sqrDragDist > clickMaxDragDistance * clickMaxDragDistance)
                return;

            // => đây là 1 cú CLICK thực sự -> set targetPos
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseUpScreenPos);
            mouseWorld.z = 0f;

            Vector2 target = mouseWorld;

            // Clamp trong bounds của map
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
#if UNITY_6000_OR_NEWER
            rb.linearVelocity = Vector2.zero;   // nếu bạn đang dùng API mới
#else
            rb.linearVelocity = Vector2.zero;         // nếu dùng Unity/Physics2D cũ
#endif
        }
    }
}
