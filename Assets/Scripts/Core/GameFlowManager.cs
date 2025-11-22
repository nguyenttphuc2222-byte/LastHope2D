using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    [Header("Game Over (Lose)")]
    public GameObject gameOverPanel;

    [Header("Win")]
    public GameObject winPanel;
    public TextMeshProUGUI winWaveText;
    public TextMeshProUGUI winTimeText;

    [Header("Pause")]
    public GameObject pauseMenuPanel;             // panel chứa Pause & Setting
    public TextMeshProUGUI pauseButtonLabel;      // text trên nút Pause/Resume

    [Header("Scenes")]
    public string homeSceneName = "MainMenu";

    private bool isGameOver = false;              // dùng chung cho cả win & lose
    public bool IsGameOver => isGameOver;

    private bool isWin = false;
    public bool IsWin => isWin;

    private bool isPaused = false;
    public bool IsPaused => isPaused;

    public string settingSceneName = "Setting";

    // Đếm thời gian chơi từ lúc vào MapTest (không tính thời gian pause)
    private float elapsedTime = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Time.timeScale = 1f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (winPanel != null)
            winPanel.SetActive(false);

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        UpdatePauseButtonLabel();
    }

    private void Update()
    {
        // Tăng timer nếu game đang chạy
        if (!isGameOver && !isPaused)
        {
            elapsedTime += Time.deltaTime;
        }

        // ESC để Pause / Resume
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnEscPressed();
        }
    }

    // ---------- LOSE: CORE PLAYER BỊ PHÁ ----------

    public void OnCoreDestroyed()
    {
        if (isGameOver) return;
        isGameOver = true;
        isWin = false;

        // tắt pause menu nếu đang mở
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        isPaused = false;
        UpdatePauseButtonLabel();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    // ---------- WIN: ENEMY CORE BỊ PHÁ ----------

    public void OnEnemyCoreDestroyed()
    {
        if (isGameOver) return;
        isGameOver = true;
        isWin = true;

        // tắt pause menu nếu đang mở
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        isPaused = false;
        UpdatePauseButtonLabel();

        // Lấy số wave hiện tại
        WaveManager wm = FindObjectOfType<WaveManager>();
        int waves = wm != null ? wm.CurrentWave : 0;

        if (winWaveText != null)
        {
            winWaveText.text = $"You survived {waves} waves";
        }

        // Format thời gian mm:ss
        if (winTimeText != null)
        {
            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);
            winTimeText.text = $"Time: {minutes:00}:{seconds:00}";
        }

        if (winPanel != null)
            winPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    // ---------- NÚT RESTART / BACK HOME ----------

    public void RestartGame()
    {
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.name);
    }

    public void BackToHome()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(homeSceneName);
    }

    // ---------- PAUSE / RESUME ----------

    private void OnEscPressed()
    {
        if (isGameOver) return;

        if (!isPaused)
        {
            Pause();
            // khi pause bằng ESC thì tự mở menu
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(true);
        }
        else
        {
            Resume();
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);
        }
    }

    public void TogglePauseMenuPanel()
    {
        if (isGameOver || pauseMenuPanel == null) return;

        bool active = !pauseMenuPanel.activeSelf;
        pauseMenuPanel.SetActive(active);
    }

    public void OnClickPauseOrResume()
    {
        if (isGameOver) return;

        if (isPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        if (isGameOver || isPaused) return;

        isPaused = true;
        Time.timeScale = 0f;
        UpdatePauseButtonLabel();
    }

    public void Resume()
    {
        if (!isPaused) return;

        isPaused = false;
        Time.timeScale = 1f;
        UpdatePauseButtonLabel();
    }

    private void UpdatePauseButtonLabel()
    {
        if (pauseButtonLabel == null) return;

        pauseButtonLabel.text = isPaused ? "Resume (ESC)" : "Pause (ESC)";
    }

    // ---------- MỞ SETTINGS OVERLAY ----------

    public void OnClickOpenSetting()
    {
        if (isGameOver) return;

        // Đảm bảo game đang ở trạng thái pause
        if (!isPaused)
        {
            Pause();
        }

        // Ẩn pause menu cho gọn UI
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        // Mở Setting overlay (script SettingsOverlay bạn đã có sẵn)
        SettingsOverlay.Open();
    }

    // Gọi khi từ Setting quay lại MapTest
    public void ShowPauseMenuAfterSettings()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        UpdatePauseButtonLabel();
    }
}
