using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [Min(1)] public int maxHP = 100;
    [SerializeField] private int currentHP;

    [Header("Feedback (optional)")]
    public bool logDamage = true;
    public AudioSource audioSource;
    public AudioClip hitSfx;
    public AudioClip healSfx;
    [Range(0f, 1f)] public float sfxVolume = 0.9f;

    public event Action<int, int> OnHealthChanged; // current, max
    public event Action OnDied;

    public int CurrentHP => currentHP;
    public bool IsDead => currentHP <= 0;

    void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponentInChildren<AudioSource>();

        // If not set in Inspector, start full HP.
        if (currentHP <= 0) currentHP = maxHP;
        currentHP = Mathf.Clamp(currentHP, 1, maxHP);
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;
        if (amount <= 0) return;

        currentHP = Mathf.Max(0, currentHP - amount);
        OnHealthChanged?.Invoke(currentHP, maxHP);

        if (logDamage)
            Debug.Log($"Player took {amount} damage. HP: {currentHP}/{maxHP}");

        if (CombatTextManager.Instance != null)
            CombatTextManager.Instance.SpawnWorldText(transform.position + Vector3.up * 2f, $"-{amount}", new Color(1f, 0.3f, 0.3f));

        PlaySfx(hitSfx, transform.position);

        if (currentHP == 0)
            Die();
    }

    public bool TryHeal(int amount)
    {
        if (IsDead) return false;
        if (amount <= 0) return false;
        if (currentHP >= maxHP) return false;

        int before = currentHP;
        currentHP = Mathf.Min(maxHP, currentHP + amount);
        int healed = currentHP - before;
        Debug.Log($"Player healed {healed}. HP: {currentHP}/{maxHP}");
        OnHealthChanged?.Invoke(currentHP, maxHP);

        if (CombatTextManager.Instance != null)
            CombatTextManager.Instance.SpawnWorldText(transform.position + Vector3.up * 2f, $"+{healed}", new Color(0.35f, 1f, 0.45f));
        PlaySfx(healSfx, transform.position);
        return healed > 0;
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

    void Die()
    {
        Debug.Log("Player died!");
        OnDied?.Invoke();
        if (GameManager.Instance != null)
            GameManager.Instance.HandlePlayerDied();
    }
}

