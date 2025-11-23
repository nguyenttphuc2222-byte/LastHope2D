using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public string gameSceneName = "MapTest";
    public string settingSceneName = "Setting";

    private void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMenuMusic();
        }
    }

    public void OnClickStartGame()
    {
        // Nếu có save thì yêu cầu MapTest load lại
        if (SaveSystem.HasSaveFile())
        {
            SaveSystem.RequestLoadOnNextScene();
        }
        else
        {
            SaveSystem.ClearLoadRequest();
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameplayMusic();
        }
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }

    public void OnClickOpenSetting()
    {
        // 👉 Load thẳng scene Setting, không overlay
        Time.timeScale = 1f;
        SceneManager.LoadScene(settingSceneName);
    }
}
