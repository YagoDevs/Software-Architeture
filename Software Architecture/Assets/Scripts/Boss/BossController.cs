using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Stationary-by-default boss controller driven by ScriptableObject attacks (no hardcoding).
public class BossController : MonoBehaviour
{
    [Header("Activation")]
    public bool activeOnStart = false;
    public bool isActive;

    [Header("Target")]
    public Transform target;
    public float faceTargetRotateSpeed = 8f;

    [Header("Attacks (ScriptableObjects)")]
    public List<BossAttackDefinition> attacks = new List<BossAttackDefinition>();

    [Header("Animation (optional)")]
    public Animator animator; // assign boss Animator (or leave empty to auto-find)

    [Header("Boss Settings")]
    public EnemyHealth enemyHealth; // optional, for death checks

    readonly Dictionary<BossAttackDefinition, float> nextReadyAt = new Dictionary<BossAttackDefinition, float>();
    Coroutine currentAttack;

    void Awake()
    {
        isActive = activeOnStart;
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (enemyHealth == null) enemyHealth = GetComponent<EnemyHealth>();
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
        if (!isActive) return;
        if (enemyHealth != null && enemyHealth.IsDead) return;
        if (target == null) return;
        if (currentAttack != null) return;

        FaceTarget();

        var attack = PickAttack();
        if (attack == null) return;

        currentAttack = StartCoroutine(DoAttack(attack));
    }

    public void Activate()
    {
        isActive = true;
    }

    void FaceTarget()
    {
        Vector3 dir = (target.position - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        Quaternion rot = Quaternion.LookRotation(dir.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, faceTargetRotateSpeed * Time.deltaTime);
    }

    BossAttackDefinition PickAttack()
    {
        if (attacks == null || attacks.Count == 0) return null;

        // Simple priority: first available attack whose range matches and cooldown is ready.
        float dist = PlanarDistance(transform.position, target.position);
        for (int i = 0; i < attacks.Count; i++)
        {
            var a = attacks[i];
            if (a == null) continue;
            if (dist > a.range) continue;
            if (!IsReady(a)) continue;
            return a;
        }

        return null;
    }

    bool IsReady(BossAttackDefinition a)
    {
        if (!nextReadyAt.TryGetValue(a, out float t)) return true;
        return Time.time >= t;
    }

    void SetCooldown(BossAttackDefinition a)
    {
        nextReadyAt[a] = Time.time + Mathf.Max(0f, a.cooldown);
    }

    IEnumerator DoAttack(BossAttackDefinition a)
    {
        SetCooldown(a);

        if (animator != null && !string.IsNullOrEmpty(a.animationTrigger))
            animator.SetTrigger(a.animationTrigger);

        if (a.windup > 0f)
            yield return new WaitForSeconds(a.windup);

        // Re-check state after windup
        if (!isActive) { currentAttack = null; yield break; }
        if (enemyHealth != null && enemyHealth.IsDead) { currentAttack = null; yield break; }
        if (target == null) { currentAttack = null; yield break; }

        switch (a.type)
        {
            case BossAttackType.MeleeHit:
                ApplyDamageIfInRange(a.damage, a.range);
                break;

            case BossAttackType.SlamAOE:
                ApplyDamageIfInRange(a.damage, a.aoeRadius);
                break;

            case BossAttackType.Dash:
                yield return DashForward(a.dashDistance, a.dashDuration);
                // Optional: damage at end of dash if close
                ApplyDamageIfInRange(a.damage, a.range);
                break;
        }

        currentAttack = null;
    }

    void ApplyDamageIfInRange(int dmg, float range)
    {
        if (target == null) return;
        float dist = PlanarDistance(transform.position, target.position);
        if (dist > range) return;

        var ph = target.GetComponentInParent<PlayerHealth>();
        if (ph == null) ph = target.GetComponentInChildren<PlayerHealth>();
        if (ph == null) return;

        if (CombatTextManager.Instance != null)
            CombatTextManager.Instance.SpawnWorldText(transform.position + Vector3.up * 3f, "BOSS ATTACK", new Color(1f, 0.2f, 0.8f));

        ph.TakeDamage(Mathf.Max(0, dmg));
    }

    IEnumerator DashForward(float distance, float duration)
    {
        if (duration <= 0f) yield break;
        Vector3 start = transform.position;
        Vector3 end = start + transform.forward * Mathf.Max(0f, distance);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }
    }

    static float PlanarDistance(Vector3 a, Vector3 b)
    {
        Vector2 a2 = new Vector2(a.x, a.z);
        Vector2 b2 = new Vector2(b.x, b.z);
        return Vector2.Distance(a2, b2);
    }
}

