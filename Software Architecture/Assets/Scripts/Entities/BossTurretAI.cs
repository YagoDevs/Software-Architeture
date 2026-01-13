using UnityEngine;

// Stationary boss that activates on trigger and uses multiple attack patterns.
public class BossTurretAI : MonoBehaviour
{
    [Header("Config/Refs")]
    public EnemyConfig config;
    public EnemyHealth enemyHealth;
    public Transform target;

    [Header("Projectile")]
    public FireballProjectile projectilePrefab;
    public Transform firePoint;
    public LayerMask projectileHitLayers = ~0;
    public float projectileSpeed = 22f;
    public float projectileLifetime = 3.5f;

    [Header("Activation")]
    public bool activeOnStart = false;
    public bool isActive;

    [Header("Patterns")]
    public float patternSwitchInterval = 4.0f;
    public float windup = 0.25f;
    public int volleyCount = 3;
    public float volleyInterval = 0.12f;
    public float spreadAngle = 10f; // pattern B

    float nextAttackTime;
    float nextPatternSwitch;
    int patternIndex; // 0=single/volley, 1=spread

    void Awake()
    {
        if (enemyHealth == null) enemyHealth = GetComponent<EnemyHealth>();
        if (config == null && enemyHealth != null) config = enemyHealth.config;
        isActive = activeOnStart;
    }

    void Start()
    {
        if (target == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }

        nextPatternSwitch = Time.time + patternSwitchInterval;
        patternIndex = 0;
    }

    void Update()
    {
        if (!isActive) return;
        if (enemyHealth != null && enemyHealth.IsDead) return;
        if (config == null) return;
        if (target == null) return;
        if (projectilePrefab == null) return;

        // Switch patterns over time
        if (Time.time >= nextPatternSwitch)
        {
            patternIndex = (patternIndex + 1) % 2;
            nextPatternSwitch = Time.time + patternSwitchInterval;
        }

        // Range check (planar)
        Vector2 a = new Vector2(transform.position.x, transform.position.z);
        Vector2 b = new Vector2(target.position.x, target.position.z);
        float dist = Vector2.Distance(a, b);
        if (dist > config.attackRange) return;

        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + config.attackCooldown;

        if (windup > 0f)
            Invoke(nameof(DoAttack), windup);
        else
            DoAttack();
    }

    public void Activate()
    {
        if (isActive) return;
        isActive = true;
        nextAttackTime = Time.time + 0.35f;
        if (CombatTextManager.Instance != null)
            CombatTextManager.Instance.SpawnWorldText(transform.position + Vector3.up * 3f, "BOSS!", new Color(1f, 0.2f, 0.8f));
    }

    void DoAttack()
    {
        if (!isActive) return;
        if (enemyHealth != null && enemyHealth.IsDead) return;

        if (patternIndex == 0)
        {
            // Pattern A: short volley (multi-behaviour)
            for (int i = 0; i < Mathf.Max(1, volleyCount); i++)
                Invoke(nameof(FireAtPlayer), i * volleyInterval);
            return;
        }

        // Pattern B: spread shot (3 projectiles)
        FireSpread();
    }

    void FireAtPlayer()
    {
        FireWithDirection(GetAimDirection());
    }

    void FireSpread()
    {
        Vector3 baseDir = GetAimDirection();
        FireWithDirection(Quaternion.Euler(0, -spreadAngle, 0) * baseDir);
        FireWithDirection(baseDir);
        FireWithDirection(Quaternion.Euler(0, spreadAngle, 0) * baseDir);
    }

    Vector3 GetAimDirection()
    {
        Vector3 spawnPos = firePoint != null ? firePoint.position : (transform.position + Vector3.up * 2.2f);
        Vector3 dir = (target.position - spawnPos);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) dir = transform.forward;
        dir.Normalize();
        return dir;
    }

    void FireWithDirection(Vector3 dir)
    {
        if (projectilePrefab == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : (transform.position + Vector3.up * 2.2f);
        var proj = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(dir, Vector3.up));
        proj.damage = Mathf.Max(1, config.contactDamage);
        proj.speed = projectileSpeed;
        proj.lifetime = projectileLifetime;
        proj.hitLayers = projectileHitLayers;
        proj.Launch(dir, proj.damage, proj.speed, proj.hitLayers);
        proj.hitsPlayer = true;
        proj.hitsEnemies = false;
    }
}

