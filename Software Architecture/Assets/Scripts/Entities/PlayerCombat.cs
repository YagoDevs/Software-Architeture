/*
This script is used to handle the player's attacks (melee and optional fireball) with cooldowns and animation triggers.
*/

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack")]
    public float attackRange = 2.0f;
    public float attackCooldown = 0.35f;
    public int baseDamage = 8;
    public float altAttackRange = 2.6f;
    public float altAttackCooldown = 0.75f;
    public int altBaseDamage = 14;
    public float altAttackWindup = 0.2f; // delay before damage/projectile (sync with animation)
    public LayerMask enemyLayers = ~0; // everything by default (simple)

    [Header("Alt Attack: Fireball (optional)")]
    public bool altAttackIsFireball = false;
    public FireballProjectile fireballPrefab;
    public Transform fireballSpawnPoint;
    public float fireballSpeed = 18f;
    public float fireballSpawnForwardOffset = 1.1f;

    [Header("Animation (optional)")]
    public Animator animator; // assign Rogue_Hooded Animator (or leave empty to auto-find)
    public string attack1Trigger = "Attack1"; // left click
    public string attack2Trigger = "Attack2"; // right click

    [Header("Audio (optional)")]
    public AudioSource audioSource; // if null, uses PlayClipAtPoint
    public AudioClip attack1Sfx;
    public AudioClip attack2Sfx;
    public AudioClip fireballLaunchSfx;
    [Range(0f, 1f)] public float sfxVolume = 0.9f;

    [Header("Feedback (optional)")]
    public bool logAttacks = true;

    float nextAttackTime;
    float nextAltAttackTime;
    PlayerProgression progression;
    IPlayerAttackStrategy primaryStrategy;
    IPlayerAttackStrategy altStrategy;

    void Awake()
    {
        progression = GetComponent<PlayerProgression>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        if (audioSource == null)
            audioSource = GetComponentInChildren<AudioSource>();

        // Strategy Pattern: choose the algorithm implementation via subclasses.
        primaryStrategy = new PlayerMeleeAttackStrategy();
        altStrategy = altAttackIsFireball ? new PlayerFireballAttackStrategy() : new PlayerMeleeAttackStrategy();
    }

    void Update()
    {
        // Don't attack when UI is open / cursor unlocked
        if (Cursor.lockState != CursorLockMode.Locked) return;
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
            TryAttackPrimary();

        if (Mouse.current.rightButton.wasPressedThisFrame)
            TryAttackAlt();
    }

    void TryAttackPrimary()
    {
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + attackCooldown;

        int dmg = baseDamage;
        if (progression != null)
            dmg = Mathf.RoundToInt(baseDamage * progression.damageMultiplier);

        if (logAttacks)
            Debug.Log($"Player attacked for {dmg}!");

        var req = new PlayerAttackRequest
        {
            range = attackRange,
            windupSeconds = 0f,
            enemyLayers = enemyLayers,
            animatorTrigger = attack1Trigger,
            sfx = attack1Sfx,
            isAlt = false,
            isFireball = false
        };

        primaryStrategy.Execute(this, req, dmg);
    }

    void TryAttackAlt()
    {
        if (Time.time < nextAltAttackTime) return;
        nextAltAttackTime = Time.time + altAttackCooldown;

        int dmg = altBaseDamage;
        if (progression != null)
            dmg = Mathf.RoundToInt(altBaseDamage * progression.damageMultiplier);

        if (logAttacks)
            Debug.Log($"Player ALT attacked for {dmg}!");

        // Ensure the ALT strategy matches current configuration (in case altAttackIsFireball is toggled at runtime).
        altStrategy = altAttackIsFireball ? (IPlayerAttackStrategy)new PlayerFireballAttackStrategy() : new PlayerMeleeAttackStrategy();

        var req = new PlayerAttackRequest
        {
            range = altAttackRange,
            windupSeconds = Mathf.Max(0f, altAttackWindup),
            enemyLayers = enemyLayers,
            animatorTrigger = attack2Trigger,
            sfx = attack2Sfx,
            isAlt = true,

            isFireball = altAttackIsFireball,
            fireballPrefab = fireballPrefab,
            fireballSpawnPoint = fireballSpawnPoint,
            fireballSpeed = fireballSpeed,
            fireballSpawnForwardOffset = fireballSpawnForwardOffset,
            fireballLaunchSfx = fireballLaunchSfx
        };

        altStrategy.Execute(this, req, dmg);
    }

    public bool IsInputAllowed()
    {
        return Cursor.lockState == CursorLockMode.Locked;
    }

    public void TriggerAttackAnimation(string trigger)
    {
        if (animator == null) return;
        if (string.IsNullOrEmpty(trigger)) return;
        animator.SetTrigger(trigger);
    }

    public void PlayAttackSfx(AudioClip clip, Vector3 pos)
    {
        PlaySfx(clip, pos);
    }

    public void LogEnemyHit(EnemyHealth eh, bool isAlt)
    {
        if (!logAttacks) return;
        if (eh == null) return;
        string n = (eh.config != null && !string.IsNullOrEmpty(eh.config.displayName)) ? eh.config.displayName : eh.name;
        if (!isAlt)
            Debug.Log($"Hit confirmed on {n}. Enemy HP: {eh.CurrentHP}/{eh.MaxHP}");
        else
            Debug.Log($"ALT hit confirmed on {n}. Enemy HP: {eh.CurrentHP}/{eh.MaxHP}");
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = new Color(0.3f, 0.6f, 1f);
        Gizmos.DrawWireSphere(transform.position, altAttackRange);
    }
}

