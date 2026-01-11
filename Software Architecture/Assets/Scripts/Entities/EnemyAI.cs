using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public EnemyConfig config;
    public EnemyHealth enemyHealth;

    [Header("Target")]
    public Transform target;

    protected float nextAttackTime;
    bool warnedZeroDamage;

    void Awake()
    {
        if (enemyHealth == null) enemyHealth = GetComponent<EnemyHealth>();
        if (config == null)
        {
            if (enemyHealth != null) config = enemyHealth.config;
        }
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

        // Use planar distance (XZ) so differences in model height/scale don't break melee range.
        Vector2 a = new Vector2(transform.position.x, transform.position.z);
        Vector2 b = new Vector2(target.position.x, target.position.z);
        float dist = Vector2.Distance(a, b);

        // Move towards player until in attack range
        if (dist > config.attackRange)
        {
            Vector3 dir = (target.position - transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
            {
                dir.Normalize();
                transform.position += dir * config.moveSpeed * Time.deltaTime;
                transform.forward = Vector3.Lerp(transform.forward, dir, 12f * Time.deltaTime);
            }
        }
        else
        {
            TryAttack();
        }
    }

    protected virtual void TryAttack()
    {
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + config.attackCooldown;

        if (!warnedZeroDamage && config.contactDamage <= 0)
        {
            warnedZeroDamage = true;
            Debug.LogWarning($"{config.displayName} has contactDamage <= 0. It will not hurt the player. Check EnemyConfig.");
        }

        var playerHealth = target.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null)
            playerHealth = target.GetComponentInChildren<PlayerHealth>();
        if (playerHealth != null)
        {
            // Clear indication via log (you can also add VFX later)
            Debug.Log($"{config.displayName} attacked for {config.contactDamage}!");
            if (CombatTextManager.Instance != null)
                CombatTextManager.Instance.SpawnWorldText(transform.position + Vector3.up * 2f, "ATTACK", new Color(1f, 0.6f, 0.2f));
            playerHealth.TakeDamage(config.contactDamage);
        }
    }
}

