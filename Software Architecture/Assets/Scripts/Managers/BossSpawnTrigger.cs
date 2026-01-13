using UnityEngine;

// Put this on a trigger collider. When Player enters, spawn boss at a specific spawn point and activate it.
public class BossSpawnTrigger : MonoBehaviour
{
    [Header("Boss Spawn")]
    public GameObject bossPrefab;
    public Transform spawnPoint;
    public bool spawnOnce = true;

    [Header("Activation")]
    public bool activateBossAI = true;

    bool spawned;

    void OnTriggerEnter(Collider other)
    {
        if (spawnOnce && spawned) return;
        if (!other.CompareTag("Player")) return;
        if (bossPrefab == null || spawnPoint == null) return;

        var boss = Instantiate(bossPrefab, spawnPoint.position, spawnPoint.rotation);
        spawned = true;

        if (activateBossAI)
        {
            var ai = boss.GetComponentInChildren<BossTurretAI>();
            if (ai != null) ai.Activate();
        }
    }
}

