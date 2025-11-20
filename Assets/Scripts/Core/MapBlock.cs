using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class MapBlock : MonoBehaviour
{
    private void Start()
    {
        if (GridManager.Instance == null)
        {
            Debug.LogError("MapBlock: No GridManager found in scene.");
            return;
        }

        Vector2Int cell = GridManager.Instance.WorldToCell(transform.position);
        GridManager.Instance.SetTerrainBlocked(cell, true);

        // Option: snap block vào đúng tâm ô grid
        transform.position = GridManager.Instance.CellToWorldCenter(cell);
    }
}
