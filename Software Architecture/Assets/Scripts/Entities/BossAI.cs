using UnityEngine;

// Simple boss: alternates patterns to satisfy "multiple behaviours".
public class BossAI : EnemyAI
{
    [Header("Boss Patterns")]
    public float patternSwitchInterval = 4f;
    public int slamDamageMultiplier = 2;
    public float slamRangeMultiplier = 1.35f;

    float nextPatternSwitch;
    int patternIndex; // 0 = normal, 1 = slam

    void OnEnable()
    {
        nextPatternSwitch = Time.time + patternSwitchInterval;
        patternIndex = 0;
    }

    void LateUpdate()
    {
        if (config == null) return;
        if (!config.isBoss) return;

        if (Time.time >= nextPatternSwitch)
        {
            patternIndex = (patternIndex + 1) % 2;
            nextPatternSwitch = Time.time + patternSwitchInterval;
        }
    }

    protected override void TryAttack()
    {
        if (config == null) return;
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + config.attackCooldown;

        var playerHealth = target != null ? target.GetComponent<PlayerHealth>() : null;
        if (playerHealth == null) return;

        if (patternIndex == 0)
        {
            // Pattern A: normal hit
            Debug.Log($"{config.displayName} (BOSS) used Normal Attack!");
            playerHealth.TakeDamage(config.contactDamage);
            return;
        }

        // Pattern B: slam (bigger range + damage)
        float slamRange = config.attackRange * slamRangeMultiplier;
        float dist = target != null ? Vector3.Distance(transform.position, target.position) : float.MaxValue;
        if (dist <= slamRange)
        {
            int dmg = Mathf.Max(1, config.contactDamage * slamDamageMultiplier);
            Debug.Log($"{config.displayName} (BOSS) used SLAM for {dmg}!");
            playerHealth.TakeDamage(dmg);
        }
        else
        {
            // If too far, don't waste cooldown: try again soon
            nextAttackTime = Time.time + 0.25f;
        }
    }
}

