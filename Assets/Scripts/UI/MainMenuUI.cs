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
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameplayMusic();
        }
    }

    public void OnClickOpenSetting()
    {
        // 👉 Load thẳng scene Setting, không overlay
        Time.timeScale = 1f;
        SceneManager.LoadScene(settingSceneName);
    }
}
