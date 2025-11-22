using UnityEngine;

[DisallowMultipleComponent]
public class BuildingSelectionHighlight : MonoBehaviour
{
    [Header("Highlight")]
    [Tooltip("SpriteRenderer dùng làm viền / overlay khi được chọn.")]
    public SpriteRenderer highlightRenderer;

    [Tooltip("Màu highlight khi được chọn.")]
    public Color highlightColor = new Color(0f, 1f, 1f, 0.4f);

    private Color originalColor;

    private void Awake()
    {
        if (highlightRenderer == null)
        {
            // Tìm child tên "Highlight" trước, nếu không thì lấy SpriteRenderer con đầu tiên
            Transform child = transform.Find("Highlight");
            if (child != null)
                highlightRenderer = child.GetComponent<SpriteRenderer>();
            if (highlightRenderer == null)
                highlightRenderer = GetComponentInChildren<SpriteRenderer>(true);
        }

        if (highlightRenderer != null)
        {
            originalColor = highlightRenderer.color;
            highlightRenderer.enabled = false; // tắt mặc định
        }
    }

    public void SetSelected(bool selected)
    {
        if (highlightRenderer == null) return;

        highlightRenderer.enabled = selected;
        highlightRenderer.color = selected ? highlightColor : originalColor;
    }

    // Cho MultiBuildingSelectionUI dùng tên cũ mà vẫn chạy được
    public void SetHighlighted(bool highlighted)
    {
        SetSelected(highlighted);
    }


}
