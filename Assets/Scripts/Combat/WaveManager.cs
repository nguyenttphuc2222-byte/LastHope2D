using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaveManager : MonoBehaviour
{
    public EnemySpawner[] spawners;

    [Header("Wave settings")]
    public float waveDuration = 25f;         // mỗi wave dài bao lâu (giây)
    public int baseEnemiesPerWave = 1;
    public float baseSpawnInterval = 5f;

    public float spawnIntervalDecreasePerWave = 0.5f;
    public int enemiesIncreasePerWave = 1;

    [Header("UI")]
    public TextMeshProUGUI waveText;                    // UI Text để hiển thị "Wave X"

    private int currentWave = 1;
    public int CurrentWave => currentWave;
    private float waveTimer;

    private void Start()
    {
        waveTimer = waveDuration;
        ApplyWaveSettings();
        UpdateWaveUI();
    }

    private void Update()
    {
        waveTimer -= Time.deltaTime;
        if (waveTimer <= 0f)
        {
            currentWave++;
            waveTimer = waveDuration;
            ApplyWaveSettings();
            UpdateWaveUI();
        }
    }

    private void ApplyWaveSettings()
    {
        if (spawners == null) return;

        foreach (var spawner in spawners)
        {
            if (spawner == null) continue;

            spawner.enemiesPerWave = baseEnemiesPerWave + enemiesIncreasePerWave * (currentWave - 1);

            float newInterval = baseSpawnInterval - spawnIntervalDecreasePerWave * (currentWave - 1);
            spawner.spawnInterval = Mathf.Max(1f, newInterval); // không nhỏ hơn 1s
        }

        Debug.Log($"Wave {currentWave} bắt đầu. enemiesPerWave = {baseEnemiesPerWave + enemiesIncreasePerWave * (currentWave - 1)}");
    }

    private void UpdateWaveUI()
    {
        if (waveText != null)
        {
            waveText.text = $"Wave {currentWave}";
        }
    }
}
