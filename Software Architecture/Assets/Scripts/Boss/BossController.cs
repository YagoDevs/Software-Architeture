/*
This script is used to control the boss: chase the player, choose attacks, trigger animations, and apply damage. Some parts were iterated with AI assistance during development (refinement and edge-case fixes).
*/

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
    public string targetTag = "Player";
    public bool fallbackFindPlayerHealth = true;
    [Min(0.1f)] public float reacquireInterval = 0.75f;
    float nextReacquireTime;

    [Header("Attacks (ScriptableObjects)")]
    public List<BossAttackDefinition> attacks = new List<BossAttackDefinition>();
    [Tooltip("If enabled, the boss alternates attacks in list order (round-robin), instead of always picking the first available attack.")]
    public bool alternateAttacks = true;
    int nextAttackIndex;
    [Tooltip("Extra global delay after any attack finishes (independent of per-attack cooldown).")]
    [Min(0f)] public float globalDelayBetweenAttacks = 2.0f;
    float nextAttackAllowedAt;

    [Header("Movement (Chase)")]
    public bool chaseTarget = true;
    [Min(0f)] public float moveSpeed = 2.5f;
    [Min(0f)] public float stoppingDistance = 3.5f; // used if attacks list is empty

    [Header("Animation (optional)")]
    public Animator animator; // assign boss Animator (or leave empty to auto-find)
    public string animSpeedParam = "Speed"; // requires a float param on the controller
    public float animSpeedDamp = 10f;
    bool warnedAnim;

    [Header("Boss Settings")]
    public EnemyHealth enemyHealth; // optional, for death checks

    readonly Dictionary<BossAttackDefinition, float> nextReadyAt = new Dictionary<BossAttackDefinition, float>();
    readonly Dictionary<BossAttackType, IBossAttackStrategy> strategies = new Dictionary<BossAttackType, IBossAttackStrategy>();
    Coroutine currentAttack;
    float move01;

    void Awake()
    {
        isActive = activeOnStart;
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (enemyHealth == null) enemyHealth = GetComponent<EnemyHealth>();

        // Strategy Pattern: one subclass per attack type.
        strategies[BossAttackType.MeleeHit] = new BossMeleeHitStrategy();
        strategies[BossAttackType.SlamAOE] = new BossSlamAOEStrategy();
        strategies[BossAttackType.Dash] = new BossDashStrategy();
    }

    void Start()
    {
        TryAcquireTarget(force: true);
    }

    void Update()
    {
        if (!isActive) return;
        if (enemyHealth != null && enemyHealth.IsDead) return;
        if (target == null)
        {
            TryAcquireTarget(force: false);
            UpdateAnimator();
            return;
        }
        //already attacking
        if (currentAttack != null) return;
        //not allowed to attack yet
        if (Time.time < nextAttackAllowedAt) return;
        //calculate the distance to the target

        float dist = PlanarDistanceToTarget();
        float desiredStop = GetDesiredStopDistance();

        if (chaseTarget && dist > desiredStop)
        {
            MoveTowardsTarget();
            move01 = 1f;
            UpdateAnimator();
            return;
        }

        move01 = 0f;
        FaceTarget();
        UpdateAnimator();

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

    void MoveTowardsTarget()
    {
        if (target == null) return;

        Vector3 dir = (target.position - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();

        transform.position += dir * Mathf.Max(0f, moveSpeed) * Time.deltaTime;
        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, faceTargetRotateSpeed * Time.deltaTime);
    }

    float GetDesiredStopDistance()
    {
        // Prefer the smallest configured attack range (so boss walks until it can use at least one attack).
        float min = float.PositiveInfinity;
        if (attacks != null)
        {
            for (int i = 0; i < attacks.Count; i++)
            {
                var a = attacks[i];
                if (a == null) continue;
                min = Mathf.Min(min, Mathf.Max(0.1f, a.range));
            }
        }
        if (!float.IsFinite(min)) min = Mathf.Max(0.1f, stoppingDistance);
        return min;
    }

    void TryAcquireTarget(bool force)
    {
        if (!force && Time.time < nextReacquireTime) return;
        nextReacquireTime = Time.time + Mathf.Max(0.1f, reacquireInterval);

        if (!string.IsNullOrEmpty(targetTag))
        {
            try
            {
                var go = GameObject.FindGameObjectWithTag(targetTag);
                if (go != null) { target = go.transform; return; }
            }
            catch (UnityException)
            {
                // tag might not exist
            }
        }

        if (fallbackFindPlayerHealth)
        {
            var ph = FindObjectOfType<PlayerHealth>();
            if (ph != null) target = ph.transform;
        }
    }

    void UpdateAnimator()
    {
        if (animator == null) return;
        if (string.IsNullOrEmpty(animSpeedParam)) return;

        if (!warnedAnim)
        {
            warnedAnim = true;
            if (animator.runtimeAnimatorController == null)
                Debug.LogWarning($"{name}: BossController Animator has no controller assigned.");
            else if (!HasParameter(animator, animSpeedParam))
                Debug.LogWarning($"{name}: BossController Animator missing float parameter '{animSpeedParam}'. Add it to the BossAnimator controller if you want walk animations.");
        }

        if (!HasParameter(animator, animSpeedParam)) return;
        float current = animator.GetFloat(animSpeedParam);
        float next = Mathf.Lerp(current, move01, animSpeedDamp * Time.deltaTime);
        animator.SetFloat(animSpeedParam, next);
    }

    static bool HasParameter(Animator anim, string paramName)
    {
        if (anim == null) return false;
        var ps = anim.parameters;
        for (int i = 0; i < ps.Length; i++)
        {
            if (ps[i].name == paramName)
                return true;
        }
        return false;
    }

    BossAttackDefinition PickAttack()
    {
        if (attacks == null || attacks.Count == 0) return null;

        float dist = PlanarDistanceToTarget();
        int count = attacks.Count;

        // Round-robin: try starting from nextAttackIndex to alternate attacks over time.
        if (alternateAttacks)
        {
            int start = (count <= 0) ? 0 : (nextAttackIndex % count);
            for (int offset = 0; offset < count; offset++)
            {
                int i = (start + offset) % count;
                var a = attacks[i];
                if (a == null) continue;
                if (dist > a.range) continue;
                if (!IsReady(a)) continue;
                nextAttackIndex = (i + 1) % count;
                return a;
            }
        }
        else
        {
            // Simple priority: first available attack whose range matches and cooldown is ready.
            for (int i = 0; i < count; i++)
            {
                var a = attacks[i];
                if (a == null) continue;
                if (dist > a.range) continue;
                if (!IsReady(a)) continue;
                return a;
            }
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

        if (!strategies.TryGetValue(a.type, out var strategy) || strategy == null)
        {
            currentAttack = null;
            yield break;
        }

        yield return strategy.Execute(this, a);

        // Global spacing between skills (even if per-attack cooldown is low).
        nextAttackAllowedAt = Time.time + Mathf.Max(0f, globalDelayBetweenAttacks);
        currentAttack = null;
    }

    public void ApplyDamageIfInRange(int dmg, float range)
    {
        if (target == null) return;
        float dist = PlanarDistanceToTarget();
        if (dist > range) return;

        var ph = target.GetComponentInParent<PlayerHealth>();
        if (ph == null) ph = target.GetComponentInChildren<PlayerHealth>();
        if (ph == null) return;

        if (CombatTextManager.Instance != null)
            CombatTextManager.Instance.SpawnWorldText(transform.position + Vector3.up * 3f, "BOSS ATTACK", new Color(1f, 0.2f, 0.8f));

        ph.TakeDamage(Mathf.Max(0, dmg));
    }

    public IEnumerator DashForward(float distance, float duration)
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

    float PlanarDistanceToTarget()
    {
        if (target == null) return float.PositiveInfinity;

        var myCol = GetComponentInChildren<Collider>();
        var targetCol = target.GetComponentInParent<Collider>();
        if (targetCol == null) targetCol = target.GetComponentInChildren<Collider>();

        // Prefer bounds-to-bounds distance on XZ: robust even with large scales and overlapping colliders.
        if (myCol != null && targetCol != null)
            return PlanarBoundsDistance(myCol.bounds, targetCol.bounds);

        return PlanarDistance(transform.position, target.position);
    }

    static float PlanarBoundsDistance(Bounds a, Bounds b)
    {
        // Distance between AABB rectangles on XZ plane (0 if overlapping).
        float dx = 0f;
        if (a.max.x < b.min.x) dx = b.min.x - a.max.x;
        else if (b.max.x < a.min.x) dx = a.min.x - b.max.x;

        float dz = 0f;
        if (a.max.z < b.min.z) dz = b.min.z - a.max.z;
        else if (b.max.z < a.min.z) dz = a.min.z - b.max.z;

        return Mathf.Sqrt(dx * dx + dz * dz);
    }
}

