using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    [Header("Game Over")]
    public GameObject gameOverPanel;

    [Header("Pause")]
    public GameObject pauseMenuPanel;             // panel chứa Pause & Setting
    public TextMeshProUGUI pauseButtonLabel;      // text trên nút Pause/Resume


    [Header("Scenes")]
    public string homeSceneName = "MainMenu";

    private bool isGameOver = false;
    public bool IsGameOver => isGameOver;

    private bool isPaused = false;
    public bool IsPaused => isPaused;

    public string settingSceneName = "Setting";

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

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        UpdatePauseButtonLabel();
    }

    private void Update()
    {
        // ESC để Pause / Resume
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnEscPressed();
        }
    }

    // ---------- GAME OVER ----------

    public void OnCoreDestroyed()
    {
        if (isGameOver) return;
        isGameOver = true;

        // tắt pause menu nếu đang mở
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        isPaused = false;
        UpdatePauseButtonLabel();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }

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

    // PAUSE / RESUME

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


    public void OnClickOpenSetting()
    {
        if (isGameOver) return;

        // Đảm bảo game đang ở trạng thái pause
        if (!isPaused)
        {
            Pause();   // hàm Pause() bạn đã có sẵn
        }

        // Ẩn pause menu cho gọn UI
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        // Mở Setting overlay
        SettingsOverlay.Open();
    }


    public void ShowPauseMenuAfterSettings()
    {
        // Được gọi khi từ Setting quay lại MapTest
        isPaused = true;
        Time.timeScale = 0f;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        UpdatePauseButtonLabel();
    }

}
