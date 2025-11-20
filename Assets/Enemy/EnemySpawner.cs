using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy prefabs (chọn random 1 cái khi spawn)")]
    public EnemyBasic[] enemyPrefabs;

    [Header("Spawn settings")]
    public float spawnInterval = 5f;
    public int enemiesPerWave;
    public bool autoStart = true;

    private float timer;
    private bool spawning;

    private void Start()
    {
        spawning = autoStart;
        timer = spawnInterval;
    }

    private void Update()
    {
        if (!spawning) return;
        if (CoreBuilding.Instance == null) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = spawnInterval;
            SpawnWave();
        }
    }

    public void StartSpawning()
    {
        spawning = true;
        timer = spawnInterval;
    }

    public void StopSpawning()
    {
        spawning = false;
    }

    private void SpawnWave()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("EnemySpawner: chưa gán enemyPrefabs.");
            return;
        }

        for (int i = 0; i < enemiesPerWave; i++)
        {
            EnemyBasic prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            Vector3 pos = transform.position;
            pos += new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), 0f);

            Instantiate(prefab, pos, Quaternion.identity);
        }
    }
}
