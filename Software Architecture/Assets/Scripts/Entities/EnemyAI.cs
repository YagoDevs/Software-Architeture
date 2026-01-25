/*
This script is used for a simple melee enemy AI: chase the player, play walk/attack animations, and deal contact damage on a cooldown.
*/

using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public EnemyConfig config;
    public EnemyHealth enemyHealth;
    public Animator animator;

    [Header("Target")]
    public Transform target;
    [Tooltip("If Target is not set, EnemyAI will try to find a target by Tag (default: Player).")]
    public string targetTag = "Player";
    [Tooltip("If Target is not set and tag search fails, EnemyAI will try to find a PlayerHealth in the scene.")]
    public bool fallbackFindPlayerHealth = true;
    [Min(0.1f)]
    public float reacquireInterval = 0.75f;
    float nextReacquireTime;
    bool warnedNoTarget;

    protected float nextAttackTime;
    bool warnedZeroDamage;

    [Header("Animation (optional)")]
    public string animSpeedParam = "Speed"; // 0 idle, 1 walking
    public float animSpeedDamp = 10f;
    public string attackTrigger = "Attack";
    float move01;
    bool warnedAnim;
    bool warnedNoAnimator;

    void Awake()
    {
        if (enemyHealth == null) enemyHealth = GetComponent<EnemyHealth>();
        if (config == null)
        {
            if (enemyHealth != null) config = enemyHealth.config;
        }

        animator = ResolveAnimator(animator);
    }

    void Start()
    {
        TryAcquireTarget(force: true);
    }

    void Update()
    {
        if (enemyHealth != null && enemyHealth.IsDead) return;
        if (config == null) return;
        if (target == null)
        {
            TryAcquireTarget(force: false);
            if (target == null)
            {
                if (!warnedNoTarget)
                {
                    warnedNoTarget = true;
                    Debug.LogWarning($"{name}: EnemyAI has no target. Set 'target' in Inspector or tag your player as '{targetTag}'. (Fallback search by PlayerHealth is {(fallbackFindPlayerHealth ? "ON" : "OFF")}).");
                }
                UpdateAnimator(); // keep animator stable even without target
                return;
            }
        }

        // Use planar distance XZ.... Prefer collider-to-collider distance so CharacterController/colliders
        // don't prevent reaching the attack range due to center-to-center checks.
        float dist = PlanarDistanceToTarget();

        // Move towards player until in attack range
        if (dist > config.attackRange)
        {
            move01 = 1f;
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
            move01 = 0f;
            TryAttack();
        }

        UpdateAnimator();
    }

    float PlanarDistanceToTarget()
    {
        if (target == null) return float.PositiveInfinity;

        var myCol = GetComponentInChildren<Collider>();
        var targetCol = target.GetComponentInParent<Collider>();
        if (targetCol == null) targetCol = target.GetComponentInChildren<Collider>();

        Vector3 a = transform.position;
        Vector3 b = target.position;

        if (myCol != null)
        {
            // Closest point on this enemy to the target's position.
            a = myCol.ClosestPoint(target.position);
        }
        if (targetCol != null)
        {
            // Closest point on the target to this enemy's position.
            b = targetCol.ClosestPoint(transform.position);
        }

        Vector2 a2 = new Vector2(a.x, a.z);
        Vector2 b2 = new Vector2(b.x, b.z);
        return Vector2.Distance(a2, b2);
    }

    void TryAcquireTarget(bool force)
    {
        if (!force && Time.time < nextReacquireTime) return;
        nextReacquireTime = Time.time + Mathf.Max(0.1f, reacquireInterval);

        // 1) Prefer tag-based lookup (fast, conventional)
        if (!string.IsNullOrEmpty(targetTag))
        {
            try
            {
                var player = GameObject.FindGameObjectWithTag(targetTag);
                if (player != null)
                {
                    target = player.transform;
                    warnedNoTarget = false;
                    return;
                }
            }
            catch (UnityException)
            {
                // Tag might not exist in Tag Manager; ignore and fallback
            }
        }

        // 2) Fallback: find any PlayerHealth in the scene
        if (fallbackFindPlayerHealth)
        {
            var ph = FindObjectOfType<PlayerHealth>();
            if (ph != null)
            {
                target = ph.transform;
                warnedNoTarget = false;
            }
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

            if (animator != null && !string.IsNullOrEmpty(attackTrigger))
                animator.SetTrigger(attackTrigger);

            playerHealth.TakeDamage(config.contactDamage);
        }
    }

    void UpdateAnimator()
    {
        if (animator == null)
        {
            if (!warnedNoAnimator)
            {
                warnedNoAnimator = true;
                Debug.LogWarning($"{name}: EnemyAI could not find an Animator to drive. Add an Animator (ideally on the model with SkinnedMeshRenderer) or assign the 'animator' field.");
            }
            return;
        }
        if (string.IsNullOrEmpty(animSpeedParam)) return;

        if (!warnedAnim)
        {
            warnedAnim = true;
            // Warn once if controller/parameters are missing
            if (animator.runtimeAnimatorController == null)
                Debug.LogWarning($"{name}: EnemyAI Animator has no controller assigned.");
            else if (!HasParameter(animator, animSpeedParam))
                Debug.LogWarning($"{name}: EnemyAI Animator missing float parameter '{animSpeedParam}'.");
            else if (!string.IsNullOrEmpty(attackTrigger) && !HasParameter(animator, attackTrigger))
                Debug.LogWarning($"{name}: EnemyAI Animator missing trigger parameter '{attackTrigger}'.");
        }

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

    Animator ResolveAnimator(Animator preferred)
    {
        // If explicitly assigned and valid, keep it.
        if (preferred != null) return preferred;

        var anims = GetComponentsInChildren<Animator>(true);
        if (anims == null || anims.Length == 0) return null;
        if (anims.Length == 1) return anims[0];

        // Prefer an Animator that actually drives a skinned mesh (typical for characters),
        // and avoid the Animator on the same GameObject as this AI when there are multiple.
        Animator best = null;
        int bestScore = int.MinValue;
        for (int i = 0; i < anims.Length; i++)
        {
            var a = anims[i];
            if (a == null) continue;

            int score = 0;
            if (a.transform != transform) score += 10; // prefer child/model animator over root
            if (a.GetComponentInChildren<SkinnedMeshRenderer>(true) != null) score += 5;
            if (a.runtimeAnimatorController != null) score += 1;

            // If we already know the param name, prefer controllers that contain it.
            if (!string.IsNullOrEmpty(animSpeedParam) && HasParameter(a, animSpeedParam)) score += 2;
            if (!string.IsNullOrEmpty(attackTrigger) && HasParameter(a, attackTrigger)) score += 1;

            if (score > bestScore)
            {
                bestScore = score;
                best = a;
            }
        }

        return best != null ? best : anims[0];
    }
}

