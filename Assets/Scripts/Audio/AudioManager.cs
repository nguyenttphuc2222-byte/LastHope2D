using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;   // nhạc nền
    public AudioSource sfxSource;     // toàn bộ SFX

    [Header("Default Music Clips")]
    public AudioClip menuMusic;       // nhạc nền menu (optional)
    public AudioClip gameplayMusic;   // nhạc nền trong MapTest (optional)

    [Header("Volumes (0-1)")]
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("Mute States")]
    public bool musicMuted = false;
    public bool sfxMuted = false;

    // PlayerPrefs keys
    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SfxVolume";
    private const string MusicMutedKey = "MusicMuted";
    private const string SfxMutedKey = "SfxMuted";

    public float MusicVolume => musicVolume;
    public float SfxVolume => sfxVolume;
    public bool MusicMuted => musicMuted;
    public bool SfxMuted => sfxMuted;

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource == null)
            musicSource = GetComponents<AudioSource>()[0];

        if (sfxSource == null)
        {
            var sources = GetComponents<AudioSource>();
            if (sources.Length > 1) sfxSource = sources[1];
            else sfxSource = gameObject.AddComponent<AudioSource>();
        }

        musicSource.loop = true;
        musicSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        LoadSettings();
        ApplyVolumes();

        // CHỖ QUAN TRỌNG: bắt buộc phát nhạc menu nếu có clip
        if (menuMusic != null)
        {
            PlayMusicLoop(menuMusic, true);
            Debug.Log("AudioManager: Play menu music");
        }
        else
        {
            Debug.LogWarning("AudioManager: menuMusic chưa được gán!");
        }
    }



    //Load / Save

    private void LoadSettings()
    {
        // Nếu chưa có key thì mặc định = 1
        musicVolume = PlayerPrefs.HasKey(MusicVolumeKey)
            ? PlayerPrefs.GetFloat(MusicVolumeKey)
            : 1f;

        sfxVolume = PlayerPrefs.HasKey(SfxVolumeKey)
            ? PlayerPrefs.GetFloat(SfxVolumeKey)
            : 1f;

        musicMuted = PlayerPrefs.GetInt(MusicMutedKey, 0) == 1;
        sfxMuted = PlayerPrefs.GetInt(SfxMutedKey, 0) == 1;

        // đảm bảo luôn trong khoảng 0..1
        musicVolume = Mathf.Clamp01(musicVolume);
        sfxVolume = Mathf.Clamp01(sfxVolume);
    }


    private void SaveSettings()
    {
        PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
        PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);

        PlayerPrefs.SetInt(MusicMutedKey, musicMuted ? 1 : 0);
        PlayerPrefs.SetInt(SfxMutedKey, sfxMuted ? 1 : 0);
    }

    private void ApplyVolumes()
    {
        float musicEff = musicMuted ? 0f : musicVolume;
        float sfxEff = sfxMuted ? 0f : sfxVolume;

        if (musicSource != null) musicSource.volume = musicEff;
        if (sfxSource != null) sfxSource.volume = sfxEff;
    }

    //API cho volume / mute

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        SaveSettings();
        ApplyVolumes();
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        SaveSettings();
        ApplyVolumes();
    }

    public void ToggleMusicMute()
    {
        musicMuted = !musicMuted;
        SaveSettings();
        ApplyVolumes();
    }

    public void ToggleSfxMute()
    {
        sfxMuted = !sfxMuted;
        SaveSettings();
        ApplyVolumes();
    }

    // API cho phát nhạc / SFX

    public void PlayMusicLoop(AudioClip clip, bool forceRestart = false)
    {
        if (clip == null || musicSource == null) return;

        if (!forceRestart && musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayGameplayMusic()
    {
        if (gameplayMusic != null)
            PlayMusicLoop(gameplayMusic);
    }

    public void PlayMenuMusic()
    {
        if (menuMusic != null)
            PlayMusicLoop(menuMusic);
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }
}
