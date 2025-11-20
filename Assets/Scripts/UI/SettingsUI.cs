using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsUI : MonoBehaviour
{
    [Header("Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Music Mute Button")]
    public Button musicMuteButton;
    public Image musicMuteIcon;
    public Sprite musicOnSprite;
    public Sprite musicOffSprite;

    [Header("SFX Mute Button")]
    public Button sfxMuteButton;
    public Image sfxMuteIcon;
    public Sprite sfxOnSprite;
    public Sprite sfxOffSprite;

    [Header("Navigation")]
    public string backSceneName = "MainMenu";

    // Lưu lại volume khác 0 lần cuối để khi unmute thì trả về
    float lastMusicVolume = 1f;
    float lastSfxVolume = 1f;

    private void Start()
    {
        var am = AudioManager.Instance;
        if (am == null)
        {
            Debug.LogWarning("SettingsUI: Không tìm thấy AudioManager.");
            return;
        }

        // đảm bảo slider range 0..1
        if (musicSlider != null)
        {
            musicSlider.minValue = 0f;
            musicSlider.maxValue = 1f;
            musicSlider.SetValueWithoutNotify(am.MusicVolume);
            lastMusicVolume = am.MusicVolume > 0f ? am.MusicVolume : 1f;
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;
            sfxSlider.SetValueWithoutNotify(am.SfxVolume);
            lastSfxVolume = am.SfxVolume > 0f ? am.SfxVolume : 1f;
        }

        UpdateMuteIcons();
    }

    // ----- Slider callback -----

    public void OnMusicSliderChanged(float value)
    {
        var am = AudioManager.Instance;
        if (am == null) return;

        am.SetMusicVolume(value);

        // Nếu >0 thì nhớ lại volume này để sau unmute quay về
        if (value > 0f)
            lastMusicVolume = value;

        // 1) Nếu kéo về 0 -> bật trạng thái mute
        if (value <= 0.0001f)
        {
            if (!am.MusicMuted)
                am.ToggleMusicMute();
        }
        else // > 0 -> tự unmute nếu đang mute
        {
            if (am.MusicMuted)
                am.ToggleMusicMute();
        }

        UpdateMuteIcons();
    }

    public void OnSfxSliderChanged(float value)
    {
        var am = AudioManager.Instance;
        if (am == null) return;

        am.SetSfxVolume(value);

        if (value > 0f)
            lastSfxVolume = value;

        if (value <= 0.0001f)
        {
            if (!am.SfxMuted)
                am.ToggleSfxMute();
        }
        else
        {
            if (am.SfxMuted)
                am.ToggleSfxMute();
        }

        UpdateMuteIcons();
    }

    // ----- Mute button callback -----

    public void OnClickMusicMute()
    {
        var am = AudioManager.Instance;
        if (am == null) return;

        am.ToggleMusicMute();

        if (am.MusicMuted)
        {
            // 2) Khi vừa chuyển sang mute -> kéo slider về 0 luôn
            if (musicSlider != null)
                musicSlider.SetValueWithoutNotify(0f);

            am.SetMusicVolume(0f);
        }
        else
        {
            // Khi unmute -> trả slider về volume cũ (lastMusicVolume)
            if (musicSlider != null)
                musicSlider.SetValueWithoutNotify(lastMusicVolume);

            am.SetMusicVolume(lastMusicVolume);
        }

        UpdateMuteIcons();
    }

    public void OnClickSfxMute()
    {
        var am = AudioManager.Instance;
        if (am == null) return;

        am.ToggleSfxMute();

        if (am.SfxMuted)
        {
            if (sfxSlider != null)
                sfxSlider.SetValueWithoutNotify(0f);

            am.SetSfxVolume(0f);
        }
        else
        {
            if (sfxSlider != null)
                sfxSlider.SetValueWithoutNotify(lastSfxVolume);

            am.SetSfxVolume(lastSfxVolume);
        }

        UpdateMuteIcons();
    }

    // ----- Icon -----

    private void UpdateMuteIcons()
    {
        var am = AudioManager.Instance;
        if (am == null) return;

        if (musicMuteIcon != null)
            musicMuteIcon.sprite = am.MusicMuted ? musicOffSprite : musicOnSprite;

        if (sfxMuteIcon != null)
            sfxMuteIcon.sprite = am.SfxMuted ? sfxOffSprite : sfxOnSprite;
    }

    // ----- Back -----

    public void OnClickBack()
    {
        // Nếu đang mở dạng overlay từ MapTest
        if (SettingsOverlay.IsOpen && GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.ShowPauseMenuAfterSettings();
            SettingsOverlay.Close();
        }
        else
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(backSceneName);
        }
    }
}
