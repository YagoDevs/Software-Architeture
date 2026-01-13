using UnityEngine;

// Stationary enemy that shoots projectiles at the player.
public class MageTurretAI : MonoBehaviour
{
    [Header("Config/Refs")]
    public EnemyConfig config;
    public EnemyHealth enemyHealth;
    public Transform target;

    [Header("Projectile")]
    public FireballProjectile projectilePrefab;
    public Transform firePoint;
    public LayerMask projectileHitLayers = ~0; // should include Player layer
    public float projectileSpeed = 18f;
    public float projectileLifetime = 3f;
    public float windup = 0.15f; // delay to sync with cast anim
    public float rotateSpeed = 8f;

    [Header("Animation (optional)")]
    public Animator animator; // assign SkeletonMage Animator (or leave empty to auto-find)
    public string castTrigger = "Cast";

    [Header("Audio (optional)")]
    public AudioSource audioSource; // if null, uses PlayClipAtPoint
    public AudioClip castSfx;
    [Range(0f, 1f)] public float sfxVolume = 0.9f;

    float nextAttackTime;

    void Awake()
    {
        if (enemyHealth == null) enemyHealth = GetComponent<EnemyHealth>();
        if (config == null && enemyHealth != null) config = enemyHealth.config;
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (audioSource == null) audioSource = GetComponentInChildren<AudioSource>();
    }

    void Start()
    {
        if (target == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }
    }

    void Update()
    {
        if (enemyHealth != null && enemyHealth.IsDead) return;
        if (config == null) return;
        if (target == null) return;
        if (projectilePrefab == null) return;

        // Range check (planar)
        Vector2 a = new Vector2(transform.position.x, transform.position.z);
        Vector2 b = new Vector2(target.position.x, target.position.z);
        float dist = Vector2.Distance(a, b);
        if (dist > config.attackRange) return;

        // Rotate to face player (Y only)
        Vector3 lookDir = (target.position - transform.position);
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
        }

        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + config.attackCooldown;

        // Trigger cast animation right away, then shoot after windup
        if (animator != null && !string.IsNullOrEmpty(castTrigger))
            animator.SetTrigger(castTrigger);
        PlaySfx(castSfx, transform.position);

        if (windup > 0f) Invoke(nameof(Fire), windup);
        else Fire();
    }

    void OnDisable()
    {
        CancelInvoke();
    }

    void Fire()
    {
        if (enemyHealth != null && enemyHealth.IsDead) return;
        if (target == null || projectilePrefab == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : (transform.position + Vector3.up * 1.8f);
        Vector3 dir = (target.position - spawnPos);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) dir = transform.forward;
        dir.Normalize();

        var proj = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(dir, Vector3.up));
        proj.damage = Mathf.Max(1, config.contactDamage); // reuse contactDamage as projectile damage (simple)
        proj.speed = projectileSpeed;
        proj.lifetime = projectileLifetime;
        proj.hitLayers = projectileHitLayers;
        proj.hitsPlayer = true;
        proj.hitsEnemies = false;

        // Launch() defaults to hitsEnemies=true, so we set flags again after calling it.
        proj.Launch(dir, proj.damage, proj.speed, proj.hitLayers);
        proj.hitsPlayer = true;
        proj.hitsEnemies = false;

        if (CombatTextManager.Instance != null)
            CombatTextManager.Instance.SpawnWorldText(transform.position + Vector3.up * 2f, "CAST", new Color(0.6f, 0.8f, 1f));
    }

    // Optional: you can call this from an Animation Event on the cast clip
    public void PlayCastSfx()
    {
        PlaySfx(castSfx, transform.position);
    }

    void PlaySfx(AudioClip clip, Vector3 pos)
    {
        if (clip == null) return;
        if (sfxVolume <= 0f) return;

        if (audioSource != null)
        {
            audioSource.PlayOneShot(clip, sfxVolume);
            return;
        }

        AudioSource.PlayClipAtPoint(clip, pos, sfxVolume);
    }
}

