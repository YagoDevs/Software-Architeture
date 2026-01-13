using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs (2 regular + 1 boss)")]
    public GameObject enemyTypeA;
    public GameObject enemyTypeB;
    public GameObject bossEnemy;

    [Header("Spawn Rules")]
    public int maxAlive = 6;
    public float spawnInterval = 2.0f;
    public float spawnRadius = 18f;
    public float minDistanceFromPlayer = 6f;
    public bool countExistingEnemiesOnStart = true;

    [Header("Boss Rules")]
    public bool spawnBossAfterKills = true;
    public int killsToSpawnBoss = 12;
    public bool stopSpawningAfterBossSpawned = false;
    public bool stopSpawningAfterBossKilled = false;

    [Header("Stop Spawning (Quest)")]
    public bool stopSpawningWhenQuestCompleted = false;
    public string questIdToStopSpawning;

    [Header("Optional Spawn Points")]
    public List<Transform> spawnPoints = new List<Transform>();
    public bool useSequentialSpawnPoints = false;

    float nextSpawnTime;
    int aliveCount;
    int killCount;
    bool bossSpawned;
    bool stopSpawning;
    int nextSpawnPointIndex;

    Transform player;

    void OnEnable()
    {
        CombatEvents.OnEnemyKilled += HandleEnemyKilled;
    }

    void OnDisable()
    {
        CombatEvents.OnEnemyKilled -= HandleEnemyKilled;
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        if (countExistingEnemiesOnStart)
        {
            // If you already placed enemies in the scene, don't exceed maxAlive.
            aliveCount = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None).Length;

            // If a boss already exists in scene, don't spawn another one.
            if (FindObjectsByType<BossController>(FindObjectsSortMode.None).Length > 0)
                bossSpawned = true;
        }

        nextSpawnPointIndex = 0;
    }

    void Update()
    {
        if (stopSpawning) return;
        if (Time.time < nextSpawnTime) return;
        nextSpawnTime = Time.time + spawnInterval;

        if (stopSpawningWhenQuestCompleted && QuestManager.Instance != null && !string.IsNullOrEmpty(questIdToStopSpawning))
        {
            if (QuestManager.Instance.IsQuestCompleted(questIdToStopSpawning))
            {
                stopSpawning = true;
                return;
            }
        }

        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            if (player == null) return;
        }

        // Boss trigger
        if (!bossSpawned && spawnBossAfterKills && killCount >= killsToSpawnBoss && bossEnemy != null)
        {
            TrySpawn(bossEnemy);
            bossSpawned = true;
            if (stopSpawningAfterBossSpawned)
                stopSpawning = true;
            return;
        }

        // Regular spawn
        if (aliveCount >= maxAlive) return;

        GameObject prefab = PickRegularPrefab();
        if (prefab == null) return;

        TrySpawn(prefab);
    }

    void HandleEnemyKilled(EnemyConfig config)
    {
        // One death => one less alive
        aliveCount = Mathf.Max(0, aliveCount - 1);

        // Count kills for boss trigger (ignore null config)
        if (config != null)
            killCount += 1;

        if (config != null && config.isBoss && stopSpawningAfterBossKilled)
            stopSpawning = true;
    }

    GameObject PickRegularPrefab()
    {
        if (enemyTypeA == null && enemyTypeB == null) return null;
        if (enemyTypeA != null && enemyTypeB == null) return enemyTypeA;
        if (enemyTypeA == null && enemyTypeB != null) return enemyTypeB;
        return (Random.value < 0.5f) ? enemyTypeA : enemyTypeB;
    }

    void TrySpawn(GameObject prefab)
    {
        if (aliveCount >= maxAlive && prefab != bossEnemy) return;

        Vector3 pos = GetSpawnPosition();
        if (Vector3.Distance(pos, player.position) < minDistanceFromPlayer)
            return;

        Instantiate(prefab, pos, Quaternion.identity);
        aliveCount += 1;
    }

    Vector3 GetSpawnPosition()
    {
        // Use spawn points if provided
        if (spawnPoints != null && spawnPoints.Count > 0)
        {
            Transform sp = null;
            if (useSequentialSpawnPoints)
            {
                // round-robin across the list
                for (int tries = 0; tries < spawnPoints.Count; tries++)
                {
                    int idx = nextSpawnPointIndex % spawnPoints.Count;
                    nextSpawnPointIndex = (nextSpawnPointIndex + 1) % spawnPoints.Count;
                    sp = spawnPoints[idx];
                    if (sp != null) break;
                }
            }
            else
            {
                sp = spawnPoints[Random.Range(0, spawnPoints.Count)];
            }

            if (sp != null) return sp.position;
        }

        // Else random around spawner
        Vector2 circle = Random.insideUnitCircle.normalized * Random.Range(3f, spawnRadius);
        return transform.position + new Vector3(circle.x, 0f, circle.y);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}

