using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Dùng trong Play mode: chọn prefab placePrefab (Placeable), di chuột để preview ghost,
/// click trái để đặt nếu area free. Click vào object đã đặt để pick up (move).
/// Attach lên 1 GameObject (e.g., GameManager).
/// </summary>
public class PlacementController : MonoBehaviour
{
    public GridManager grid;
    [Tooltip("Prefab phải chứa component Placeable")]
    public GameObject placePrefab;

    GameObject ghost; // preview instance
    Placeable ghostPlaceable;
    bool isDraggingExisting = false;
    GameObject draggingObject = null; // the actual placed obj being moved (removed from grid on pickup)

    void Start()
    {
        if (grid == null) grid = FindObjectOfType<GridManager>();
    }

    void Update()
    {
        if (grid == null) return;

        // If right-click, cancel placing
        if (Input.GetMouseButtonDown(1))
        {
            CancelGhost();
            return;
        }

        // If currently dragging an existing placed object (picked up), follow mouse
        if (isDraggingExisting && draggingObject != null)
        {
            UpdateGhostFromDragging();
            if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
            {
                TryPlaceDraggedObject();
            }
            return;
        }

        // If there's no selected prefab, do nothing
        if (placePrefab == null)
        {
            CancelGhost();
            return;
        }

        // Ensure ghost exists
        if (ghost == null) CreateGhost();

        // Move ghost to grid cell under mouse
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector2Int cell = grid.WorldToCell(mouseWorld);

        // Use the ghost's size to interpret baseCell such that mouse points to desired base location.
        Vector2Int size = new Vector2Int(ghostPlaceable.sizeX, ghostPlaceable.sizeY);

        // We'll snap baseCell so that the ghost's bottom-left aligns to the clicked cell.
        Vector2Int baseCell = cell;

        // Check area free
        bool canPlace = grid.IsAreaFree(baseCell, size);

        // Color ghost accordingly
        var sr = ghost.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = canPlace ? new Color(0f, 1f, 0f, 0.6f) : new Color(1f, 0f, 0f, 0.6f);

        // set ghost position/scale
        ghostPlaceable.baseCell = baseCell;
        ghostPlaceable.AlignToCell(grid);

        // Click to place
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
        {
            if (canPlace)
            {
                // instantiate actual object from prefab at correct transform
                var real = Instantiate(placePrefab);
                var p = real.GetComponent<Placeable>();
                p.baseCell = baseCell;
                if (!grid.PlaceObjectAt(real, baseCell, new Vector2Int(p.sizeX, p.sizeY)))
                {
                    Destroy(real);
                    Debug.LogWarning("Unexpected: failed to place even though ghost said free.");
                }
                else
                {
                    p.isPlaced = true;
                    p.AlignToCell(grid);
                }
            }
            else
            {
                // feedback
                Debug.Log("Cannot place here - area occupied or out of bounds.");
            }
        }

        // Pick up placed object on click (if clicking on existing placed object)
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
        {
            // raycast check topmost placed object under mouse
            RaycastHit2D hit = Physics2D.Raycast(mouseWorld, Vector2.zero);
            if (hit.collider != null)
            {
                var picked = hit.collider.gameObject.GetComponent<Placeable>();
                if (picked != null && picked.isPlaced)
                {
                    // start dragging this existing object
                    StartDraggingExisting(picked.gameObject);
                    return;
                }
            }
        }
    }

    void CreateGhost()
    {
        ghost = Instantiate(placePrefab);
        ghost.name = "GHOST_" + placePrefab.name;
        DestroyImmediate(ghost.GetComponent<Collider2D>()); // remove collider if any
        ghostPlaceable = ghost.GetComponent<Placeable>();
        // make transparent & not interactable
        var sr = ghost.GetComponent<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = 1000;
        // disable script behaviors if any
        foreach (var mb in ghost.GetComponents<MonoBehaviour>()) mb.enabled = false;
    }

    void CancelGhost()
    {
        if (ghost != null) Destroy(ghost);
        ghost = null;
        ghostPlaceable = null;
        if (isDraggingExisting)
        {
            // if cancel dragging, put object back to its old place (we removed occupancy on pickup)
            if (draggingObject != null)
            {
                var p = draggingObject.GetComponent<Placeable>();
                if (p != null)
                {
                    // try to re-place where baseCell currently is (should be old baseCell)
                    grid.PlaceObjectAt(draggingObject, p.baseCell, new Vector2Int(p.sizeX, p.sizeY));
                    p.isPlaced = true;
                    p.AlignToCell(grid);
                }
            }
            isDraggingExisting = false;
            draggingObject = null;
        }
    }

    void StartDraggingExisting(GameObject obj)
    {
        // remove occupancy so area becomes free while moving
        grid.RemoveObject(obj);
        var p = obj.GetComponent<Placeable>();
        if (p != null) p.isPlaced = false;
        draggingObject = obj;
        isDraggingExisting = true;
        // create ghost visual from obj
        if (ghost != null) Destroy(ghost);
        ghost = Instantiate(obj);
        ghost.name = "DRAG_GHOST_" + obj.name;
        ghostPlaceable = ghost.GetComponent<Placeable>();
        // disable scripts on ghost
        foreach (var mb in ghost.GetComponents<MonoBehaviour>()) mb.enabled = false;
        var sr = ghost.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = new Color(0f, 0.5f, 1f, 0.6f);
    }

    void UpdateGhostFromDragging()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector2Int cell = grid.WorldToCell(mouseWorld);
        Vector2Int size = new Vector2Int(ghostPlaceable.sizeX, ghostPlaceable.sizeY);
        Vector2Int baseCell = cell;
        bool canPlace = grid.IsAreaFree(baseCell, size);
        var sr = ghost.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = canPlace ? new Color(0f, 1f, 0f, 0.6f) : new Color(1f, 0f, 0f, 0.6f);
        ghostPlaceable.baseCell = baseCell;
        ghostPlaceable.AlignToCell(grid);
    }

    void TryPlaceDraggedObject()
    {
        Vector2Int baseCell = ghostPlaceable.baseCell;
        Vector2Int size = new Vector2Int(ghostPlaceable.sizeX, ghostPlaceable.sizeY);
        if (grid.IsAreaFree(baseCell, size))
        {
            // place draggingObject
            var p = draggingObject.GetComponent<Placeable>();
            p.baseCell = baseCell;
            if (!grid.PlaceObjectAt(draggingObject, baseCell, size))
            {
                Debug.LogWarning("Unexpected fail to place dragged object.");
            }
            else
            {
                p.isPlaced = true;
                p.AlignToCell(grid);
            }
            // destroy ghost
            Destroy(ghost);
            ghost = null;
            ghostPlaceable = null;
            draggingObject = null;
            isDraggingExisting = false;
        }
        else
        {
            Debug.Log("Cannot place dragged object here.");
        }
    }

    bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}