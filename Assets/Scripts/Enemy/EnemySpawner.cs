using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    private List<EnemyBasic> currentWaveEnemies = new List<EnemyBasic>();
    private float spawnInterval;
    private float timer;
    private bool isSpawning = false;

    // Biến lưu hệ số sức mạnh của Wave hiện tại
    private float currentBuffMultiplier = 1f;

    private void Update()
    {
        if (!isSpawning) return;

        if (currentWaveEnemies.Count == 0)
        {
            isSpawning = false;
            return;
        }

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnNextEnemy();
            timer = spawnInterval;
        }
    }

    // [QUAN TRỌNG] Cập nhật hàm này nhận thêm float multiplier
    public void StartSpawningWave(List<EnemyBasic> enemies, float interval, float multiplier)
    {
        currentWaveEnemies = new List<EnemyBasic>(enemies);
        spawnInterval = interval;
        currentBuffMultiplier = multiplier; // Lưu lại hệ số

        isSpawning = true;
        timer = 0f;
    }

    public void StopSpawning()
    {
        isSpawning = false;
        currentWaveEnemies.Clear();
    }

    private void SpawnNextEnemy()
    {
        if (currentWaveEnemies.Count > 0)
        {
            EnemyBasic prefabToSpawn = currentWaveEnemies[0];
            currentWaveEnemies.RemoveAt(0);

            Vector3 pos = transform.position;
            pos += new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), 0f);

            if (prefabToSpawn != null)
            {
                // 1. Tạo quái
                EnemyBasic newEnemy = Instantiate(prefabToSpawn, pos, Quaternion.identity);

                // 2. [QUAN TRỌNG] Buff sức mạnh ngay lập tức
                newEnemy.BuffStats(currentBuffMultiplier);
            }
        }
    }
}