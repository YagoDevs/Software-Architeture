using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack")]
    public float attackRange = 2.0f;
    public float attackCooldown = 0.35f;
    public int baseDamage = 8;
    public LayerMask enemyLayers = ~0; // everything by default (simple)

    [Header("Feedback (optional)")]
    public bool logAttacks = true;

    float nextAttackTime;
    PlayerProgression progression;

    void Awake()
    {
        progression = GetComponent<PlayerProgression>();
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
            TryAttack();
    }

    void TryAttack()
    {
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + attackCooldown;

        int dmg = baseDamage;
        if (progression != null)
            dmg = Mathf.RoundToInt(baseDamage * progression.damageMultiplier);

        if (logAttacks)
            Debug.Log($"Player attacked for {dmg}!");

        // Simple melee: hit any EnemyHealth near the player
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, enemyLayers);
        for (int i = 0; i < hits.Length; i++)
        {
            var eh = hits[i].GetComponentInParent<EnemyHealth>();
            if (eh != null && !eh.IsDead)
            {
                eh.TakeDamage(dmg);
                if (logAttacks)
                {
                    string n = (eh.config != null && !string.IsNullOrEmpty(eh.config.displayName)) ? eh.config.displayName : eh.name;
                    Debug.Log($"Hit confirmed on {n}. Enemy HP: {eh.CurrentHP}/{eh.MaxHP}");
                }
                // 1 target per swing (simple + readable)
                return;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

