using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSfx : MonoBehaviour
{
    [Header("SFX")]
    public AudioClip clickSfx;
    [Range(0f, 1f)] public float volumeMultiplier = 1f;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(PlayClickSfx);
    }

    private void OnDestroy()
    {
        // Dọn listener khi object bị destroy cho sạch
        if (button != null)
            button.onClick.RemoveListener(PlayClickSfx);
    }

    private void PlayClickSfx()
    {
        if (clickSfx == null || AudioManager.Instance == null)
            return;

        // Nếu AudioManager của bạn ĐÃ có hàm overload:
        // public void PlaySfx(AudioClip clip, float volumeMultiplier = 1f)
        AudioManager.Instance.PlaySfx(clickSfx, volumeMultiplier);

        // Nếu project bạn hiện tại CHỈ có hàm:
        // public void PlaySfx(AudioClip clip)
        // thì đổi dòng trên thành:
        // AudioManager.Instance.PlaySfx(clickSfx);
    }
}
