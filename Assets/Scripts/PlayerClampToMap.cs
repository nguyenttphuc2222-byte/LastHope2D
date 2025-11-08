using UnityEngine;

public class PlayerClampToMap : MonoBehaviour
{
    private void LateUpdate()
    {
        if (GridManager.Instance == null) return;

        Rect bounds = GridManager.Instance.GetWorldBounds();
        Vector3 pos = transform.position;

        float minX = bounds.xMin;
        float maxX = bounds.xMax;
        float minY = bounds.yMin;
        float maxY = bounds.yMax;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        transform.position = pos;
    }
}
