using UnityEngine.SceneManagement;

public static class SettingsOverlay
{
    // Tên scene Setting trong Build Settings
    private const string SettingsSceneName = "Setting";

    private static string previousSceneName;
    private static bool overlayOpen;

    public static bool IsOpen => overlayOpen;

    // Mở scene Setting dạng overlay (additive)
    public static void Open()
    {
        if (overlayOpen) return;

        overlayOpen = true;
        previousSceneName = SceneManager.GetActiveScene().name;

        // Load Setting chồng thêm lên scene hiện tại
        SceneManager.LoadScene(SettingsSceneName, LoadSceneMode.Additive);

        // Đặt scene Setting làm active để UI, EventSystem hoạt động ở đó
        var settingScene = SceneManager.GetSceneByName(SettingsSceneName);
        if (settingScene.IsValid())
        {
            SceneManager.SetActiveScene(settingScene);
        }
    }

    // Đóng overlay Setting, quay về scene cũ
    public static void Close()
    {
        if (!overlayOpen) return;

        overlayOpen = false;

        // Unload scene Setting
        SceneManager.UnloadSceneAsync(SettingsSceneName);

        // Đặt lại active scene về scene trước đó
        if (!string.IsNullOrEmpty(previousSceneName))
        {
            var prevScene = SceneManager.GetSceneByName(previousSceneName);
            if (prevScene.IsValid())
            {
                SceneManager.SetActiveScene(prevScene);
            }
        }
    }
}
