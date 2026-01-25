/*
This script is used to spawn and activate the boss when the player enters a trigger area (with an optional fallback).
*/

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

    [Header("Debug / Fallback")]
    public bool debugLogs = true;
    public bool useBoundsFallback = true; // works even if OnTriggerEnter never fires

    bool spawned;
    Collider triggerCollider;

    void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        if (debugLogs && triggerCollider == null)
            Debug.LogWarning("BossSpawnTrigger: No Collider found on this GameObject. Add a BoxCollider and enable Is Trigger.");
    }

    void Update()
    {
        if (!useBoundsFallback) return;
        if (spawnOnce && spawned) return;
        if (bossPrefab == null || spawnPoint == null) return;
        if (triggerCollider == null) return;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // Bounds check fallback: if player position is inside trigger bounds, spawn.
        if (triggerCollider.bounds.Contains(player.transform.position))
        {
            if (debugLogs) Debug.Log("BossSpawnTrigger: bounds fallback activated.");
            SpawnBoss(player);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (spawnOnce && spawned) return;
        if (!other.CompareTag("Player")) return;
        if (bossPrefab == null || spawnPoint == null) return;

        if (debugLogs) Debug.Log("BossSpawnTrigger: OnTriggerEnter activated.");
        SpawnBoss(other.gameObject);
    }

    void SpawnBoss(GameObject player)
    {
        if (spawnOnce && spawned) return;
        var boss = Instantiate(bossPrefab, spawnPoint.position, spawnPoint.rotation);
        spawned = true;

        if (debugLogs) Debug.Log($"BossSpawnTrigger: spawned boss '{boss.name}' at '{spawnPoint.name}'.");

        if (activateBossAI)
        {
            // Prefer new configurable boss controller, fallback to older turret AI.
            var ctrl = boss.GetComponentInChildren<BossController>();
            if (ctrl != null) { ctrl.Activate(); return; }
        }
    }
}

