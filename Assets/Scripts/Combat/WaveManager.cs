using UnityEngine;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class WaveEnemyGroup
{
    public EnemyBasic enemyPrefab;
    public int count;
}

[System.Serializable]
public class WaveConfig
{
    [Header("Cấu hình Wave")]
    public List<WaveEnemyGroup> enemyGroups;
    public float spawnInterval = 1f;
    public bool shuffle = false;
}

public class WaveManager : MonoBehaviour
{
    public EnemySpawner[] spawners;

    [Header("--- WAVE CONFIG (Wave 1 -> X) ---")]
    public List<WaveConfig> waves;

    [Header("--- ENDLESS MODE (Wave X+1) ---")]
    public EnemyBasic basicEnemyPrefab;
    public EnemyBasic fastEnemyPrefab;
    public EnemyBasic tankEnemyPrefab;
    public EnemyBasic rangedEnemyPrefab; // <-- MỚI: Thêm slot cho Ranged Enemy

    [Header("Settings chung")]
    public float waveDuration = 20f;
    public float timeBetweenWaves = 5f;

    [Header("Độ khó (Scaling)")]
    public int baseEnemiesPerWave = 5;
    public int enemiesIncreasePerWave = 2;

    public float baseSpawnInterval = 3f;
    public float spawnIntervalDecreasePerWave = 0.1f;
    public float minSpawnInterval = 0.3f;


    public int CurrentWave => currentWave;
    public bool IsWaveActive => isWaveActive;
    public float CurrentTimer => timer;


    [Tooltip("Mỗi wave quái sẽ mạnh thêm bao nhiêu %? (0.1 = 10%)")]
    public float statIncreasePerWave = 0.1f;

    [Header("Audio")]
    public AudioClip waveStartSound;
    [Range(0f, 1f)] public float soundVolume = 1.0f;

    [Header("UI")]
    public TextMeshProUGUI waveText;

    private int currentWave = 1;
    
    private float timer;
    private bool isWaveActive = false;

    private void Start()
    {
        if (SaveSystem.LoadOnNextScene && SaveSystem.HasSaveFile())
        {
            // SaveApplier sẽ tự gọi LoadFromSave, nên không start wave ở đây
            return;
        }

        StartWave();
    }


    private void Update()
    {
        timer -= Time.deltaTime;

        if (isWaveActive)
        {
            if (timer <= 0f) StartDelay();
        }
        else
        {
            if (waveText != null)
            {
                int secondsLeft = Mathf.CeilToInt(timer);
                waveText.text = $"Wave {currentWave + 1} incoming in {secondsLeft}...";
            }

            if (timer <= 0f)
            {
                currentWave++;
                StartWave();
            }
        }
    }

    // Gọi từ SaveApplier
    public void LoadFromSave(int wave, bool active, float t)
    {
        currentWave = Mathf.Max(1, wave);
        isWaveActive = active;
        timer = t;

        // Không auto StartWave() ở đây, vì enemies sẽ được sinh tiếp
        // bằng logic có sẵn trong Update().
        UpdateWaveUI_Fighting();
    }

    private void StartWave()
    {
        isWaveActive = true;
        timer = waveDuration;

        if (waveStartSound != null)
        {
            if (AudioManager.Instance != null)
            {
                // tiếng bắt đầu Wave cũng là SFX
                AudioManager.Instance.PlaySfx(waveStartSound, soundVolume);
            }
            else
            {
                Vector3 playPos = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
                playPos.z = 0f;
                AudioSource.PlayClipAtPoint(waveStartSound, playPos, soundVolume);
            }
        }


        List<EnemyBasic> enemiesToSpawn = GenerateEnemyList();
        float interval = GetCurrentInterval();
        float multiplier = 1f + ((currentWave - 1) * statIncreasePerWave);

        foreach (var spawner in spawners)
        {
            if (spawner != null)
            {
                spawner.StartSpawningWave(enemiesToSpawn, interval, multiplier);
            }
        }

        UpdateWaveUI_Fighting();
        Debug.Log($"--- START WAVE {currentWave} ---");
    }

    private void StartDelay()
    {
        isWaveActive = false;
        timer = timeBetweenWaves;
        Debug.Log("Wave ended. Resting...");
    }

    private List<EnemyBasic> GenerateEnemyList()
    {
        List<EnemyBasic> list = new List<EnemyBasic>();

        // CASE 1: Config
        if (currentWave <= waves.Count)
        {
            WaveConfig config = waves[currentWave - 1];
            foreach (var group in config.enemyGroups)
            {
                for (int i = 0; i < group.count; i++) list.Add(group.enemyPrefab);
            }
            if (config.shuffle) Shuffle(list);
        }
        // CASE 2: Endless (Đã cập nhật Ranged)
        else
        {
            int totalCount = baseEnemiesPerWave + (currentWave * enemiesIncreasePerWave);
            for (int i = 0; i < totalCount; i++)
            {
                int rand = Random.Range(0, 100);

                // Cập nhật tỷ lệ spawn cho 4 loại quái:
                // 35% Fast (Spam nhiều)
                // 25% Tank (Đỡ đạn)
                // 25% Ranged (Bắn tỉa)
                // 15% Basic (Lấp chỗ trống)

                if (rand < 35) list.Add(fastEnemyPrefab);      // 0-34
                else if (rand < 60) list.Add(tankEnemyPrefab); // 35-59
                else if (rand < 85) list.Add(rangedEnemyPrefab); // 60-84 <-- MỚI
                else list.Add(basicEnemyPrefab);               // 85-99
            }
        }
        return list;
    }

    private float GetCurrentInterval()
    {
        if (currentWave <= waves.Count) return waves[currentWave - 1].spawnInterval;

        float calculatedInterval = baseSpawnInterval - (currentWave * spawnIntervalDecreasePerWave);
        return Mathf.Max(minSpawnInterval, calculatedInterval);
    }

    private void UpdateWaveUI_Fighting()
    {
        if (waveText != null) waveText.text = $"Wave {currentWave}";
    }

    private void Shuffle(List<EnemyBasic> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            EnemyBasic value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}