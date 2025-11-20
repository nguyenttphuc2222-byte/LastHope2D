using System.Collections;
using UnityEngine;

public class BuildEffect : MonoBehaviour
{
    [Header("Pop Settings")]
    [SerializeField] private float popScaleMultiplier = 1.2f;
    [SerializeField] private float popDuration = 0.15f;

    private Vector3 originalScale;
    private Coroutine popRoutine;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void PlayPop()
    {
        if (popRoutine != null)
            StopCoroutine(popRoutine);

        popRoutine = StartCoroutine(PopCoroutine());
    }

    private IEnumerator PopCoroutine()
    {
        float half = popDuration * 0.5f;
        float t = 0f;
        Vector3 target = originalScale * popScaleMultiplier;

        // scale lên
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float lerp = t / half;
            transform.localScale = Vector3.Lerp(originalScale, target, lerp);
            yield return null;
        }

        // scale xuống lại
        t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float lerp = t / half;
            transform.localScale = Vector3.Lerp(target, originalScale, lerp);
            yield return null;
        }

        transform.localScale = originalScale;
        popRoutine = null;
    }
}
