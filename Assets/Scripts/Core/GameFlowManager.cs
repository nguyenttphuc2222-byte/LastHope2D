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

    [Header("Restart Confirm")]
    [Tooltip("Panel nhỏ hỏi: 'Restart và xoá save? Y / N'")]
    public GameObject restartConfirmPanel;

    private bool isGameOver = false;              // dùng chung cho cả win & lose
    public bool IsGameOver => isGameOver;

    private bool isWin = false;
    public bool IsWin => isWin;

    private bool isPaused = false;
    public bool IsPaused => isPaused;

    public string settingSceneName = "Setting";

    // Đếm thời gian chơi từ lúc vào MapTest (không tính thời gian pause)

    private float elapsedTime = 0f;
    public float ElapsedTime
    {
        get => elapsedTime;
        set => elapsedTime = Mathf.Max(0f, value);
    }

    // Trạng thái có đang mở bảng confirm restart không?
    private bool isRestartConfirmOpen = false;

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

        if (restartConfirmPanel != null)
            restartConfirmPanel.SetActive(false);

        UpdatePauseButtonLabel();
    }

    private void Update()
    {
        // Nếu đang mở bảng xác nhận Restart -> chỉ nhận Y / N, bỏ qua logic khác
        if (isRestartConfirmOpen)
        {
            HandleRestartConfirmInput();
            return;
        }

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

    public void PauseAndShowMenu()
    {
        if (isGameOver) return;

        Pause();    // dùng hàm Pause() bạn đã có
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
    }

    public void OnClickSaveAndExit()
    {
        if (isGameOver) return;

        // Dừng thời gian trong khi save
        Time.timeScale = 0f;

        SaveSystem.SaveGame();

        // Về MainMenu
        SceneManager.LoadScene(homeSceneName);
    }

    // Gán hàm này cho nút Restart trong PauseMenuPanel
    public void OnClickRestartFromPause()
    {
        if (isGameOver) return;

        isRestartConfirmOpen = true;

        if (restartConfirmPanel != null)
            restartConfirmPanel.SetActive(true);
    }


    private void HandleRestartConfirmInput()
    {
        // Y = đồng ý restart + xoá save
        if (Input.GetKeyDown(KeyCode.Y))
        {
            ConfirmRestart();
        }
        // N hoặc Esc = huỷ
        else if (Input.GetKeyDown(KeyCode.N) || Input.GetKeyDown(KeyCode.Escape))
        {
            CancelRestart();
        }
    }

    private void ConfirmRestart()
    {
        isRestartConfirmOpen = false;

        if (restartConfirmPanel != null)
            restartConfirmPanel.SetActive(false);

        // Xoá file save (nếu có)
        SaveSystem.DeleteSave();

        // Reset tạm (dù load scene xong cũng reset lại hết)
        isGameOver = false;
        isWin = false;
        isPaused = false;
        Time.timeScale = 1f;

        // Load lại scene hiện tại
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.name);
    }

    private void CancelRestart()
    {
        isRestartConfirmOpen = false;

        if (restartConfirmPanel != null)
            restartConfirmPanel.SetActive(false);
    }



    // ---------- LOSE: CORE PLAYER BỊ PHÁ ----------

    public void OnCoreDestroyed()
    {
        if (isGameOver) return;
        isGameOver = true;
        isWin = false;

        SaveSystem.DeleteSave();
        SaveSystem.ClearLoadRequest();

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

        SaveSystem.DeleteSave();
        SaveSystem.ClearLoadRequest();

        // tắt pause menu nếu đang mở
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        isPaused = false;
        UpdatePauseButtonLabel();

        // Lấy số wave hiện tại
        WaveManager wm = FindFirstObjectByType<WaveManager>();
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
