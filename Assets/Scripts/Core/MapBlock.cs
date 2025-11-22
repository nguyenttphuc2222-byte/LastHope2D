using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class MapBlock : MonoBehaviour
{
    [Tooltip("Nếu có BuildingBase thì dùng size của nó để block nhiều ô (ví dụ EnemyCore 2x2).")]
    public bool useBuildingSize = false;

    private void Start()
    {
        var gm = GridManager.Instance;
        if (gm == null)
        {
            Debug.LogError("MapBlock: No GridManager found in scene.");
            return;
        }

        // Ô gốc (anchor) tính từ vị trí hiện tại
        Vector2Int baseCell = gm.WorldToCell(transform.position);

        // Nếu bật useBuildingSize và có BuildingBase -> block theo size
        BuildingBase building = useBuildingSize ? GetComponent<BuildingBase>() : null;
        if (building != null)
        {
            Vector2Int size = building.size;

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Vector2Int c = new Vector2Int(baseCell.x + x, baseCell.y + y);
                    gm.SetTerrainBlocked(c, true);
                }
            }
        }
        else
        {
            // Trường hợp block 1 ô như Block thường
            gm.SetTerrainBlocked(baseCell, true);
        }

        // Snap object vào tâm ô gốc cho gọn (tạm thời, đủ dùng)
        transform.position = gm.CellToWorldCenter(baseCell);
    }
}
