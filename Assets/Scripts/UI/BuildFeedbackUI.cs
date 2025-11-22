using UnityEngine;
using TMPro;

public class BuildFeedbackUI : MonoBehaviour
{
    public static BuildFeedbackUI Instance { get; private set; }

    [Header("Message")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float messageDuration = 1f;
    [SerializeField] private Vector2 screenOffset = new Vector2(0f, 30f);

    private CanvasGroup canvasGroup;
    private RectTransform messageRect;
    private float startTime;
    private bool isShowing;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (messageText == null)
        {
            Debug.LogWarning("BuildFeedbackUI: Chưa gán messageText.");
            return;
        }

        canvasGroup = messageText.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = messageText.gameObject.AddComponent<CanvasGroup>();
        }

        messageRect = messageText.GetComponent<RectTransform>();
        canvasGroup.alpha = 0f;
    }

    private void Update()
    {
        if (!isShowing || canvasGroup == null) return;

        float elapsed = Time.unscaledTime - startTime;
        float t = Mathf.Clamp01(elapsed / messageDuration);
        canvasGroup.alpha = 1f - t;

        if (elapsed >= messageDuration)
        {
            canvasGroup.alpha = 0f;
            isShowing = false;
        }
    }

    public void ShowMessage(string text, Vector2 mouseScreenPosition)
    {
        if (messageText == null || canvasGroup == null || messageRect == null)
            return;

        messageText.text = text;

        // đặt text gần con trỏ
        RectTransform canvasRect = messageText.canvas.GetComponent<RectTransform>();
        Vector2 localPoint;
        Vector2 posWithOffset = mouseScreenPosition + screenOffset;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            posWithOffset,
            messageText.canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : messageText.canvas.worldCamera,
            out localPoint
        );

        messageRect.anchoredPosition = localPoint;

        startTime = Time.unscaledTime;
        canvasGroup.alpha = 1f;
        isShowing = true;
    }

    public void ShowNotEnoughOre(Vector2 mouseScreenPosition)
    {
        ShowMessage("Not enough ore", mouseScreenPosition);
    }

}
