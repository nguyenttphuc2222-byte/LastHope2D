using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public Transform fill;   // gán object Fill ở Inspector

    private Vector3 originalScale;

    private void Awake()
    {
        if (fill != null)
            originalScale = fill.localScale;
    }

    public void SetValue(float normalized)
    {
        normalized = Mathf.Clamp01(normalized);
        if (fill != null)
        {
            fill.localScale = new Vector3(
                originalScale.x * normalized,
                originalScale.y,
                originalScale.z
            );
        }
    }
}
